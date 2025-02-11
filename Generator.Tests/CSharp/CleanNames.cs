using NUnit.Framework.Internal;

namespace Generator.Tests.CSharp
{
    public class CleanNames
    {
        [Test]
        public void TestModelEditorRevitSDKParameterNames()
        {
            //parameter name RoomExportByIdSetting
            var clean = TemplateModels.Helper.CleanParameterName("RoomExportByIdSetting");
            Assert.That(clean, Is.EqualTo("roomExportByIdSetting"));

        }

        [Test]
        public void TestModelEditorRevitSDKMethodNames()
        {
            var clean = TemplateModels.Helper.CleanMethodName("/export-selected/");
            Assert.That(clean, Is.EqualTo("ExportSelected"));

        }
    }
}
