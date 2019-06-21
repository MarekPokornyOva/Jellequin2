namespace Tvl.VisualStudio.Language.Jellequin.Project
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Project;
    using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;
    using vsCommandStatus = EnvDTE.vsCommandStatus;
    using VSConstants = Microsoft.VisualStudio.VSConstants;
    using VsMenus = Microsoft.VisualStudio.Shell.VsMenus;

    [ComVisible(true)]
    public class JellequinFileNode : FileNode
    {
        public JellequinFileNode(ProjectNode root, ProjectElement element)
            : base(root, element)
        {
        }

        protected override NodeProperties CreatePropertiesObject()
        {
            return new JellequinFileNodeProperties(this);
        }

        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref vsCommandStatus result)
        {
            if (cmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                switch ((VsCommands2K)cmd)
                {
                case VsCommands2K.INCLUDEINPROJECT:
                case VsCommands2K.EXCLUDEFROMPROJECT:
                    result = vsCommandStatus.vsCommandStatusUnsupported;
                    return VSConstants.S_OK;

                default:
                    break;
                }
            }

            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }
    }
}
