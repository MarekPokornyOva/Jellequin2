namespace Tvl.VisualStudio.Language.Jellequin.Project
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.Project;

    using MSBuild = Microsoft.Build.Evaluation;

    public class JellequinConfigProvider : ConfigProvider
    {
        public const string DisplayAnyCPU = "Any CPU";

        public JellequinConfigProvider(JellequinProjectNode manager)
            : base(manager)
        {
        }

        protected new JellequinProjectNode ProjectManager
        {
            get
            {
                return (JellequinProjectNode)base.ProjectManager;
            }
        }

        protected override ProjectConfig CreateProjectConfiguration(string configName, string platform)
        {
            return new JellequinProjectConfig(this.ProjectManager, configName, platform);
        }

        public override string GetPlatformNameFromPlatformProperty(string platformProperty)
        {
            switch (platformProperty)
            {
            case JellequinProjectFileConstants.AnyCPU:
                return DisplayAnyCPU;

            default:
                return base.GetPlatformNameFromPlatformProperty(platformProperty);
            }
        }

        public override string GetPlatformPropertyFromPlatformName(string platformName)
        {
            switch (platformName)
            {
            case DisplayAnyCPU:
                return JellequinProjectFileConstants.AnyCPU;

            default:
                return base.GetPlatformPropertyFromPlatformName(platformName);
            }
        }

        protected override IEnumerable<MSBuild.Project> GetBuildProjects(bool includeUserBuildProjects = true)
        {
            if (!includeUserBuildProjects || ProjectManager.UserBuildProject == null)
                return base.GetBuildProjects(includeUserBuildProjects);

            return base.GetBuildProjects(false).Concat(new[] { ProjectManager.UserBuildProject });
        }
    }
}
