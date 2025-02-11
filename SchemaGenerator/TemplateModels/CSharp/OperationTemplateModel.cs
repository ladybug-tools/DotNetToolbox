using Newtonsoft.Json.Linq;
using NSwag;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TemplateModels.Base;

namespace TemplateModels.CSharp;

public class OperationTemplateModel
{
    public string MethodName { get; set; }
    public bool HasReturn { get; set; }
    public bool HasParameter { get; set; }

    public string ReturnDoc { get; set; }

    public string Summary { get; set; }
    public string Document { get; set; }

    public string ReturnTypeName { get; set; } // void or type name
    public PropertyTemplateModel ReturnType { get; set; }
    public List<PropertyTemplateModel> Params { get; set; }

    public OperationTemplateModel(string name, OpenApiPathItem openApi)
    {
        this.MethodName = Helper.CleanMethodName(name);
        var operation = openApi.First().Value;
        this.Summary = operation.Summary;
        this.Document = operation.Description;

        var requestBody = operation.RequestBody;
        Params = requestBody?.Content?
            .Select(_ => _.Value.Schema)
            .Select(_ => new PropertyTemplateModel(_.Title, _, requestBody.IsRequired, false))?
            .ToList();
        HasParameter = (Params?.Any()).GetValueOrDefault();

        var returnObj = operation.Responses["200"]?.Content?.FirstOrDefault().Value?.Schema; // Successful Response
        ReturnType = new PropertyTemplateModel(name, returnObj, false, false);

        if (returnObj != null)
        {
            ReturnTypeName = ReturnType.Type;
        }

        if (string.IsNullOrEmpty(ReturnTypeName) || ReturnTypeName == "None")
        {
            ReturnTypeName = "void";
        }

        HasReturn = ReturnTypeName != "void";

    }

}
