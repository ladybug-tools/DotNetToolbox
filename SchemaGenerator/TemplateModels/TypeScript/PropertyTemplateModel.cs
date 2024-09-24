
using TemplateModels.Base;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics;

namespace TemplateModels.TypeScript;

public class PropertyTemplateModel : PropertyTemplateModelBase
{


    public string ConvertToJavaScriptCode { get; set; } // for TS: property value to JS object
    public string ConvertToClassCode { get; set; } // for TS: JSON object to class property



    public List<string> TsImports { get; set; } = new List<string>();
    public bool HasTsImports => TsImports.Any();



    public bool HasValidationDecorators => ValidationDecorators.Any();
    public List<string> ValidationDecorators { get; set; }

    public bool HasTransformDecorator => !string.IsNullOrWhiteSpace(TransformDecorator);
    public string TransformDecorator {  get; set; }

    public PropertyTemplateModel(string name, JsonSchemaProperty json) : base(name, json)
    {

        DefaultCodeFormat = ConvertTsDefaultValue(json);

        // check types
        Type = GetTypeScriptType(json, AddTsImportTypes);

        ConvertToJavaScriptCode = $"data[\"{PropertyName}\"] = this.{PropertyName};";
        ConvertToClassCode = $"this.{PropertyName} = obj.{PropertyName};";
        //validation decorators
        ValidationDecorators = GetValidationDecorators(json);

        // get @Transform
        TransformDecorator = GetTransform(json, false);

    }
    public static string GetTransform(JsonSchema json, bool isArray)
    {
        var code = string.Empty;
        if ((json.AnyOf?.Any()).GetValueOrDefault())
        {
            var allRefs = json.AnyOf.All(_ => _.HasReference);
            if (!allRefs)
                return code;

            var refTypes = json.AnyOf.Select(r => r.ActualSchema.Title).ToList();

            var tps = refTypes.Select(_ => $"if (item?.type === '{_}') return {_}.fromJS(item);").ToList();
            tps = tps.Take(1).Concat(tps.Skip(1).Select(_ => $"else {_}")).ToList();
            tps.Add("else return item;");
            tps = tps.Select(_ => $"      {_}").ToList();

            var trans = string.Join(Environment.NewLine, tps);
            code = $@"@Transform(({{ value }}) => {{
      const item = value;
{trans}
    }})";
            return isArray ? trans : code;
        }
        else if (json.IsArray)
        {
            var arrayItem = json.Item;
            var itemCode = GetTransform(arrayItem, true);
            if (string.IsNullOrEmpty(itemCode))
                return code;
            code = $@"@Transform(({{ value }}) => value.map((item: any) => {{
{itemCode}
    }}))";
            return code;
        }
        else
        {
            return code;
        }

    }


    public static string GetTypeScriptType(JsonSchema json, Action<string> collectImportType)
    {
        var type = string.Empty;
        if ((json.AnyOf?.Any()).GetValueOrDefault())
        {
            var types = GetAnyOfTypes(json, collectImportType);
            var tps = types.Select(_ => ConvertToTypeScriptType(_)).ToList();
            type = $"({string.Join(" | ", tps)})";
        }
        else if (json.IsArray)
        {
            var arrayItem = json.Item;
            var itemType = GetTypeScriptType(arrayItem, collectImportType);
            type = $"{ConvertToTypeScriptType(itemType)} []";
        }
        else
        {
            var propType = json.Type.ToString();
            if (json.HasReference)
            {
                propType = HandleReferenceType(json, collectImportType);
            }
            type = ConvertToTypeScriptType(propType);
        }

        return type;
    }

    public static List<string> GetAnyOfTypes(JsonSchema json, Action<string> collectImportType)
    {
        var types = new List<string>();
        var anyof = json.AnyOf;
        foreach (var item in anyof)
        {
            var typeName = HandleType(item, collectImportType);
            types.Add(typeName);
        }

        return types;
    }


    private static string HandleType(JsonSchema json, Action<string> collectImportType)
    {
        var type = string.Empty;
        if (json.HasReference)
        {
            type = HandleReferenceType(json, collectImportType);
        }
        else
        {
            type = json.Type.ToString();
        }
        return type;
    }

    private static string HandleReferenceType(JsonSchema json, Action<string> collectImportType)
    {
        var typeName = json.ActualSchema.Title;
        collectImportType(typeName);
        return typeName;
    }


    public static string ConvertToTypeScriptType(string type)
    {
        return TsTypeMapper.TryGetValue(type, out var result) ? result : type;
    }

    public static Dictionary<string, string> TsTypeMapper = new Dictionary<string, string>
    {
        {"String", "string" },
        {"Integer", "number" },
        {"Number", "number" },
        {"Boolean", "boolean" }
    };

    private static List<string> GetValidationDecorators(JsonSchema json, bool isArrayItem)
    {
        var result = new List<string>();
        if (json.IsArray)
        {
            var arrayItem = json.Item;
            var decos = GetValidationDecorators(arrayItem, true);

            result.Add("@IsArray({ each: true })"); // Ensures each item in the array is also an array.
            result.Add("@ValidateNested({each: true })");// Ensures each item in the array is validated as a nested object.
            result.Add($"@Type(() => Array)"); //Helps class-transformer understand that each item in the array should be treated as an array.

            result.AddRange(decos);

            return result;
        }


        var propType = json.Type.ToString();
        if (json.HasReference)
        {
            var refPropType = json.ActualSchema.Title;
            if (json.ActualSchema.IsEnumeration)
            {
                result.Add(isArrayItem ? $"@IsEnum({refPropType}, {{ each: true }})" : $"@IsEnum({refPropType})");
                result.Add($"@Type(() => String)");
            }
            else
            {
                result.Add(isArrayItem ? $"@IsInstance({refPropType}, {{ each: true }})" : $"@IsInstance({refPropType})");
                result.Add($"@Type(() => {refPropType})");
                result.Add(isArrayItem ? "@ValidateNested({ each: true })" : "@ValidateNested()");
            }

        }
        else if (propType == "Integer")
        {
            result.Add(isArrayItem ? "@IsInt({ each: true })" : "@IsInt()");
        }
        else if (propType == "Number")
        {
            result.Add(isArrayItem ? "@IsNumber({},{ each: true })" : "@IsNumber()");
        }
        else if (propType == "String")
        {
            result.Add(isArrayItem ? "@IsString({ each: true })" : "@IsString()");
        }
        else if (propType == "Boolean")
        {
            result.Add(isArrayItem ? "@IsBoolean({ each: true })" : "@IsBoolean()");
        }
        else
        {
            //result.Add($"@IsObject()");
        }

        return result;
    }
    public static List<string> GetValidationDecorators(JsonSchemaProperty json)
    {
        var result = new List<string>();
        if (json.IsArray)
        {
            result.Add("@IsArray()");
            var arrayItem = json.Item;
            var decos = GetValidationDecorators(arrayItem, isArrayItem: true);

            if (arrayItem.IsArray)
            {
                var IsNestedNumberArray = decos.Any(_ => _.StartsWith("@IsNumber"));
                var IsNestedIntegerArray = decos.Any(_ => _.StartsWith("@IsInt"));
                var IsNestedStringArray = decos.Any(_ => _.StartsWith("@IsString"));

                if (IsNestedNumberArray)
                    result.Add("@IsNestedNumberArray()"); // for number[][] or number[][][] types
                else if (IsNestedIntegerArray)
                    result.Add("@IsNestedIntegerArray()"); // for number[][] or number[][][] types
                else if (IsNestedStringArray)
                    result.Add("@IsNestedStringArray()"); // for number[][] or number[][][] types
                else
                    result.AddRange(decos);
            }
            else
            {
                result.AddRange(decos);
            }
          

        }
        else
        {
            var decos = GetValidationDecorators(json, isArrayItem: false);
            result.AddRange(decos);
        }

        if (json.IsRequired)
        {
            result.Add($"@IsDefined()");
        }
        else
        {
            result.Add($"@IsOptional()");
        }


        if (!string.IsNullOrEmpty(json.Pattern))
        {
            result.Add($"@Matches(/{json.Pattern}/)");
        }
        if (json.Minimum.HasValue)
        {
            result.Add($"@Min({json.Minimum})");
        }
        if (json.Maximum.HasValue)
        {
            result.Add($"@Max({json.Maximum})");
        }
        if (json.MinLength.HasValue)
        {
            result.Add($"@MinLength({json.MinLength})");
        }
        if (json.MaxLength.HasValue)
        {
            result.Add($"@MaxLength({json.MaxLength})");
        }

        return result;

    }

    public void AddTsImportTypes(string type)
    {
        if (type == Type)
            return;
        TsImports.Add(type);
    }

    private static string ConvertTsDefaultValue(JsonSchemaProperty prop)
    {
        var defaultValue = prop.Default;
        var defaultCodeFormat = string.Empty;
        if (defaultValue == null) return defaultCodeFormat;

        if (defaultValue is string)
        {
            defaultCodeFormat = $"\"{defaultValue}\"";
            if (prop.ActualSchema.IsEnumeration)
            {
                var enumType = prop.ActualSchema.Title;
                defaultCodeFormat = $"{enumType}.{defaultValue}";
            }

        }
        else if (defaultValue is Newtonsoft.Json.Linq.JToken jToken)
        {
            defaultCodeFormat = GetDefaultFromJson(jToken);
        }
        else if (prop.Type.ToString() == "Boolean")
        {
            defaultCodeFormat = defaultValue.ToString().ToLower();
        }
        else
        {
            defaultCodeFormat = defaultValue?.ToString();
        }

        return defaultCodeFormat;
    }

    private static string GetDefaultFromJson(JToken jContainer)
    {
        var defaultCodeFormat = string.Empty;
        if (jContainer is Newtonsoft.Json.Linq.JObject jObj)
        {
            if (jObj.TryGetValue("type", out var vType))
            {
                var isFullJsonObj = jObj.Values().Count() > 1;
                defaultCodeFormat = isFullJsonObj ? $"{vType}.fromJS({jObj})" : $"new {vType}()";
            }
            else
            {
                defaultCodeFormat = jContainer.ToString();
            }
        }
        else if (jContainer is Newtonsoft.Json.Linq.JArray jArray)
        {
            var arrayCode = new List<string>();
            var separator = $", ";
            foreach (var item in jArray)
            {
                arrayCode.Add(GetDefaultFromJson(item).ToString());
            }
            defaultCodeFormat = $"[{string.Join(separator, arrayCode)}]";
        }
        else
        {
            defaultCodeFormat = jContainer.ToString();
        }

        return defaultCodeFormat;
    }
}
