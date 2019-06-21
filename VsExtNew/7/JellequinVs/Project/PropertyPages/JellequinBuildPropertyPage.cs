namespace Tvl.VisualStudio.Language.Jellequin.Project.PropertyPages
{
    using System;
    using System.Runtime.InteropServices;
    using _PersistStorageType = Microsoft.VisualStudio.Shell.Interop._PersistStorageType;

    [ComVisible(true)]
    [Guid(JellequinProjectConstants.JellequinBuildPropertyPageGuidString)]
    public class JellequinBuildPropertyPage : JellequinPropertyPage
    {
        public JellequinBuildPropertyPage()
        {
            PageName = JellequinConfigConstants.PageNameBuild;
        }

        public new JellequinBuildPropertyPagePanel PropertyPagePanel
        {
            get
            {
                return (JellequinBuildPropertyPagePanel)base.PropertyPagePanel;
            }
        }

        protected override void BindProperties()
        {
            if (ProjectManager != null)
                ProjectManager.SharedBuildOptions.Build = PropertyPagePanel;

            // general
            PropertyPagePanel.SourceRelease = GetConfigProperty(JellequinConfigConstants.SourceRelease, _PersistStorageType.PST_PROJECT_FILE);
            PropertyPagePanel.TargetRelease = GetConfigProperty(JellequinConfigConstants.TargetRelease, _PersistStorageType.PST_PROJECT_FILE);
            PropertyPagePanel.Encoding = GetConfigProperty(JellequinConfigConstants.SourceEncoding, _PersistStorageType.PST_PROJECT_FILE);

            // debugging
            DebuggingInformation info;
            if (!Enum.TryParse(GetConfigProperty(JellequinConfigConstants.DebugSymbols, _PersistStorageType.PST_PROJECT_FILE), out info))
                info = DebuggingInformation.Default;

            PropertyPagePanel.DebuggingInformation = info;
            PropertyPagePanel.SpecificDebuggingInformation = GetConfigProperty(JellequinConfigConstants.SpecificDebugSymbols, _PersistStorageType.PST_PROJECT_FILE);

            // warnings
            PropertyPagePanel.ShowWarnings = GetConfigPropertyBoolean(JellequinConfigConstants.ShowWarnings, _PersistStorageType.PST_PROJECT_FILE);
            PropertyPagePanel.ShowAllWarnings = GetConfigPropertyBoolean(JellequinConfigConstants.ShowAllWarnings, _PersistStorageType.PST_PROJECT_FILE);

            // warnings as errors
            WarningsAsErrors warnAsError;
            if (!Enum.TryParse(GetConfigProperty(JellequinConfigConstants.TreatWarningsAsErrors, _PersistStorageType.PST_PROJECT_FILE), out warnAsError))
                warnAsError = WarningsAsErrors.None;

            PropertyPagePanel.WarningsAsErrors = warnAsError;
            PropertyPagePanel.SpecificWarningsAsErrors = GetConfigProperty(JellequinConfigConstants.WarningsAsErrors, _PersistStorageType.PST_PROJECT_FILE);

            // output
            PropertyPagePanel.OutputPath = GetConfigProperty(JellequinConfigConstants.OutputPath, _PersistStorageType.PST_PROJECT_FILE);

            // extra arguments
            PropertyPagePanel.ExtraArguments = GetConfigProperty(JellequinConfigConstants.BuildArgs, _PersistStorageType.PST_PROJECT_FILE);
        }

        protected override bool ApplyChanges()
        {
            // general
            SetConfigProperty(JellequinConfigConstants.SourceRelease, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.SourceRelease);
            SetConfigProperty(JellequinConfigConstants.TargetRelease, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.TargetRelease);
            SetConfigProperty(JellequinConfigConstants.SourceEncoding, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.Encoding);

            // debugging
            SetConfigProperty(JellequinConfigConstants.DebugSymbols, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.DebuggingInformation.ToString());
            SetConfigProperty(JellequinConfigConstants.SpecificDebugSymbols, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.SpecificDebuggingInformation);

            // warnings
            SetConfigProperty(JellequinConfigConstants.ShowWarnings, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.ShowWarnings);
            SetConfigProperty(JellequinConfigConstants.ShowAllWarnings, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.ShowAllWarnings);

            // warnings as errors
            SetConfigProperty(JellequinConfigConstants.TreatWarningsAsErrors, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.WarningsAsErrors.ToString());
            SetConfigProperty(JellequinConfigConstants.WarningsAsErrors, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.SpecificWarningsAsErrors);

            // output
            SetConfigProperty(JellequinConfigConstants.OutputPath, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.OutputPath);

            // extra arguments
            SetConfigProperty(JellequinConfigConstants.BuildArgs, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.ExtraArguments);

            return true;
        }

        protected override JellequinPropertyPagePanel CreatePropertyPagePanel()
        {
            return new JellequinBuildPropertyPagePanel(this);
        }
    }
}
