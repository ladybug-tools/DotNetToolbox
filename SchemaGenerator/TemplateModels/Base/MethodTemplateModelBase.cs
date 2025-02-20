using NSwag;
using System.Collections.Generic;
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

    public string ReturnTypeName { get; set; } 
    // void or type name
    //public PropertyTemplateModelBase ReturnType { get; set; }
    //public List<PropertyTemplateModelBase> Params { get; set; }

    protected List<(NJsonSchema.JsonSchema schema, bool required, string name)> ParamSchemas { get; } = new List<(NJsonSchema.JsonSchema schema, bool required, string name)>();
    public MethodTemplateModelBase(string pathName, OpenApiPathItem openApi)
    {
        var operationName = openApi?.FirstOrDefault().Value?.OperationId;
        operationName = string.IsNullOrEmpty(operationName) ? pathName: operationName;

        this.MethodName = Helper.CleanMethodName(operationName);
        var operation = openApi.First().Value;
        this.Summary = operation.Summary;
        this.Document = operation.Description;

        // all reference and non-reference type parameters
        if (operation?.ActualParameters != null && operation.ActualParameters.Any())
            ParamSchemas = operation.ActualParameters
                .Select(_ => (_.Schema, _.IsRequired, name: _.Kind == OpenApiParameterKind.Body ? _?.Schema?.Title: _.Name))
                .ToList();


    }
    public override string ToString()
    {
        return this.MethodName;
    }

}
