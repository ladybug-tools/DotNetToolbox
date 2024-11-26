using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using Fluid;

namespace SchemaGenerator;

public partial class Generator
{
    private static readonly string _generatorFolder = ".generator";
    private static readonly string _toolFolder = "SchemaGenerator";
    protected static Config _config;
    public static string sdkName => _config.sdkName; //"DragonflySchema";
    public static string moduleName => _config.moduleName; // "dragonfly_schema";

    public static string workingDir = Environment.CurrentDirectory;
    public static string rootDir => workingDir.Substring(0, workingDir.IndexOf(_generatorFolder) + _generatorFolder.Length);

    public static string docDir => System.IO.Path.Combine(System.IO.Path.GetDirectoryName(rootDir), ".openapi-docs");
    public static string defaultConfigPath = Path.Combine(docDir, "config.json");
    public static string outputDir => System.IO.Path.Combine(rootDir, _toolFolder, "Output");
    public static string templateDir => System.IO.Path.Combine(rootDir, _toolFolder, "Templates");

    static void Main(string[] args)
    {

        Console.WriteLine($"Current working dir: {workingDir}");
        Console.WriteLine(string.Join(",", args));

        if (!System.IO.Directory.Exists(rootDir))
            throw new ArgumentException($"Invalid {rootDir}");
        Console.WriteLine($"Current root dir: {rootDir}");

        System.IO.Directory.CreateDirectory(outputDir);

        var supportedArgs = new string[] { "--download", "--genTsModel", "--genCsModel", "--genCsProcessor", "--genCsInterface", "--updateVersion", "--config" };
        if (args == null || !args.Any())
            args = supportedArgs;

        if (args.Where(_ => _.StartsWith("--")).Any(_ => !supportedArgs.Contains(_)))
            throw new ArgumentException($"Only following arguments are supported: {string.Join(",", supportedArgs)}");


        // get config.json
        var argList = args.ToList();
        var configIndex = argList.IndexOf("--config");
        var configPath = defaultConfigPath;
        if (configIndex >= 0 && !string.IsNullOrEmpty(argList.ElementAtOrDefault(configIndex + 1)))
        {
            var p = System.IO.Path.GetFullPath(argList.ElementAtOrDefault(configIndex + 1));
            if (System.IO.Path.GetExtension(p).ToLower() == ".json")
            {
                configPath = p;
            }
        }

        // set configs
        var configJson = File.ReadAllText(configPath);
        _config = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(configJson);


        // download all json files
        if (args.Contains("--download"))
        {
            HttpHelper.SetUp();
            GetSchemaJsonFiles();
        }

        if (args.Contains("--genTsModel"))
        {
            GenTsDTO.Execute();
        }

        if (args.Contains("--genCsModel"))
        {
            GenCsDTO.Execute();
        }

        if (args.Contains("--genCsProcessor"))
        {
            GenCsProcessor.Execute();
        }

        //Generate Interfaces
        if (args.Contains("--genCsInterface"))
        {
            GenInterface.Execute();
        }

        if (args.Contains("--updateVersion"))
            UpdateVersions();

    }


    // Download all schema jsons
    static void GetSchemaJsonFiles()
    {
        var baseURL = _config.baseURL;
        var files = _config.files;

        var dir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(rootDir), ".openapi-docs");
        if (System.IO.Directory.Exists(dir))
        {
            var oldFiles = System.IO.Directory.GetFiles(dir).Where(_ => !_.Contains("config.json"));
            foreach (var file in oldFiles)
            {
                System.IO.File.Delete(file);
            }
        }
        System.IO.Directory.CreateDirectory(dir);
        foreach (var f in files)
        {
            var url = $"{baseURL}/{f}";
            var saveAs = System.IO.Path.Combine(dir, f);
            var savedAs = HttpHelper.DownloadFile(url, saveAs);
            if (!System.IO.File.Exists(savedAs))
                throw new ArgumentException($"Failed to rename file to {saveAs}");
        }

    }

    public static void UpdateVersions()
    {
        // get the current version from model_inheritance.json
        var root = System.IO.Path.GetDirectoryName(rootDir);
        var docDir = System.IO.Path.Combine(root, ".openapi-docs");
        var jsonFile = System.IO.Path.Combine(docDir, "model_inheritance.json");
        var modelJson = System.IO.File.ReadAllText(jsonFile);
        var newVersion = JObject.Parse(modelJson)["info"]["version"].ToString();
        newVersion = string.IsNullOrEmpty(newVersion) ? "1.0" : newVersion;
        Console.WriteLine($"Found document version: {newVersion}");

        // 1.58.3.1 => 1.5803.1
        //1.5.8 => 1.508
        //1.12.8 => 1.1208
        //1.12.12 => 1.1212
        //1.13.9 => 1.1309
        var digits = newVersion.Split(new[] { '.' }).ToList();
        if (digits.Count < 3)
            digits.Add("00");
        if (digits.Count == 3)
            digits.Add("0");

        var secondD = digits[1];
        var thridD = digits[2];
        thridD = thridD.Length == 1 ? $"0{thridD}" : thridD;
        var fourthD = digits[3];

        var baseVersion = $"{digits.First()}.{secondD}{thridD}";
        newVersion = $"{baseVersion}.{fourthD}";
        Console.WriteLine($"Re-formated document version: {newVersion}");

        var packageName = sdkName;

        // Check the version from nuget
        var api = $"https://api.nuget.org/v3-flatcontainer/{sdkName.ToLower()}/index.json";
        var versions = (HttpHelper.ReadJson(api)["versions"] as JArray)?.Select(_ => _.ToString())?.ToList();
        var increaseVersion = false;
        if (versions != null && versions.Any())
        {
            versions.Sort(new VersionComparer());
            var lastVersion = versions.Last().ToString();
            Console.WriteLine($"Found latest version on Nuget: {lastVersion}");
            increaseVersion = lastVersion.StartsWith(baseVersion);
            if (increaseVersion)
                newVersion = lastVersion;
            else
                Console.WriteLine($"Schema version {newVersion} is newer than the latest version on Nuget: {lastVersion}");
        }

        Console.WriteLine($"Getting the existing version: {newVersion}");
        if (increaseVersion)
        {
            digits = newVersion.Split(new[] { '.' }).ToList();
            if (digits.Count == 3)
            {
                var lastV = digits.LastOrDefault().Replace("v", "");
                var v = int.Parse(lastV) + 1;
                newVersion = string.Join(".", digits.SkipLast(1)) + $".{v}";
            }
            else
            {
                throw new ArgumentException("Unexpected version!!");
            }
        }

        Console.WriteLine($"New version: {newVersion}");


        //# update the version for CSharp
        var assemblyFile = System.IO.Path.Combine(root, "src", packageName, $"{packageName}.csproj");
        var file = System.IO.File.ReadAllText(assemblyFile);
        var newFile = Regex.Replace(file, @"(?<=\SVersion\>)\S+(?=\<\/)", newVersion);
        System.IO.File.WriteAllText(assemblyFile, newFile, Encoding.UTF8);


        //# update the version for TypeScript
        var tsFile = System.IO.Path.Combine(root, "src", "TypescriptSDK", "package.json");
        file = System.IO.File.ReadAllText(tsFile, Encoding.UTF8);
        newFile = Regex.Replace(file, @"(?<=version"": "")[^""]+(?="")", newVersion);
        System.IO.File.WriteAllText(tsFile, newFile, new UTF8Encoding(false));

    }


    private static TemplateOptions _TsTemplateOptions;
    private static TemplateOptions _CsTemplateOptions;
    private static TemplateOptions GetTemplateOptions(TemplateModels.TargetLanguage language)
    {
        switch (language)
        {
            case TemplateModels.TargetLanguage.CSharp:
                _CsTemplateOptions = _CsTemplateOptions ?? getOptions();
                return _CsTemplateOptions;
                break;
            case TemplateModels.TargetLanguage.TypeScript:
                _TsTemplateOptions = _TsTemplateOptions ?? getOptions();
                return _TsTemplateOptions;
                break;
            default:
                throw new ArgumentException($"Non-supported {language}");
                break;
        }

        TemplateOptions getOptions()
        {
            var options = new TemplateOptions();
            var tps = typeof(Generator).Assembly
                .GetTypes()
                .Where(_ => _.IsPublic)
                .Where(t => t.Namespace.StartsWith("TemplateModels.Base") || t.Namespace.StartsWith($"TemplateModels.{language}"))
                .ToList();

            foreach (var item in tps)
            {
                options.MemberAccessStrategy.Register(item);
            }

            options.Greedy = false;
            return options;
        }

    }

    public static string Gen(string templateSource, object model)
    {
        var parser = new FluidParser();
        if (parser.TryParse(templateSource, out var template, out var error))
        {
            var context = new TemplateContext(model, GetTemplateOptions(TemplateModels.Helper.Language));
            var code = template.Render(context);
            return code;
        }
        else
        {
            return $"Error: {error}";
        }
    }

    public static Mapper ReadMapper(string mapperFilePath)
    {
        if (!System.IO.File.Exists(mapperFilePath) || !mapperFilePath.EndsWith("_mapper.json"))
            return null;
        Console.WriteLine($"Reading mapper from: {mapperFilePath}");
        var mapperJson = System.IO.File.ReadAllText(mapperFilePath);
        var data = JObject.Parse(mapperJson);
        var classItems = (data["classes"] as JObject).Properties();
        var enumItems = (data["enums"] as JObject).Properties();


        var cls = classItems?.Select(_ => new MapperItem(_.Name, _.Value.ToString()))?.ToList();
        var enms = enumItems?.Select(_ => new MapperItem(_.Name, _.Value.ToString()))?.ToList();

        var mapper = new Mapper(cls, enms);
        return mapper;
    }



}


public class VersionComparer : IComparer<string>
{
    public int Compare(string x, string y)
    {
        if (x == null) return y == null ? 0 : -1;
        if (y == null) return 1;

        var regex = new Regex(@"(\d+|\D+)");
        var xParts = regex.Matches(x);
        var yParts = regex.Matches(y);

        int maxLength = Math.Max(xParts.Count, yParts.Count);
        for (int i = 0; i < maxLength; i++)
        {
            if (i >= xParts.Count) return -1;
            if (i >= yParts.Count) return 1;

            var xPart = xParts[i].Value;
            var yPart = yParts[i].Value;

            int result;
            if (int.TryParse(xPart, out int xNum) && int.TryParse(yPart, out int yNum))
            {
                result = xNum.CompareTo(yNum);
            }
            else
            {
                result = string.Compare(xPart, yPart, StringComparison.Ordinal);
            }

            if (result != 0) return result;
        }

        return 0;
    }
}