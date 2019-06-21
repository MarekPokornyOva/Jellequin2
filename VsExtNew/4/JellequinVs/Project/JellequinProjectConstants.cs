namespace Tvl.VisualStudio.Language.Jellequin.Project
{
    using Guid = System.Guid;

    public static class JellequinProjectConstants
    {
        public const int ProjectResourceId = 200;
        public const string ProjectPackageNameResourceString = "#210";
        public const string ProjectPackageDetailsResourceString = "#211";
        public const string ProjectPackageProductVersionString = "1.0";

        public const string ProjectPackageGuidString = "E9AB1381-6EDF-4F80-A342-8005A678B89C";
        public static readonly Guid ProjectPackageGuid = new Guid("{" + ProjectPackageGuidString + "}");

        public const string ProjectFactoryGuidString = "BE0A4C86-FC82-4F70-8ED6-D24C8B13E7C0";
        public static readonly Guid ProjectGuid = new Guid("{" + ProjectFactoryGuidString + "}");
    }
}
