﻿using NJsonSchema;
using System.Collections.Generic;
using System.Linq;

namespace TemplateModels.Base;

public class PropertyTemplateModelBase
{
    public string PropertyName { get; set; }
    public string Type { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsRequired { get; set; }
    public bool HasDescription => !string.IsNullOrEmpty(Description);
    public string Description { get; set; }

    public object Default { get; set; }
    public string DefaultCodeFormat { get; set; }
    public bool HasDefault => Default != null;
    public bool HasVeryLongDefault => HasDefault && (DefaultCodeFormat?.Length > 100);
    public bool IsAnyOf { get; set; }
    public List<JsonSchema> AnyOf { get; set; }

    public bool IsArray { get; set; }
    public PropertyTemplateModelBase(string name, JsonSchemaProperty json): this(name, json, json.IsRequired, json.IsReadOnly)
    {
    }

    public PropertyTemplateModelBase(string name, JsonSchema json, bool isRequired, bool isReadOnly)
    {
        PropertyName = name;
        Default = json.Default;

        Description = json.Description?.Replace("\n", "\\n")?.Replace("\"", "\"\"");

        AnyOf = json.AnyOf?.ToList();
        IsAnyOf = (AnyOf?.Any()).GetValueOrDefault();
        IsReadOnly = isReadOnly;
        IsRequired = isRequired;
        IsArray = json.IsArray;
     
    }

    public override string ToString()
    {
        return PropertyName;
    }
}
