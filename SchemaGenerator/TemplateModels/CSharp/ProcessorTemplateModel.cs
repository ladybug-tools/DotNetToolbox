using NSwag;
using System.Collections.Generic;
using System.Linq;

namespace TemplateModels.CSharp;

public class ProcessorTemplateModel
{
    public static string SDKName { get; set; }
    public string NameSpaceName => SDKName;

    public static string BuildSDKVersion;
    public string SDKVersion => BuildSDKVersion;

    public string InterfaceName => $"I{ClassName}";
    public string ClassName { get; set; }
    public List<MethodTemplateModel> Methods { get; set; }
    public bool HasMethod => Methods.Any();

    public List<string> CsImports { get; set; } = new List<string>();
    public List<string> CsPackages => CsImports;
    public bool HasCsImports => CsPackages.Any();
    public ProcessorTemplateModel(OpenApiDocument doc, string processorName = "Processor")
    {
        ClassName = processorName;
        Methods = doc.Paths.Select(_ => new MethodTemplateModel(_.Key, _.Value))?.Where(_ => !string.IsNullOrEmpty(_.MethodName))?.ToList();
    }
}
