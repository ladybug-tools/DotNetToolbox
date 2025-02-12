using NSwag;
using NJsonSchema;
using NJsonSchema.CodeGeneration;
using SchemaGenerator;
using TemplateModels.TypeScript;

namespace Generator.Tests.TypeScript
{
    public class MessageProcessorTests
    {

        static OpenApiDocument doc = null;
        [SetUp]
        public void Setup()
        {
            var jsonFile = Path.Combine(TestHelper.ModelEditorRevitSdkDir, "openapi_inheritance.json");
            doc = GenProcessorBase.GetDoc(new[] {jsonFile});

        }


        [Test]
        public void TestParameterName()
        {
            var model = new TemplateModels.TypeScript.ProcessorTemplateModel(doc, "MessageProcessor");
            var imports = model.TsImports.GroupBy(x => x.From);
            foreach (var import in imports) {
                var classes = import.Select(_ => _.Name);
                Assert.That(classes.Distinct().Count(), Is.EqualTo(classes.Count()));
            }

        }

    }
}