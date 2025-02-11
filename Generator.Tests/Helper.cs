namespace Generator.Tests
{
    public static class Helper
    {
        static string _dll = typeof(Helper).Assembly.Location;
        static string _projectDir = _dll.Substring(0, _dll.LastIndexOf("bin"));

        public static string ProjectDir => _projectDir;
        public static string SampleDir => Path.Combine(_projectDir, "Samples");
        public static string HoneybeeDir => Path.Combine(SampleDir, "HoneybeeSchema");
        public static string DragonflyDir => Path.Combine(SampleDir, "DragonflySchema");
        public static string ModelEditorRevitSdkDir => Path.Combine(SampleDir, "ModelEditorRevitSDK");


    }
}
