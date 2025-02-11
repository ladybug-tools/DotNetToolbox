using NUnit.Framework.Internal;

namespace Generator.Tests.CSharp
{
    public class CleanNames
    {
        [Test]
        public void TestModelEditorRevitSDKParameterNames()
        {
            //parameter name RoomExportByIdSetting
            var clean = TemplateModels.Helper.CleanName(TemplateModels.Helper.ToCamelCase("RoomExportByIdSetting"));
            Assert.That(clean, Is.EqualTo("roomExportByIdSetting"));

        }
    }
}
