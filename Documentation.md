# Documentation

### CompilerOptions
* FileKind
	- Dll - compiles code as library (without entry point).
	- ConsoleExe - compiles code as console application (with entry point). Such application might be launched by "dotnet app.dll" command.
* RuntimeMethodsUsage
	- Call - needed runtime is simply used from "Jellequin.Runtime" assembly.
	- Copy - all runtime assembly content is copied to output assembly.
* DontUseDynamicJsMembers
	- false - supports dynamic members for ES6 compatibility.
	- true - uses static members (implemented as standard .NET class with standard members).
* Icon - icon built into generated assembly (resources).
* Debug - see DebugOptions below.

##### DebugOptions
* Debug - if to generate output PDB.
* EmbedSourceCode - if to embed source code to output PDB.
* Pdb - PDB output stream.
