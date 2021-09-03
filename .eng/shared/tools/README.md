# Introduction 

This repo contains custom build tools shared by multiple repos at PostSharp Technologies. They are packages for `dotnet tool`.

# Commands

The tools implement the following commands:

* `nuget rename`: Renames the packages in a directory, including the dependency, from the prefix `Microsoft` to the prefix `Caravela.Roslyn`.
* `nuget verify`: Verify the dependencies of all packages in a directory: the dependency must be either public or must be present in the directory itself.

# Using the tools

1. Install the tool locally

	```dotnet tool install --add-source . PostSharp.Engineering.BuildTools  --tool-path tools```

2. Run the tool. See the help.

	```tools\postsharp-eng.exe```

# Build

1. Update the version number in PostSharp.Engineering.BuildTools

2. Use `dotnet pack` 

3. Upload manually to https://nuget.postsharp.net

4. Update the version number in TeamCity (project-level parameter BUILD_TOOLS_VERSION).
