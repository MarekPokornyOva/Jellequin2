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

        // Property pages
        public const string JellequinApplicationPropertyPageGuidString = "9AEC2F56-7F7B-4939-887C-A3EA832613E1";
        public static readonly Guid JellequinApplicationPropertyPageGuid = new Guid("{" + JellequinApplicationPropertyPageGuidString + "}");

        /*public const string JellequinBuildPropertyPageGuidString = "396E8C81-6642-4E74-8A44-DA16A6B57B7B";
        public static readonly Guid JellequinBuildPropertyPageGuid = new Guid("{" + JellequinBuildPropertyPageGuidString + "}");

        public const string JellequinDebugPropertyPageGuidString = "9CFF1FD3-FFF7-4A96-AB40-4921BC9A95ED";
        public static readonly Guid JellequinDebugPropertyPageGuid = new Guid("{" + JellequinDebugPropertyPageGuidString + "}");*/
    }
}
/*
 Application
	AssemblyName
	OutputType (exe/library)
	Icon
	Version

Build
	OutputPath
	RuntimeMethodsUsage
	GeneratePdbSymbols

Debug
	CommandLineArguments
*/
