using NSwag;
using NJsonSchema;
using TemplateModels.CSharp;

namespace Generator.Tests.CSharp
{
    public class ProjectInfomationTests
    {
        static OpenApiDocument doc = null;
        [SetUp]
        public void Setup()
        {

            var jsonFile = Path.Combine(TestHelper.HoneybeeDir, "project-information_inheritance.json");

            var json = File.ReadAllText(jsonFile);
            doc = OpenApiDocument.FromJsonAsync(json).Result;

        }



        [Test]
        public void TestGenerateClass()
        {
            var json = doc.Components.Schemas["_OpenAPIGenBaseModel"];

            var classModel = new ClassTemplateModel(doc, json);
            Assert.That(classModel.HasInheritance, Is.False);
            Assert.That(classModel.HasDerivedClasses, Is.True);
            //var prop = json.ActualProperties.FirstOrDefault();

        }

        [Test]
        public void TestMinimumMaximum()
        {
            var json = doc.Components.Schemas["ProjectInfo"];

            var classModel = new ClassTemplateModel(doc, json);
            var prop = classModel.Properties.First(_ => _.CsPropertyName == "North");
            Assert.IsNotNull(prop);

            Assert.That(prop.HasMaximum, Is.True);

        }

    }
}