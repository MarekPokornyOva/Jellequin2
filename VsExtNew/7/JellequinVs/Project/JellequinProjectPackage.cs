namespace Tvl.VisualStudio.Language.Jellequin.Project
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Project;
    using Microsoft.VisualStudio.Shell;
    using VSConstants = Microsoft.VisualStudio.VSConstants;
    using IVsComponentSelectorProvider = Microsoft.VisualStudio.Shell.Interop.IVsComponentSelectorProvider;
    using VSPROPSHEETPAGE = Microsoft.VisualStudio.Shell.Interop.VSPROPSHEETPAGE;

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

    [ProvideObject(typeof(PropertyPages.JellequinApplicationPropertyPage))]
    //[ProvideObject(typeof(PropertyPages.JellequinBuildPropertyPage))]
    //[ProvideObject(typeof(PropertyPages.JellequinDebugPropertyPage))]

    public class JellequinProjectPackage : ProjectPackage, IVsComponentSelectorProvider
    {
        public override string ProductUserContext => "Jellequin";

        protected override void Initialize()
        {
            base.Initialize();

            RegisterProjectFactory(new JellequinProjectFactory(this));
        }


        #region IVsComponentSelectorProvider Members

        public int GetComponentSelectorPage(ref Guid rguidPage, VSPROPSHEETPAGE[] ppage)
        {
            if (ppage == null)
                throw new ArgumentNullException("ppage");
            if (ppage.Length == 0)
                throw new ArgumentException();

            return VSConstants.E_INVALIDARG;
        }

        #endregion
    }
}
