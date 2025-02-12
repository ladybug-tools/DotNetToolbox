using NSwag;
using System.Collections.Generic;
using System.Linq;

namespace TemplateModels.TypeScript;

public class ProcessorTemplateModel
{
    public static string SDKName { get; set; }
    public string NameSpaceName => SDKName;
    public string InterfaceName => $"I{ClassName}";
    public string ClassName { get; set; }
    public List<MethodTemplateModel> Methods { get; set; }
    public List<TsImport> TsImports { get; set; } = new List<TsImport>();
    public bool HasTsImports => TsImports.Any();

    public ProcessorTemplateModel(OpenApiDocument doc, string processorName = "Processor")
    {
        ClassName = processorName;
        Methods = doc.Paths.Select(_=> new MethodTemplateModel(_.Key, _.Value))?.Where(_=>!string.IsNullOrEmpty(_.MethodName))?.ToList();
        TsImports = Methods?.SelectMany(_ => _.TsImports)?.Distinct().Select(_ => new TsImport(_, from: "./models"))?.ToList() ?? new List<TsImport>();
    }
}
