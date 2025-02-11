using System;
using System.Collections.Generic;
using System.Linq;
using NJsonSchema;
using NJsonSchema.Generation;
using NSwag;
using Newtonsoft.Json.Linq;
using System.IO;
using TemplateModels.CSharp;
using TemplateModels;

namespace SchemaGenerator;

public class GenCsProcessor : Generator
{
    static string _sdkName => Generator.sdkName;
    static string workingDir => Generator.workingDir;
    static string rootDir => Generator.rootDir;
    internal static void Execute()
    {
        TemplateModels.Helper.Language = TemplateModels.TargetLanguage.CSharp;
        Console.WriteLine($"Current working dir: {workingDir}");
        //Console.WriteLine(string.Join(",", args));

        System.IO.Directory.CreateDirectory(outputDir);


        //var schemaFile = System.IO.Path.Combine(outputDir, "schema.json");
        var jsons = _config.files.Where(_ => !_.Contains("_mapper.json"));


        JObject docJson = null;
        JObject jPaths = null;
        // combine all schema components
        foreach (var j in jsons)
        {
            var schemaFile = System.IO.Path.Combine(docDir, j);
            var json = System.IO.File.ReadAllText(schemaFile, System.Text.Encoding.UTF8);
            Console.WriteLine($"Reading schema from {schemaFile}");
            var jDocObj = JObject.Parse(json);
            var paths = jDocObj["paths"] as JObject;

            if (docJson == null)
            {
                docJson = jDocObj;
                jPaths = paths;
                continue;
            }

            jPaths.Merge(paths, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });

        }


        docJson["paths"] = jPaths;
        var doc = OpenApiDocument.FromJsonAsync(docJson.ToString()).Result;
     
        // template
        var templateDir = System.IO.Path.Combine(Generator.templateDir, TemplateModels.Helper.Language.ToString());

        var classModels = new List<ClassTemplateModel>();
        var srcDir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(rootDir), "src", _sdkName, "Path");
        Directory.CreateDirectory(srcDir);

        var m = new ProcessorTemplateModel(doc, "MessageProcessor");
        ProcessorTemplateModel.SDKName = _sdkName;

        // generate processor interface
        var file = GenProcessorInterface(templateDir, m, outputDir);
        // copy to src dir
        var targetSrcTs = System.IO.Path.Combine(srcDir, System.IO.Path.GetFileName(file));
        System.IO.File.Copy(file, targetSrcTs, true);
        Console.WriteLine($"Generated {m.InterfaceName} is added as {targetSrcTs}");


        // generate processor class
        var classfile = GenProcessorClass(templateDir, m, outputDir);
        // copy to src dir
        var targetSrcClass = System.IO.Path.Combine(srcDir, System.IO.Path.GetFileName(classfile));
        System.IO.File.Copy(classfile, targetSrcClass, true);
        Console.WriteLine($"Generated {m.ClassName} is added as {targetSrcClass}");

    }

    private static string GenProcessorClass(string templateDir, ProcessorTemplateModel model, string outputDir, string fileExt = ".cs")
    {
        var templateSource = File.ReadAllText(Path.Combine(templateDir, "MessageProcessor.liquid"), System.Text.Encoding.UTF8);
        var code = Gen(templateSource, model);
        var file = System.IO.Path.Combine(outputDir, $"{model.ClassName}{fileExt}");
        System.IO.File.WriteAllText(file, code, System.Text.Encoding.UTF8);
        return file;
    }


    private static string GenProcessorInterface(string templateDir, ProcessorTemplateModel model, string outputDir)
    {
        var templateSource = File.ReadAllText(Path.Combine(templateDir, "Interface.Processor.liquid"), System.Text.Encoding.UTF8);
        var code = Gen(templateSource, model);
        var file = System.IO.Path.Combine(outputDir, $"{model.InterfaceName}.cs");
        System.IO.File.WriteAllText(file, code, System.Text.Encoding.UTF8);
        return file;
    }
}




