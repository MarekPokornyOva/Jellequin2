namespace Tvl.VisualStudio.Language.Jellequin.Project
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Project;
    using Microsoft.VisualStudio.Shell;
    using VSConstants = Microsoft.VisualStudio.VSConstants;

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration(JellequinProjectConstants.ProjectPackageNameResourceString, JellequinProjectConstants.ProjectPackageDetailsResourceString, JellequinProjectConstants.ProjectPackageProductVersionString/*, IconResourceID = 400*/)]
    [Guid(JellequinProjectConstants.ProjectPackageGuidString)]
    [ProvideProjectFactory(
        typeof(JellequinProjectFactory),
        "Jellequin",
        "Jellequin Project Files (*.jellproj);*.jellproj",
        "jellproj",
        "jellproj",
        "ProjectTemplates",
        LanguageVsTemplate = Constants.JellequinLanguageName,
        NewProjectRequireNewFolderVsTemplate = false)]

    public class JellequinProjectPackage : ProjectPackage
    {
        public override string ProductUserContext => "Jellequin";

        protected override void Initialize()
        {
            base.Initialize();

            RegisterProjectFactory(new JellequinProjectFactory(this));
        }
    }
}
