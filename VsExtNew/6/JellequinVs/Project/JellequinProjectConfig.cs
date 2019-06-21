namespace Tvl.VisualStudio.Language.Jellequin.Project
{
    #region using
    using System.Diagnostics.Contracts;
    using JellequinVs.Debug;
    using Microsoft.VisualStudio.Project;
    #endregion using

    public class JellequinProjectConfig : ProjectConfig
    {
        internal JellequinProjectConfig(JellequinProjectNode project, string configuration, string platform)
            : base(project, configuration, platform)
        {
            Contract.Requires(project != null);
            Contract.Requires(!string.IsNullOrEmpty(configuration));
            Contract.Requires(!string.IsNullOrEmpty(platform));
        }

        public new JellequinProjectNode ProjectManager
        {
            get
            {
                Contract.Ensures(Contract.Result<JellequinProjectNode>() != null);
                return (JellequinProjectNode)base.ProjectManager;
            }
        }

        public override void Invalidate()
        {
            base.Invalidate();
        }

        public override int QueryDebugLaunch(uint flags, out int fCanLaunch)
             => DebugLauncher.QueryDebugLaunch(flags, out fCanLaunch);

        public override int DebugLaunch(uint grfLaunch)
            => DebugLauncher.DebugLaunch(grfLaunch, this.ProjectManager);
    }
}
