﻿using NSwag;
using SchemaGenerator;
using System.Collections.Generic;
using System.Linq;

namespace TemplateModels.TypeScript;

public class ProcessorTemplateModel
{
    public static string SDKName { get; set; }
    public string NameSpaceName => SDKName;

    public static string BuildSDKVersion;
    public string SDKVersion => BuildSDKVersion;
    public string InterfaceName => $"I{ClassName}";
    public string ClassName { get; set; }
    public List<MethodTemplateModel> Methods { get; set; }
    public List<TsImport> TsImports { get; set; } = new List<TsImport>();
    public bool HasTsImports => TsImports.Any();

    public ProcessorTemplateModel(OpenApiDocument doc, string processorName = "Processor", Mapper mapper = default)
    {

        mapper = mapper ?? new Mapper(null, null);

        ClassName = processorName;
        Methods = doc.Paths.Select(_=> new MethodTemplateModel(_.Key, _.Value))?.Where(_=>!string.IsNullOrEmpty(_.MethodName))?.ToList();
        var tsImports = Methods?.SelectMany(_ => _.TsImports)?.Distinct().Select(_ => new TsImport(_, from: mapper.TryGetModule(_)))?.ToList() ?? new List<TsImport>();
        // remove importing self
        tsImports = tsImports.Where(_ => _.Name != ClassName).ToList();
        // remove duplicates
        TsImports = tsImports.GroupBy(_ => _.Name).Select(_ => _.First()).OrderBy(_ => _.Name).ToList();

        // fix TsImports
        TsImports.ForEach(_ => _.Check());

        // add models to import path
        TsImports.ForEach(_ => {
            if (_.From.StartsWith("./"))
                _.From = $"./models/{_.From.Substring(2)}";
        });
    }
}
