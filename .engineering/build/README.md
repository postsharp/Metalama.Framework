# PostSharp Engineering: Build Features

Make sure you have read and understood [PostSharp Engineering](../README.md) before reading this doc.

## Table of contents

- [PostSharp Engineering: Build Features](#postsharp-engineering-build-features)
  - [Table of contents](#table-of-contents)
  - [Executable scripts](#executable-scripts)
    - [CreateLocalPackages.ps1](#createlocalpackagesps1)
    - [RestoreRelease.ps1](#restorereleaseps1)
    - [Kill.ps1](#killps1)
  - [Imported scripts](#imported-scripts)
    - [AssemblyMetadata.props and AssemblyMetadata.targets](#assemblymetadataprops-and-assemblymetadatatargets)
    - [CompilerOptions.props](#compileroptionsprops)
    - [Engineering.Directories.props](#engineeringdirectoriesprops)
    - [Engineering.Versions.props](#engineeringversionsprops)
    - [SourceLink.props](#sourcelinkprops)
  - [Versioning](#versioning)
    - [Installation](#installation)
    - [Usage](#usage)
      - [Product package version and maturity configuration](#product-package-version-and-maturity-configuration)
      - [Package dependencies versions configuration](#package-dependencies-versions-configuration)
      - [Creating local development packages](#creating-local-development-packages)
  - [Continuous integration](#continuous-integration)

## Executable scripts

### CreateLocalPackages.ps1

Creates a new set of nuget packages for local use. See [Versioning](#versioning) for details on usage of this feature.

### RestoreRelease.ps1

Used by CI pipeline. See [Continuous integration](#continuous-integration) for details.

### Kill.ps1

Kills all processes which might hold any files from the repository.

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

## Versioning

This section describes centralized version management.

### Installation

1. Add `LocalBuildId.props` to `.gitignore`.

2. Create `.engineering-local\Versions.props`. The content can look like:

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

TODO