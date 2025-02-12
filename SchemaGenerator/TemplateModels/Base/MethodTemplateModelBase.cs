using NSwag;
using System.Linq;

namespace TemplateModels.Base;

public class MethodTemplateModelBase
{
    public string MethodName { get; set; }
    public bool HasReturn { get; set; }
    public bool HasParameter { get; set; }

    public string ReturnDoc { get; set; }

    public string Summary { get; set; }
    public string Document { get; set; }

    public string ReturnTypeName { get; set; } // void or type name
    //public PropertyTemplateModelBase ReturnType { get; set; }
    //public List<PropertyTemplateModelBase> Params { get; set; }

    public MethodTemplateModelBase(string name, OpenApiPathItem openApi)
    {
        this.MethodName = Helper.CleanMethodName(name);
        var operation = openApi.First().Value;
        this.Summary = operation.Summary;
        this.Document = operation.Description;


    }

}
