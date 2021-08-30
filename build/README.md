# PostSharp Engineering: Build Features

Make sure you have read and understood [PostSharp Engineering](../README.md) before reading this doc.

## Table of contents

- [PostSharp Engineering: Build Features](#postsharp-engineering-build-features)
  - [Table of contents](#table-of-contents)
  - [Executable scripts](#executable-scripts)
    - [Build.ps1](#buildps1)
    - [Kill.ps1](#killps1)
  - [Imported scripts](#imported-scripts)
    - [AssemblyMetadata.targets](#assemblymetadatatargets)
    - [BuildOptions.props](#buildoptionsprops)
    - [SourceLink.props](#sourcelinkprops)
  - [NuGet packages metadata](#nuget-packages-metadata)
    - [Installation and configuration](#installation-and-configuration)
  - [Versioning](#versioning)
    - [Installation and configuration](#installation-and-configuration-1)
    - [Usage](#usage)
      - [Product package version and package version suffix configuration](#product-package-version-and-package-version-suffix-configuration)
      - [Package dependencies versions configuration](#package-dependencies-versions-configuration)
      - [Local build and testing](#local-build-and-testing)
      - [Local package referencing](#local-package-referencing)
  - [Continuous integration](#continuous-integration)
    - [Instalation](#instalation)

## Executable scripts

### Build.ps1

This is the main build script providing support for build, packaging and testing, both local and withing a CI/CD pipeline.

### Kill.ps1

Kills all processes which might hold any files from the repository.

## Imported scripts

The scripts listed below are meant to be imported in
- `Directory.Build.props` (*.props)
- `Directory.Build.targets` (*.targets)

### AssemblyMetadata.targets

Adds package versions to assembly metadata.

### BuildOptions.props

Sets the compiler options like language version, nullability and other build options like output path.

### SourceLink.props

Enables SourceLink support.

## NuGet packages metadata

This section describes centralized NuGet packages metadata management.

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

In this how-to, we use the name `[Product]` as a placeholder for the name of the product contained in a specific repository containing the `.engineering` subtree.

1. Add `.engineering-local\[Product]Version.props` to `.gitignore`.

2. Create `.engineering-local\MainVersion.props` file. The content should look like:

```
<Project>
    <PropertyGroup>
        <ProductName>[Product]</ProductName>
        <MainVersion>0.3.6</MainVersion>
        <PackageVersionSuffix>-preview</PackageVersionSuffix>
    </PropertyGroup>
</Project>
```

3. Create `.engineering-local\Versions.props` file. The content should look like:

```
<Project>

    <!-- Normally you should call build.ps1 -local -prepare before opening the project in an IDE, and it creates [Product]Version.props. -->
    <Import Project="[Product]Version.props" Condition="Exists('[Product]Version.props')" />

    <!-- However, if you don't, default values are used. -->
    <Import Project="MainVersion.props" Condition="!Exists('[Product]Version.props')" />
    
    <PropertyGroup Condition="!Exists('[Product]Version.props')">
        <[Product]Version>$(MainVersion)$(PackageVersionSuffix)</[Product]Version>
        <[Product]AssemblyVersion>$(MainVersion)</[Product]AssemblyVersion>
    </PropertyGroup>

    <PropertyGroup>
        <AssemblyVersion>$([Product]AssemblyVersion)</AssemblyVersion>
        <Version>$([Product]Version)</Version>
    </PropertyGroup>

    <!-- Versions of dependencies -->
    <PropertyGroup>
        <RoslynVersion>3.8.0</RoslynVersion>
        <CaravelaCompilerVersion>3.8.12-preview</CaravelaCompilerVersion>
        <MicrosoftCSharpVersion>4.7.0</MicrosoftCSharpVersion>
    </PropertyGroup>

</Project>
```

4. Add the following import to `Directory.Build.props`:

```
  <Import Project=".engineering-local\Versions.props" />
```

### Usage

#### Product package version and package version suffix configuration

The product package version and package version suffix configuration is centralized in the `.engineering-local\MainVersion.props` script via the `MainVersion` and `PackageVersionSuffix` properties, respectively. For RTM products, leave the `PackageVersionSuffix` property value empty.

#### Package dependencies versions configuration

Package dependecies vesrions configuration is centralized in the `.engineering-local\Versions.props` script. Each dependency version is configured in a property named `<[DependencyName]Version>`, eg. `<SystemCollectionsImmutableVersion>`.

This property value is then available in all MSBuild project files in the repository and can be used in the `PackageReference` items. For example:

```
<ItemGroup>
    <PackageReference Include="System.Collections.Immutable" Version="$(SystemCollectionsImmutableVersion)" />
</ItemGroup>
```

#### Local build and testing

See the initial comments in the `.engineering\build\Build.ps1` script for details.

#### Local package referencing

Local NuGet packages creating using the `.engineering\build\Build.ps1` script can be referenced in other repositories using the following steps:

1. Add the following import to `Directory.Build.props`.

```
<Import Project="[PathToReferencedRepo]\[ReferencedProduct]Version.props" Condition="Exists('.local')"/>
```

> TODO: Should we generate the `.local` file?

2. In the dependencies version, set the default version of the referenced package:

```
<[ReferencedProduct]Version Condition="'$([ReferencedProduct]Version)'==''">0.3.6-preview</[ReferencedProduct]Version>
```

3. Add a package reference to projects where required:

```
<PackageReference Include="[ReferencedPackage]" Version="$([ReferencedProduct]Version)" />
```

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

> TODO: Should we switch to the build.ps1 script?
