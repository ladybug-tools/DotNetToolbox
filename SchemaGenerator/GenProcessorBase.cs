using System;
using System.Linq;
using NSwag;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
//using TemplateModels.CSharp;

namespace SchemaGenerator;

public class GenProcessorBase : Generator
{
    internal static string _sdkName => Generator.sdkName;
    internal static string _version => Generator.BuildVersion;
    internal static string workingDir => Generator.workingDir;
    internal static string rootDir => Generator.rootDir;
    internal static OpenApiDocument GetDoc(out Mapper mapper)
    {
        Console.WriteLine($"Current working dir: {workingDir}");
        //Console.WriteLine(string.Join(",", args));

        System.IO.Directory.CreateDirectory(outputDir);

        //var schemaFile = System.IO.Path.Combine(outputDir, "schema.json");
        var jsons = _config.files.Where(_ => !_.Contains("_mapper.json"));

        OpenApiDocument doc = GetDoc(jsons, out mapper);
        return doc;

    }

    public static OpenApiDocument GetDoc(IEnumerable<string> jsons, out Mapper mapper)
    {
        JObject docJson = null;
        JObject jPaths = null;

        Mapper docMapper = null;
        // combine all schema components
        foreach (var j in jsons)
        {
            var schemaFile = System.IO.Path.Combine(docDir, j);
            var json = System.IO.File.ReadAllText(schemaFile, System.Text.Encoding.UTF8);
            Console.WriteLine($"Reading schema from {schemaFile}");
            var jDocObj = JObject.Parse(json);
            var paths = jDocObj["paths"] as JObject;

            // read mapper
            var mapperFile = System.IO.Path.Combine(docDir, j.Replace("_inheritance", "_mapper"));
            var mp = ReadMapper(mapperFile);

            if (docJson == null)
            {
                docJson = jDocObj;
                jPaths = paths;
                docMapper = mp;
                continue;
            }

            jPaths.Merge(paths, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });

            docMapper.Merge(mp);

        }

        mapper = docMapper;
        docJson["paths"] = jPaths;
        var doc = OpenApiDocument.FromJsonAsync(docJson.ToString()).Result;
        return doc;
    }
}




