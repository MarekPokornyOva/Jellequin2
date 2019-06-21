﻿namespace Tvl.VisualStudio.Language.Jellequin.Project
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Project;
    using Tvl.VisualStudio.Shell;

    using __VSDBGLAUNCHFLAGS = Microsoft.VisualStudio.Shell.Interop.__VSDBGLAUNCHFLAGS;
    using __VSDBGLAUNCHFLAGS2 = Microsoft.VisualStudio.Shell.Interop.__VSDBGLAUNCHFLAGS2;
    using _PersistStorageType = Microsoft.VisualStudio.Shell.Interop._PersistStorageType;
    using CommandLineBuilder = Microsoft.Build.Utilities.CommandLineBuilder;
    using DEBUG_LAUNCH_OPERATION = Microsoft.VisualStudio.Shell.Interop.DEBUG_LAUNCH_OPERATION;
    //using DebugAgent = Tvl.VisualStudio.Language.Jellequin.Project.PropertyPages.DebugAgent;
    using Directory = System.IO.Directory;
    using File = System.IO.File;
    using IVsDebugger2 = Microsoft.VisualStudio.Shell.Interop.IVsDebugger2;
    using IVsUIShell = Microsoft.VisualStudio.Shell.Interop.IVsUIShell;
    //using JellequinDebugEngine = Tvl.VisualStudio.Language.Jellequin.Debugger.JellequinDebugEngine;
    using Path = System.IO.Path;
    using RegistryHive = Microsoft.Win32.RegistryHive;
    using RegistryKey = Microsoft.Win32.RegistryKey;
    using RegistryKeyPermissionCheck = Microsoft.Win32.RegistryKeyPermissionCheck;
    using RegistryView = Microsoft.Win32.RegistryView;
    using SecurityException = System.Security.SecurityException;
    using StringComparison = System.StringComparison;
    using SVsShellDebugger = Microsoft.VisualStudio.Shell.Interop.SVsShellDebugger;
    using SVsUIShell = Microsoft.VisualStudio.Shell.Interop.SVsUIShell;

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
        {
            fCanLaunch = 1;
            return VSConstants.S_OK;
        }

        public override int DebugLaunch(uint grfLaunch)
        {
            //DebugTargetInfo info = new DebugTargetInfo();

            //CommandLineBuilder commandLine = new CommandLineBuilder();

            //bool x64 = Platform.EndsWith("X64", StringComparison.OrdinalIgnoreCase) || (Platform.EndsWith("Any CPU", StringComparison.OrdinalIgnoreCase) && Environment.Is64BitOperatingSystem);
            //string agentBaseFileName = "Tvl.Jellequin.DebugHostWrapper";
            //if (x64)
            //    agentBaseFileName += "X64";

            //bool useDevelopmentEnvironment = (grfLaunch & (uint)__VSDBGLAUNCHFLAGS.DBGLAUNCH_NoDebug) == 0;

            //string debugAgentName = GetConfigurationProperty(JavaConfigConstants.DebugAgent, _PersistStorageType.PST_USER_FILE, false);
            //bool useJdwp = string.Equals(DebugAgent.Jdwp.ToString(), debugAgentName, StringComparison.OrdinalIgnoreCase);

            //if (useJdwp)
            //{
            //    commandLine.AppendSwitch("-Xdebug");
            //    string serverValue = useDevelopmentEnvironment ? "y" : "n";
            //    commandLine.AppendSwitch("-Xrunjdwp:transport=dt_socket,server=" + serverValue + ",address=6777");
            //}
            //else
            //{
            //    string agentFolder = Path.GetDirectoryName(typeof(JellequinProjectConfig).Assembly.Location);
            //    string agentFileName = agentBaseFileName + ".dll";
            //    string agentPath = Path.GetFullPath(Path.Combine(agentFolder, agentFileName));
            //    commandLine.AppendSwitchIfNotNull("-agentpath:", agentPath);

            //    string agentArguments = GetConfigurationProperty(JellequinConfigConstants.DebugAgentArguments, _PersistStorageType.PST_USER_FILE, false);
            //    if (!string.IsNullOrEmpty(agentArguments))
            //        commandLine.AppendTextUnquoted("=" + agentArguments);
            //}

            //switch (GetConfigurationProperty(JellequinConfigConstants.DebugStartAction, _PersistStorageType.PST_USER_FILE, false))
            //{
            //case "Class":
            //    string jvmArguments = GetConfigurationProperty(JellequinConfigConstants.DebugJvmArguments, _PersistStorageType.PST_USER_FILE, false);
            //    if (!string.IsNullOrEmpty(jvmArguments))
            //        commandLine.AppendTextUnquoted(" " + jvmArguments);

            //    commandLine.AppendSwitch("-cp");
            //    commandLine.AppendFileNameIfNotNull(GetConfigurationProperty("TargetPath", _PersistStorageType.PST_PROJECT_FILE, false));

            //    string startupObject = GetConfigurationProperty(JellequinConfigConstants.DebugStartClass, _PersistStorageType.PST_USER_FILE, false);
            //    if (!string.IsNullOrEmpty(startupObject))
            //        commandLine.AppendFileNameIfNotNull(startupObject);

            //    break;

            //default:
            //    throw new NotImplementedException("This preview version of the Jellequin debugger only supports starting execution in a named class; the class name may be configured in the project properties on the Debug tab.");
            //}

            //string debugArgs = GetConfigurationProperty(JellequinConfigConstants.DebugExtraArgs, _PersistStorageType.PST_USER_FILE, false);
            //if (!string.IsNullOrEmpty(debugArgs))
            //    commandLine.AppendTextUnquoted(" " + debugArgs);

            //string workingDirectory = GetConfigurationProperty(JellequinConfigConstants.DebugWorkingDirectory, _PersistStorageType.PST_USER_FILE, false);
            //if (string.IsNullOrEmpty(workingDirectory))
            //    workingDirectory = GetConfigurationProperty(JellequinConfigConstants.OutputPath, _PersistStorageType.PST_PROJECT_FILE, false);

            //if (!Path.IsPathRooted(workingDirectory))
            //{
            //    workingDirectory = Path.GetFullPath(Path.Combine(this.ProjectManager.ProjectFolder, workingDirectory));
            //}

            //// Pass the project references via the CLASSPATH environment variable
            ///*List<string> classPathEntries = new List<string>();
            //IReferenceContainer referenceContainer = ProjectManager.GetReferenceContainer();
            //IList<ReferenceNode> references = referenceContainer.EnumReferences();
            //foreach (var referenceNode in references)
            //{
            //    JarReferenceNode jarReferenceNode = referenceNode as JarReferenceNode;
            //    if (jarReferenceNode != null)
            //    {
            //        if (File.Exists(jarReferenceNode.InstalledFilePath) || Directory.Exists(jarReferenceNode.InstalledFilePath))
            //            classPathEntries.Add(jarReferenceNode.InstalledFilePath);
            //    }
            //}*/

            //if (classPathEntries != null)
            //{
            //    string classPath = string.Join(";", classPathEntries);
            //    info.Environment.Add("CLASSPATH", classPath);
            //}

            ////List<string> arguments = new List<string>();
            ////arguments.Add(@"-agentpath:C:\dev\SimpleC\Tvl.Jellequin.DebugHost\bin\Debug\Tvl.Jellequin.DebugHostWrapper.dll");
            //////arguments.Add(@"-verbose:jni");
            //////arguments.Add(@"-cp");
            //////arguments.Add(@"C:\dev\JavaProjectTest\JavaProject\out\Debug");
            ////arguments.Add("tvl.school.ee382v.a3.problem1.program1");
            //////arguments.Add(GetConfigurationProperty("OutputPath", true));
            //////arguments.Add(GetConfigurationProperty(JellequinConfigConstants.DebugStartClass, false, _PersistStorageType.PST_USER_FILE));
            //////arguments.Add(GetConfigurationProperty(JellequinConfigConstants.DebugExtraArgs, false, _PersistStorageType.PST_USER_FILE));

            ////info.Arguments = string.Join(" ", arguments);

            //info.Arguments = commandLine.ToString();

            //info.Executable = FindJavaBinary("Java.exe", useDevelopmentEnvironment);

            ////info.CurrentDirectory = GetConfigurationProperty("WorkingDirectory", false, _PersistStorageType.PST_USER_FILE);
            //info.CurrentDirectory = workingDirectory;
            //info.SendToOutputWindow = false;
            //info.DebugEngines = new Guid[]
            //    {
            //        typeof(JavaDebugEngine).GUID,
            //        //VSConstants.DebugEnginesGuids.ManagedOnly_guid,
            //        //VSConstants.DebugEnginesGuids.NativeOnly_guid,
            //    };
            //Guid localPortSupplier = new Guid("{708C1ECA-FF48-11D2-904F-00C04FA302A1}");
            //info.PortSupplier = localPortSupplier;
            //info.LaunchOperation = DEBUG_LAUNCH_OPERATION.DLO_CreateProcess;
            //info.LaunchFlags = (__VSDBGLAUNCHFLAGS)grfLaunch | (__VSDBGLAUNCHFLAGS)__VSDBGLAUNCHFLAGS2.DBGLAUNCH_MergeEnv;

            //var debugger = (IVsDebugger2)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsShellDebugger));
            //int result = debugger.LaunchDebugTargets(info);

            //if (result != VSConstants.S_OK)
            //{
            //    IVsUIShell uishell = (IVsUIShell)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsUIShell));
            //    string message = uishell.GetErrorInfo();
            //}

            string workingDirectory= this.ProjectManager.BuildProject.Properties
                .FirstOrDefault(x=>string.Equals(x.Name,"IntermediateOutputPath",StringComparison.Ordinal))?.EvaluatedValue
                ??this.ProjectManager.ProjectFolder;

            DebugTargetInfo info = new DebugTargetInfo
            {
                CurrentDirectory= workingDirectory,
                DebugEngines=new[] { VSConstants.DebugEnginesGuids.ManagedOnly_guid },
                LaunchFlags= __VSDBGLAUNCHFLAGS.DBGLAUNCH_DetachOnStop| __VSDBGLAUNCHFLAGS.DBGLAUNCH_StopDebuggingOnEnd,
                LaunchOperation=DEBUG_LAUNCH_OPERATION.DLO_CreateProcess
                //settings.Executable = await debuggerProperties.JellProjDebuggerCommand.GetEvaluatedValueAtEndAsync();
                //settings.Arguments = await debuggerProperties.JellProjDebuggerCommandArguments.GetEvaluatedValueAtEndAsync();
            };

            var debugger = (IVsDebugger2)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsShellDebugger));
            int result = debugger.LaunchDebugTargets(info);
            return result;
        }
    }
}
