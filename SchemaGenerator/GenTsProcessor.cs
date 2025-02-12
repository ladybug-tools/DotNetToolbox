using System;
using System.IO;
//using TemplateModels.CSharp;

namespace SchemaGenerator;

public class GenTsProcessor : GenProcessorBase
{
    internal static void Execute()
    {
        TemplateModels.Helper.Language = TemplateModels.TargetLanguage.TypeScript;

        var doc = GetDoc();
        // template
        var templateDir = System.IO.Path.Combine(Generator.templateDir, TemplateModels.Helper.Language.ToString());

        //var classModels = new List<TemplateModels.TypeScript.ClassTemplateModel>();
        var srcDir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(rootDir), "src", $"{TemplateModels.Helper.Language}SDK", "src");
        Directory.CreateDirectory(srcDir);

        var m = new TemplateModels.TypeScript.ProcessorTemplateModel(doc, "MessageProcessor");
        TemplateModels.TypeScript.ProcessorTemplateModel.SDKName = _sdkName;

        // generate processor class
        var classfile = GenProcessorClass(templateDir, m, outputDir);
        // copy to src dir
        var targetSrcClass = System.IO.Path.Combine(srcDir, System.IO.Path.GetFileName(classfile));
        System.IO.File.Copy(classfile, targetSrcClass, true);
        Console.WriteLine($"Generated {m.ClassName} is added as {targetSrcClass}");
    }


    private static string GenProcessorClass(string templateDir, TemplateModels.TypeScript.ProcessorTemplateModel model, string outputDir, string fileExt = ".ts")
    {
        var templateSource = File.ReadAllText(Path.Combine(templateDir, "MessageProcessor.liquid"), System.Text.Encoding.UTF8);
        var code = Gen(templateSource, model);
        var file = System.IO.Path.Combine(outputDir, $"{model.ClassName}{fileExt}");
        System.IO.File.WriteAllText(file, code, System.Text.Encoding.UTF8);
        return file;
    }
}




