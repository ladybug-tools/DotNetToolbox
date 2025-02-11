using NSwag;
using System.Collections.Generic;
using System.Linq;

namespace TemplateModels.CSharp;

public class ProcessorTemplateModel
{
    public static string SDKName { get; set; }
    public string NameSpaceName => SDKName;
    public string InterfaceName => $"I{ClassName}";
    public string ClassName { get; set; }
    public List<OperationTemplateModel> Methods { get; set; }
    public ProcessorTemplateModel(OpenApiDocument doc, string processorName = "Processor")
    {
        ClassName = processorName;
        Methods = doc.Paths.Select(_=> new OperationTemplateModel(_.Key, _.Value))?.Where(_=>!string.IsNullOrEmpty(_.MethodName))?.ToList();
    }
}
