using System;
using System.Collections.Generic;
using NJsonSchema;
using NJsonSchema.Generation;
using System.IO;
//using TemplateModels.CSharp;
using TemplateModels;

namespace SchemaGenerator;

public class GenCsProcessor : GenProcessorBase
{
    internal static void Execute()
    {
        TemplateModels.Helper.Language = TemplateModels.TargetLanguage.CSharp;

        var doc = GetDoc();
        // template
        var templateDir = System.IO.Path.Combine(Generator.templateDir, TemplateModels.Helper.Language.ToString());

        var srcDir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(rootDir), "src", _sdkName, "Path");
        Directory.CreateDirectory(srcDir);

        var m = new TemplateModels.CSharp.ProcessorTemplateModel(doc, "MessageProcessor");
        TemplateModels.CSharp.ProcessorTemplateModel.SDKName = _sdkName;

        // generate processor interface
        var file = GenProcessorInterface(templateDir, m, outputDir);
        // copy to src dir
        var targetSrcTs = System.IO.Path.Combine(srcDir, System.IO.Path.GetFileName(file));
        System.IO.File.Copy(file, targetSrcTs, true);
        Console.WriteLine($"Generated {m.InterfaceName} is added as {targetSrcTs}");


        // generate processor class
        var classfile = GenProcessorClass(templateDir, m, outputDir);
        // copy to src dir
        var targetSrcClass = System.IO.Path.Combine(srcDir, System.IO.Path.GetFileName(classfile));
        System.IO.File.Copy(classfile, targetSrcClass, true);
        Console.WriteLine($"Generated {m.ClassName} is added as {targetSrcClass}");
    }


    private static string GenProcessorClass(string templateDir, TemplateModels.CSharp.ProcessorTemplateModel model, string outputDir, string fileExt = ".cs")
    {
        var templateSource = File.ReadAllText(Path.Combine(templateDir, "MessageProcessor.liquid"), System.Text.Encoding.UTF8);
        var code = Gen(templateSource, model);
        var file = System.IO.Path.Combine(outputDir, $"{model.ClassName}{fileExt}");
        System.IO.File.WriteAllText(file, code, System.Text.Encoding.UTF8);
        return file;
    }


    private static string GenProcessorInterface(string templateDir, TemplateModels.CSharp.ProcessorTemplateModel model, string outputDir)
    {
        var templateSource = File.ReadAllText(Path.Combine(templateDir, "Interface.Processor.liquid"), System.Text.Encoding.UTF8);
        var code = Gen(templateSource, model);
        var file = System.IO.Path.Combine(outputDir, $"{model.InterfaceName}.cs");
        System.IO.File.WriteAllText(file, code, System.Text.Encoding.UTF8);
        return file;
    }
}




