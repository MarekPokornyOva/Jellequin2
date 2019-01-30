![Logo](./VsExtension/JellequinVs2017.ProjectType/Jellequin.ico "Logo")
# Jellequin

### Description
Jellequin is EcmaScript compiler to CLR (.NET) assembly with debug ability.
Jellequin is an implementation of script language which would be as most as possible like Javascript. It aims to compile the script to IL (.NET) assembly and be able to deeply integrate with hosting environment/application (e.g. written in C#) . It is intended to be able to extend core parts of .NET application with simple and well known script language.
It is also possible to build executable application (single file or referring runtime assembly).

### Features
* Compile JavaScript to .NET assembly (CIL)
* Assemblies could be fully debugged with VS (compiler creates .PDB file)
* Uses .NET types (include callback functions) - to be able to be deeply integrated to .NET application
* Possible to copy runtime functionality to target assembly
* Possible to generate console application
* Visual studio integration - not well working trial of intergration to VS2017

### Documentation
[See](./Documentation.md)

### Release notes
[See](./ReleaseNotes.md)

### Thanks to
This project uses pieces of
* Microsoft Ajax Minifier - https://archive.codeplex.com/?p=ajaxmin
* Roslyn - https://github.com/dotnet/roslyn
* System.Reflection.Metadata - https://www.nuget.org/packages/System.Reflection.Metadata/