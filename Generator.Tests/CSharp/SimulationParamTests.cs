using NSwag;
using NJsonSchema;
using TemplateModels.CSharp;

namespace Generator.Tests.CSharp
{
    public class SimulationParamTests
    {

        static OpenApiDocument doc = null;
        [SetUp]
        public void Setup()
        {

            var jsonFile = Path.Combine(Helper.HoneybeeDir, "simulation-parameter_inheritance.json");

            var json = File.ReadAllText(jsonFile);
            doc = OpenApiDocument.FromJsonAsync(json).Result;

        }



        [Test]
        public void TestDefaultValueForListProperty()
        {
            var json = doc.Components.Schemas["RunPeriod"];
            var classModel = new ClassTemplateModel(doc, json);
            var prop = classModel.Properties.First(_ => _.CsPropertyName == "StartDate");

            StringAssert.AreEqualIgnoringCase("(new []{1, 1}).ToList()", prop.DefaultCodeFormat);
        }


    }
}