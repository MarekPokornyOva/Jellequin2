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
* Debug - if to generate symbols (debug) information

### SaveOptions
* Symbols - see SymbolsSaveOptions below.
* Icon - icon built into generated assembly (resources).
* AssemblyAttributes - assembly custom attributes.

##### SymbolsSaveOptions
* EmbedSource - if to embed source code to output PDB.
* Pdb - PDB output stream.
* HashAlgorithm - hash algorithm used for embedded source code.
* Code - source code detail information provider.