namespace Tvl.VisualStudio.Language.Jellequin.Project.PropertyPages
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Project;
    using Tvl.Collections;
    using _PersistStorageType = Microsoft.VisualStudio.Shell.Interop._PersistStorageType;

    [ComVisible(true)]
    [Guid(JellequinProjectConstants.JellequinApplicationPropertyPageGuidString)]
    public class JellequinApplicationPropertyPage : JellequinPropertyPage
    {
        private static readonly string NotSetStartupObject = string.Empty;

        static ImmutableList<string> _defaultAvailableOutputTypes = new ImmutableList<string>(new[] { "Console Application", "Library" });

        public JellequinApplicationPropertyPage()
        {
            PageName = "Application";
        }

        public new JellequinApplicationPropertyPagePanel PropertyPagePanel
            => (JellequinApplicationPropertyPagePanel)base.PropertyPagePanel;

        protected override JellequinPropertyPagePanel CreatePropertyPagePanel()
            => new JellequinApplicationPropertyPagePanel(this);

        protected override void BindProperties()
        {
            PropertyPagePanel.AssemblyName = GetConfigProperty(ProjectFileConstants.AssemblyName, _PersistStorageType.PST_PROJECT_FILE);
            PropertyPagePanel.AvailableOutputTypes = _defaultAvailableOutputTypes;
            PropertyPagePanel.OutputType = GetConfigProperty(ProjectFileConstants.OutputType, _PersistStorageType.PST_PROJECT_FILE);
        }

        protected override bool ApplyChanges()
        {
            SetConfigProperty(ProjectFileConstants.AssemblyName, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.AssemblyName);
            SetConfigProperty(ProjectFileConstants.OutputType, _PersistStorageType.PST_PROJECT_FILE, PropertyPagePanel.OutputType);
            return true;
        }
    }
}
