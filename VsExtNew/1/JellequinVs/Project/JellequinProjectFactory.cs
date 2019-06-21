namespace Tvl.VisualStudio.Language.Jellequin.Project
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Project;
    using Tvl.VisualStudio.Shell;

    using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

    [Guid(JellequinProjectConstants.ProjectFactoryGuidString)]
    public class JellequinProjectFactory : ProjectFactory
    {
        internal JellequinProjectFactory(JellequinProjectPackage package)
            : base(package)
        {
        }

        public new JellequinProjectPackage Package => (JellequinProjectPackage)base.Package;

        protected override ProjectNode CreateProject()
        {
            JellequinProjectNode node = new JellequinProjectNode(Package);
            IOleServiceProvider serviceProvider = base.Package.GetService<IOleServiceProvider>();
            node.SetSite(serviceProvider);
            return node;
        }
    }
}
