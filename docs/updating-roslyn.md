# Updating Roslyn

1. Update Metalama.Compiler first. 
2. Study the new C# syntax features. We IGNORE any experimental feature. They are not supported. If the new Roslyn only has new experimental features, there is nothing to do in this repo.
3. Add the `Syntax.xml` file from Roslyn to `source-dependencies\Metalama.Framework.Private\Metalama.Framework.GenerateMetaSyntaxRewriter`
4. Edit `source-dependencies\Metalama.Framework.Private\Metalama.Framework.GenerateMetaSyntaxRewriter\Program.cs` to include this file.
5. Run `build.ps1 prepare -p PrepareStubs=True`.
6. Run `build.ps1 prepare`.
7. Inside `build\RoslynVersion`:
    1. Create a `.props` file for the new version. Copy from the previous latest version and just change the version number.
    2. Update the `Latest.imports` to point to the new version.
    3. In the `Roslyn.*.props` file of the _previous_ version, set the `ThisRoslynVersionProjectSuffix` property to something like `.4.0.1` and _mind the leading period_, it is necessary.
8. Look at all projects named e.g. `Metalama.*.<LAST_VERSION>.csproj` and duplicate them, but import the _previous last version_.
9. Update Metalama.sln to include the new project.
10. Do a find-in-files for the _previous_ latest version and see where things need to be changed or added. This includes:
    1. Many `InternalsVisibleTo`
    2. `ResourceExtractor.GetRoslynVersion`
    3. `JsonSerializationBinder`
11. Update `Metalama.Framework.CompilerExtensions.csproj` to include the new assemblies.
12. Don't forget that Metalama.Framework.Private is a separate repo, so it needs its own PR.

The testing should include:
* normal compile-time testing,
* basic design-time testing with the new VS version,
* basic design-time testing with the _previous_ VS version, or at least with the previous LTS version.