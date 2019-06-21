namespace Tvl.VisualStudio.Language.Jellequin.Project.PropertyPages
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using Tvl.Collections;

    public partial class JellequinApplicationPropertyPagePanel : JellequinPropertyPagePanel
    {
        private static readonly ImmutableList<string> _emptyList = new ImmutableList<string>(new string[0]);

        private ImmutableList<string> _availableOutputTypes = _emptyList;

        public JellequinApplicationPropertyPagePanel()
            : this(null)
        {
        }

        public JellequinApplicationPropertyPagePanel(JellequinApplicationPropertyPage parentPropertyPage)
            : base(parentPropertyPage)
        {
            InitializeComponent();
        }

        internal new JellequinApplicationPropertyPage ParentPropertyPage
        {
            get
            {
                return (JellequinApplicationPropertyPage)base.ParentPropertyPage;
            }
        }

        public ImmutableList<string> AvailableOutputTypes
        {
            get
            {
                Contract.Ensures(Contract.Result<ImmutableList<string>>() != null);

                return _availableOutputTypes;
            }

            set
            {
                Contract.Requires<ArgumentNullException>(value != null, "value");
                Contract.Requires<ArgumentException>(Contract.ForAll(value, i => !string.IsNullOrEmpty(i)));

                if (_availableOutputTypes.SequenceEqual(value, StringComparer.CurrentCulture))
                    return;

                _availableOutputTypes = value;
                cmbOutputType.Items.Clear();
                cmbOutputType.Items.AddRange(value.ToArray());
            }
        }

        public string AssemblyName
        {
            get
            {
                return txtAssemblyName.Text;
            }

            set
            {
                txtAssemblyName.Text = value ?? string.Empty;
            }
        }

        public string OutputType
        {
            get
            {
                Contract.Ensures(Contract.Result<string>() != null);

                if (cmbOutputType.SelectedItem == null)
                    return string.Empty;

                return cmbOutputType.SelectedItem.ToString();
            }
            set
            {
                Contract.Requires<ArgumentNullException>(value != null, "value");

                cmbOutputType.SelectedItem = value;
            }
        }

        private void HandleBuildSettingChanged(object sender, EventArgs e)
        {
            ParentPropertyPage.IsDirty = true;
        }
    }
}
