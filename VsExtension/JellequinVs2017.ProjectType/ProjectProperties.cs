namespace JellequinVs2017
{
    using System.Composition;
    using Microsoft.VisualStudio.ProjectSystem;
    using Microsoft.VisualStudio.ProjectSystem.Properties;

	//https://github.com/Microsoft/VSProjectSystem
	//https://github.com/Microsoft/VSProjectSystem/blob/master/doc/extensibility/property_pages.md
	//https://blogs.msdn.microsoft.com/vsproject/2009/06/09/platform-extensibility-part-1/
	//https://blogs.msdn.microsoft.com/vsproject/2009/06/18/platform-extensibility-part-2/
    [Export]
    internal partial class ProjectProperties : StronglyTypedPropertyAccess
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectProperties"/> class.
        /// </summary>
        [ImportingConstructor]
        public ProjectProperties(ConfiguredProject configuredProject)
            : base(configuredProject)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectProperties"/> class.
        /// </summary>
        public ProjectProperties(ConfiguredProject configuredProject, string file, string itemType, string itemName)
            : base(configuredProject, file, itemType, itemName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectProperties"/> class.
        /// </summary>
        public ProjectProperties(ConfiguredProject configuredProject, IProjectPropertiesContext projectPropertiesContext)
            : base(configuredProject, projectPropertiesContext)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectProperties"/> class.
        /// </summary>
        public ProjectProperties(ConfiguredProject configuredProject, UnconfiguredProject unconfiguredProject)
            : base(configuredProject, unconfiguredProject)
        {
        }
    }
}
