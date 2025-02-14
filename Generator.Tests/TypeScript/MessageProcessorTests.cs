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
        static ProcessorTemplateModel model = null;
        [SetUp]
        public void Setup()
        {
            var jsonFile = Path.Combine(TestHelper.ModelEditorRevitSdkDir, "openapi_inheritance.json");
            doc = GenProcessorBase.GetDoc(new[] {jsonFile});
            model = new TemplateModels.TypeScript.ProcessorTemplateModel(doc, "MessageProcessor");

        }


        [Test]
        public void TestTsImpports()
        {
            var imports = model.TsImports.GroupBy(x => x.From);
            foreach (var import in imports) {
                var classes = import.Select(_ => _.Name);
                Assert.That(classes.Distinct().Count(), Is.EqualTo(classes.Count()));
            }

        }


        [Test]
        public void TestParameterNames()
        {
            // ensure all parameter names are matching on both TS and CS side
            var csModel = new TemplateModels.CSharp.ProcessorTemplateModel(doc, "MessageProcessor");
            Assert.That(model.Methods.Count, Is.EqualTo(csModel.Methods.Count));

            var mds = model.Methods.Zip(csModel.Methods, (ts, cs) => new { ts, cs });

            foreach (var md in mds)
            {
                var ts = md.ts;
                var cs = md.cs;
                Assert.That(ts.Params.Count, Is.EqualTo(cs.Params.Count));

                var pms = ts.Params.Zip(cs.Params, (ts, cs) => new { ts, cs });
                foreach(var p in pms)
                {
                    var tsp = p.ts;
                    var csp = p.cs;
                    Assert.That(tsp.TsParameterName, Is.EqualTo(csp.CsParameterName));
                }
            }

        }

        [Test]
        public void TestGetLevelElementCount()
        {
            var json = doc.Paths["/get-level-element-count/"];
            var m = new MethodTemplateModel("/get-level-element-count/", json);
            Assert.That(m.MethodName, Is.EqualTo("GetLevelElementCount"));
            var parameters = m.Params;
            var p = parameters.FirstOrDefault();
            Assert.That(p.PropertyName, Is.EqualTo("level_uid"));
            Assert.That(p.TsPropertyName, Is.EqualTo("LevelUid"));
            Assert.That(p.TsParameterName, Is.EqualTo("levelUid"));
            Assert.That(p.Type, Is.EqualTo("string"));

        }

        [Test]
        public void TestExportByIds()
        {
            var json = doc.Paths["/export-by-ids/"];
            var m = new MethodTemplateModel("/export-by-ids/", json);
            Assert.That(m.MethodName, Is.EqualTo("ExportByIds"));

            var parameters = m.Params;
            var p = parameters.FirstOrDefault();
            Assert.That(p.PropertyName, Is.EqualTo("RoomExportByIdSetting"));
            Assert.That(p.TsPropertyName, Is.EqualTo("RoomExportByIdSetting"));
            Assert.That(p.TsParameterName, Is.EqualTo("roomExportByIdSetting"));
            Assert.That(p.Type, Is.EqualTo("RoomExportByIdSetting"));


        }
    }
}