using Newtonsoft.Json.Linq;
using NSwag;
using SchemaGenerator;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TemplateModels.Base;

namespace TemplateModels.TypeScript;

public class MethodTemplateModel: MethodTemplateModelBase
{
    public PropertyTemplateModel ReturnType { get; set; }
    public List<PropertyTemplateModel> Params { get; set; }
    public List<string> SchemaTypes { get; set; }

    public List<string> TsImports { get; set; } = new List<string>();

    public MethodTemplateModel(string name, OpenApiPathItem openApi):base(name, openApi)
    {
        var operation = openApi.First().Value;

        var requestBody = operation.RequestBody;
        Params = requestBody?.Content?
            .Select(_ => _.Value.Schema)
            .Select(_ => new PropertyTemplateModel(_.Title, _, requestBody.IsRequired, false))?
            .ToList();
        HasParameter = (Params?.Any()).GetValueOrDefault();
        var allTsImports = Params?.SelectMany(_ => _.TsImports)?.ToList() ?? new List<string>();

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
        // collect all tsImports including return type
        if (HasReturn)
            allTsImports.AddRange(ReturnType.TsImports);
        TsImports = allTsImports;


    }

}
