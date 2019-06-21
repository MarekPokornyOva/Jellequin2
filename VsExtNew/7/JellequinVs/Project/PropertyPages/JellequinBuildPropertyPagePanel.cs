namespace Tvl.VisualStudio.Language.Jellequin.Project.PropertyPages
{
    using System;

    public partial class JellequinBuildPropertyPagePanel : JellequinPropertyPagePanel
    {
        public JellequinBuildPropertyPagePanel()
            : this(null)
        {
        }

        public JellequinBuildPropertyPagePanel(JellequinPropertyPage parentPropertyPage)
            : base(parentPropertyPage)
        {
            InitializeComponent();
        }

        public new JellequinBuildPropertyPage ParentPropertyPage
            => base.ParentPropertyPage as JellequinBuildPropertyPage;

        public string SourceRelease
        {
            get
            {
                return (cmbSourceRelease.SelectedItem ?? "").ToString();
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                    value = "Default";

                cmbSourceRelease.SelectedItem = value;
            }
        }

        public bool DebuggingInformation
        {
            get { return btnDebugInfo.Checked; }
            set { btnDebugInfo.Checked = value; }
        }

        public string OutputPath
        {
            get
            {
                return txtOutputPath.Text;
            }

            set
            {
                txtOutputPath.Text = value;
            }
        }

        private void HandleStateAffectingChange(object sender, EventArgs e)
        {
            ParentPropertyPage.IsDirty = true;
        }

        private void HandleCommandLineAffectingChange(object sender, EventArgs e)
        {
            ParentPropertyPage.IsDirty = true;
        }
    }
}
