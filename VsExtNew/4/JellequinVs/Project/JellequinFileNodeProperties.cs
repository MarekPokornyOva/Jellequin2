namespace Tvl.VisualStudio.Language.Jellequin.Project
{
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Project;
    using prjBuildAction = VSLangProj.prjBuildAction;

    [ComVisible(true)]
    public class JellequinFileNodeProperties : FileNodeProperties
    {
        public JellequinFileNodeProperties(JellequinFileNode node)
            : base(node)
        {
        }

        [Browsable(false)]
        public override prjBuildAction BuildAction
        {
            get
            {
                return base.BuildAction;
            }

            set
            {
                base.BuildAction = value;
            }
        }

        public override CopyToOutputDirectoryBehavior CopyToOutputDirectory
        {
            get
            {
                return base.CopyToOutputDirectory;
            }

            set
            {
                if (Node.ItemNode.IsVirtual && value != CopyToOutputDirectoryBehavior.DoNotCopy)
                {
                    Node.ItemNode = Node.ProjectManager.AddFileToMSBuild(Node.VirtualNodeName, ProjectFileConstants.Content, null);
                }

                base.CopyToOutputDirectory = value;
            }
        }
    }
}
