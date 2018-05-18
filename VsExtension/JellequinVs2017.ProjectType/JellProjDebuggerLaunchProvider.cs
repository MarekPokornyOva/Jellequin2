using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.ProjectSystem.VS;
using Microsoft.VisualStudio.ProjectSystem.VS.Debug;

namespace JellequinVs2017
{
	[ExportDebugger(JellProjDebugger.SchemaName)]
	[AppliesTo(MyUnconfiguredProject.UniqueCapability)]
	public class JellProjDebuggerLaunchProvider : DebugLaunchProviderBase
	{
		// TODO: Specify the assembly full name here
		[ExportPropertyXamlRuleDefinition("JellequinVs2017, Version=1.0.0.0, Culture=neutral, PublicKeyToken=9be6e469bc4921f1", "XamlRuleToCode:JellProjDebugger.xaml", "Project")]
		[AppliesTo(MyUnconfiguredProject.UniqueCapability)]
		private object DebuggerXaml { get { throw new NotImplementedException(); } }

		[ImportingConstructor]
		public JellProjDebuggerLaunchProvider(ConfiguredProject configuredProject)
			 : base(configuredProject)
		{
		}

		ProjectProperties _DebuggerProperties;
		/// <summary>
		/// Gets project properties that the debugger needs to launch.
		/// </summary>
		[Import]
		private ProjectProperties DebuggerProperties
		{
			//get;
			//set;
			get { return _DebuggerProperties; }
			set { _DebuggerProperties = value; }
		}

		public override async Task<bool> CanLaunchAsync(DebugLaunchOptions launchOptions)
		{
			var properties = await this.DebuggerProperties.GetJellProjDebuggerPropertiesAsync();
			string commandValue = await properties.JellProjDebuggerCommand.GetEvaluatedValueAtEndAsync();
			return !string.IsNullOrEmpty(commandValue);
		}

		public override async Task<IReadOnlyList<IDebugLaunchSettings>> QueryDebugTargetsAsync(DebugLaunchOptions launchOptions)
		{
			var settings = new DebugLaunchSettings(launchOptions);

			// The properties that are available via DebuggerProperties are determined by the property XAML files in your project.
			var debuggerProperties = await this.DebuggerProperties.GetJellProjDebuggerPropertiesAsync();
			settings.CurrentDirectory = await debuggerProperties.JellProjDebuggerWorkingDirectory.GetEvaluatedValueAtEndAsync();
			settings.Executable = await debuggerProperties.JellProjDebuggerCommand.GetEvaluatedValueAtEndAsync();
			settings.Arguments = await debuggerProperties.JellProjDebuggerCommandArguments.GetEvaluatedValueAtEndAsync();
			settings.LaunchOperation = DebugLaunchOperation.CreateProcess;

			// TODO: Specify the right debugger engine
			settings.LaunchDebugEngineGuid = DebuggerEngines.ManagedOnlyEngine;

			return new IDebugLaunchSettings[] { settings };
		}
	}
}
