using NSwag;
using NJsonSchema;
using NJsonSchema.CodeGeneration;
using SchemaGenerator;
using TemplateModels.TypeScript;

namespace Generator.Tests.TypeScript
{
    public class ValidationTests
    {
        static OpenApiDocument doc = null;
        [SetUp]
        public void Setup()
        {
            var jsonFile = Path.Combine(TestHelper.HoneybeeDir, "validation-report.json");

            var json = File.ReadAllText(jsonFile);
            doc = OpenApiDocument.FromJsonAsync(json).Result;

        }


        [Test]
        public void TestValidationReport()
        {
            var json = doc.Components.Schemas["ValidationReport"];

            var vr = new ClassTemplateModel(doc, json);
            Assert.That(vr, Is.Not.Null);
            Assert.That(vr.IsAbstract, Is.False);

        }

    }
}