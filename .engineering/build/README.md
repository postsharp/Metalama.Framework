# PostSharp Engineering: Build Features

Make sure you have read and understood [PostSharp Engineering](../README.md) before reading this doc.

## Table of contents

- [PostSharp Engineering: Build Features](#postsharp-engineering-build-features)
  - [Table of contents](#table-of-contents)
  - [Executable scripts](#executable-scripts)
    - [CreateLocalPackages.ps1](#createlocalpackagesps1)
    - [RestoreRelease.ps1](#restorereleaseps1)
    - [Kill.ps1](#killps1)
    - [RestoreRelease.ps1](#restorereleaseps1-1)
  - [Imported scripts](#imported-scripts)
    - [AssemblyMetadata.props and AssemblyMetadata.targets](#assemblymetadataprops-and-assemblymetadatatargets)
    - [CompilerOptions.props](#compileroptionsprops)
    - [Engineering.Directories.props](#engineeringdirectoriesprops)
    - [Engineering.Versions.props](#engineeringversionsprops)
    - [SourceLink.props](#sourcelinkprops)
  - [NuGet packages metadata](#nuget-packages-metadata)
    - [Installation and configuration](#installation-and-configuration)
  - [Versioning](#versioning)
    - [Installation and configuration](#installation-and-configuration-1)
    - [Usage](#usage)
      - [Product package version and maturity configuration](#product-package-version-and-maturity-configuration)
      - [Package dependencies versions configuration](#package-dependencies-versions-configuration)
      - [Creating local development packages](#creating-local-development-packages)
  - [Continuous integration](#continuous-integration)
    - [Instalation](#instalation)

## Executable scripts

### CreateLocalPackages.ps1

Creates a new set of nuget packages for local use. See [Versioning](#versioning) for details on usage of this feature.

### RestoreRelease.ps1

Used by CI pipeline. See [Continuous integration](#continuous-integration) for details.

### Kill.ps1

Kills all processes which might hold any files from the repository.

### RestoreRelease.ps1

TODO: Unused?

## Imported scripts

The scripts listed below are meant to be imported in
- `Directory.Build.props` (*.props)
- `Directory.Build.targets` (*.targets)

### AssemblyMetadata.props and AssemblyMetadata.targets

Add package versions to assembly metadata.

### CompilerOptions.props

Sets the compiler options like language version or nullability.

### Engineering.Directories.props

Sets the common directories like the output path.

### Engineering.Versions.props

Manages versioning. See [Versioning](#versioning) for details on usage of this feature.

### SourceLink.props

Enables SourceLink support.

## NuGet packages metadata

This section describes centralize NuGet packages metadata management.

### Installation and configuration

1. Create `.engineering-local\Packaging.props` file. The content should look like this:

```
<Project>

    <!-- Properties of NuGet packages-->
    <PropertyGroup>
        <Authors>PostSharp Technologies</Authors>
        <PackageProjectUrl>https://github.com/postsharp/Caravela</PackageProjectUrl>
        <PackageTags>PostSharp Caravela AOP</PackageTags>
        <PackageRequireLicenseAcceptance>true</PackageRequireLicenseAcceptance>
        <PackageIcon>PostSharpIcon.png</PackageIcon>
        <PackageLicenseFile>LICENSE.md</PackageLicenseFile>
    </PropertyGroup>

    <!-- Additional content of NuGet packages -->
    <ItemGroup>
        <None Include="$(MSBuildThisFileDirectory)..\PostSharpIcon.png" Visible="false" Pack="true" PackagePath="" />
        <None Include="$(MSBuildThisFileDirectory)..\LICENSE.md" Visible="false" Pack="true" PackagePath="" />
        <None Include="$(MSBuildThisFileDirectory)..\THIRD-PARTY-NOTICES.TXT" Visible="false" Pack="true" PackagePath="" />
    </ItemGroup>

</Project>
```

2. Make sure that all the files referenced in the previous step exist.

3. Import the file from the first step in `Directory.Build.props`:

```
  <Import Project=".engineering-local\Packaging.props" />
```

Now all the packages creted from the repository will contain the metadata configured in the `.engineering-local\Packaging.props` file.

## Versioning

This section describes centralized version management.

### Installation and configuration

1. Add `LocalBuildId.props` to `.gitignore`.

2. Create `.engineering-local\Versions.props` file. The content should look like:

```
<Project>

  <!-- The version of the product produced by this repository. -->
  <PropertyGroup>
    <ProductMainVersion>1.0.0</ProductMainVersion>
    <!-- Empty for RTM. -->
    <ProductMaturity>preview</ProductMaturity>
  </PropertyGroup>

  <!-- Versions of dependencies -->
  <PropertyGroup>
    <SystemCollectionsImmutableVersion>5.0.0</SystemCollectionsImmutableVersion>
  </PropertyGroup>

</Project>
```

3. Add the following imports to `Directory.Build.props`:

```
  <!-- LocalBuildId.props is created by CreateLocalPackages.ps1. It is used on development environments only (not on build servers) -->
  <Import Project="LocalBuildId.props" Condition="Exists('LocalBuildId.props')" />
  <Import Project=".engineering-local\Versions.props" />
  <Import Project=".engineering\build\Engineering.Versions.props" />
```

### Usage

#### Product package version and maturity configuration

The product package version and maturity configuration is centralized in the `.engineering-local\Versions.props` script via the `ProductMainVersion` and `ProductMaturity` properties. For RTM products, leave the `ProductMaturity` property value empty.

#### Package dependencies versions configuration

Package dependecies vesrions configuration is also centralized in the `.engineering-local\Versions.props` script. Each dependency version is configured in a property named `<[DependencyName]Version>`, eg. `<SystemCollectionsImmutableVersion>`.

This property value is then available in all MSBuild project files in the repository and can be used in the `PackageReference` items. For example:

```
<ItemGroup>
    <PackageReference Include="System.Collections.Immutable" Version="$(SystemCollectionsImmutableVersion)" />
</ItemGroup>
```

#### Creating local development packages

To create packages to be used locally, call `& .engineering\build\CreateLocalPackages.ps1` from PowerShell.

The script generates a version suffix in `LocalBuildId.props` file and creates NuGet package(s) in `artifacts\bin\Debug` directory. This directory can then be used as a NuGet package repository in other projects.

## Continuous integration

We use TeamCity as our CI/CD pipeplie at the moment. The folowing sections describe a common way to set up continous integration on TeamCity. See [PostSharp Engineering: Deployment Features](../deploy/README.md#continuous-deployment) for information about continuous deployment.

### Instalation

1. Create a new (sub)project using manual setup.
   
2. Set up versioned settings if necessary.

3. Add a VCS root.

4. Create build configurations. Set build agent requirements and triggers as needed.

   1. Create "Debug Build and Test" build configuration using manual build steps configuration.

Build steps:

| # | Name | Type | Configuration |
| - | ---- | ---- | ------------- |
| 1 | Restore | .NET | Command: restore |
| 2 | Build | .NET | Command: build; Projects: [projects]; Configuration: Debug; Version suffix: %build.number% |
| 3 | Test | .NET | Command: test; Projects: [projects]; Options: Do not build the projects; Command line parameters: --no-restore |
| 4 | Pack | .NET | Command: pack; Projects: [projects]; Version suffix: %build.number% |

Artifact paths:

```
artifacts\bin\Debug\*.nupkg => artifacts/bin/Debug
```
