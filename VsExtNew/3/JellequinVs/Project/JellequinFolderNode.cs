namespace Tvl.VisualStudio.Language.Jellequin.Project
{
    using System.Diagnostics.Contracts;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Project;

    [ComVisible(true)]
    public class JellequinFolderNode : FolderNode
    {
        public JellequinFolderNode(ProjectNode root, string relativePath, ProjectElement element)
            : base(root, relativePath, element)
        {
            Contract.Requires(root != null);
            Contract.Requires(relativePath != null);
            Contract.Requires(element != null);

            if (element.IsVirtual)
            {
                string buildAction = element.GetMetadata(ProjectFileConstants.BuildAction);
                if (buildAction == ProjectFileConstants.Folder)
                    this.IsNonmemberItem = false;
            }
        }

        public new JellequinProjectNode ProjectManager
        {
            get
            {
                Contract.Ensures(Contract.Result<JellequinProjectNode>() != null);

                return (JellequinProjectNode)base.ProjectManager;
            }
        }

        public override object GetIconHandle(bool open)
        {
            if (this.IsNonmemberItem)
                return base.GetIconHandle(open);

            if (string.Equals(ItemNode.ItemName, JellequinProjectFileConstants.SourceFolder))
                return this.ProjectManager.ExtendedImageHandler.GetIconHandle(open ? (int)JellequinProjectNode.ExtendedImageName.OpenSourceFolder : (int)JellequinProjectNode.ExtendedImageName.SourceFolder);

            return base.GetIconHandle(open);
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new JellequinFolderNodeProperties(this);
        }
    }
}
