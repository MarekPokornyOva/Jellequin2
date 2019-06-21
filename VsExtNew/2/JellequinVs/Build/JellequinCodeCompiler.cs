using Jellequin.Compiler;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JellequinVs.Build
{
    public class JellequinCodeCompiler : ICodeCompiler
    {
        #region from dom
        public CompilerResults CompileAssemblyFromDom(CompilerParameters options, System.CodeDom.CodeCompileUnit compilationUnit)
        {
            throw new NotImplementedException();
        }

        public CompilerResults CompileAssemblyFromDomBatch(CompilerParameters options, System.CodeDom.CodeCompileUnit[] compilationUnits)
        {
            throw new NotImplementedException();
        }
        #endregion from dom

        #region from file
        public CompilerResults CompileAssemblyFromFile(CompilerParameters options, string fileName)
        {
            if (options == null)
                throw new ArgumentNullException("options");
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            return Compile(options, new FileSource(fileName), fileName);
        }

        public CompilerResults CompileAssemblyFromFileBatch(CompilerParameters options, string[] fileNames)
        {
            if (options == null)
                throw new ArgumentNullException("options");
            if (fileNames == null)
                throw new ArgumentNullException("fileNames");

            if (fileNames.Length != 1)
                throw new NotImplementedException();
            return CompileAssemblyFromFile(options, fileNames[0]);
        }
        #endregion from file

        #region from source
        public CompilerResults CompileAssemblyFromSource(CompilerParameters options, string source)
        {
            if (options == null)
                throw new ArgumentNullException("options");
            if (source == null)
                throw new ArgumentNullException("source");

            return Compile(options, new StringSource(source), "");
        }

        public CompilerResults CompileAssemblyFromSourceBatch(CompilerParameters options, string[] sources)
        {
            if (options == null)
                throw new ArgumentNullException("options");
            if (sources == null)
                throw new ArgumentNullException("sources");

            if (sources.Length != 1)
                throw new NotImplementedException();
            return CompileAssemblyFromSource(options, sources[0]);
        }
        #endregion from source

        #region Compile
        CompilerResults Compile(CompilerParameters options, ISource source, string fileName)
        {
            FileKind fileKind;
            RuntimeMethodsUsage runtimeMethodsUsage;
            bool toMemory;
            Version version;

            if (options is JellequinCompilerParameters jellOptions)
            {
                fileKind = jellOptions.FileKind;
                runtimeMethodsUsage = jellOptions.RuntimeMethodsUsage;
                version = jellOptions.Version;
            }
            else
            {
                fileKind = options.GenerateExecutable ? FileKind.ConsoleExe : FileKind.Dll;
                runtimeMethodsUsage = options.GenerateExecutable ? RuntimeMethodsUsage.Copy : RuntimeMethodsUsage.Call;
                version = new Version(1, 0, 0, 0);
            }
            toMemory = options.GenerateInMemory;

            JellequinCompilerResults result = new JellequinCompilerResults(new TempFileCollection() { KeepFiles = false });
            bool compilerOk;
            Stream assembly = null;
            Stream pdb = null;
            //Stream icon=null;

            try
            {
                assembly = new MemoryStream();
                bool debug = options.IncludeDebugInformation;
                bool embedSourceCode = toMemory || source is StringSource;
                if (debug && (!embedSourceCode))
                    pdb = new MemoryStream();

                Compiler.Compile(source, assembly, new AssemblyName(options.OutputAssembly) { Version = version },
                    new CompilerOptions
                    {
                        DontUseDynamicJsMembers = false,
                        FileKind = fileKind,
                        RuntimeMethodsUsage = runtimeMethodsUsage,
                        //Icon = icon,
                        Debug = new DebugOptions
                        {
                            Debug = debug,
                            EmbedSourceCode = embedSourceCode,
                            Pdb = pdb
                        }
                    });
                compilerOk = true;
            }
            catch (Exception ex)
            {
                assembly?.Dispose();
                pdb?.Dispose();
                //icon?.Dispose();

                result.CompilerException = ex;
                result.Errors.Add(new CompilerError(fileName, 0, 0, "", ex.GetBaseException().Message));
                compilerOk = false;
            }

            if (compilerOk)
            {
                if (!toMemory)
                {
                    string path = options.OutputAssembly;
                    result.PathToAssembly = path;
                    SaveToDisk(assembly, path);
                    SaveToDisk(pdb, Path.ChangeExtension(path, ".pdb"));
                }
                result.CompiledAssembly = Assembly.Load(ReadBytes(assembly), ReadBytes(pdb));
                assembly.Dispose();
                pdb.Dispose();
            }
            return result;
        }

        byte[] ReadBytes(Stream stream)
        {
            int len = (int)stream.Length;
            byte[] result = new byte[len];
            stream.Position = 0;
            stream.Read(result, 0, len);
            return result;
        }

        void SaveToDisk(Stream stream, string path)
        {
            stream.Position = 0;
            using (Stream file = File.Create(path))
                stream.CopyTo(file);
        }
        #endregion Compile
    }
}
