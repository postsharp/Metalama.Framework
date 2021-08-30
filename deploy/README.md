# PostSharp Engineering: Deployment Features

Make sure you have read and understood [PostSharp Engineering](../README.md) before reading this doc.

## Table of contents

## Continuous deployment

### Instalation

1. Set up continuos integration as described at [PostSharp Engineering: Build Features](../build/README.md#continuous-integration).

2. Create `.engineering-local\CopyToPublishDir.proj` file. The content should look like this:

```
<Project>

    <ItemGroup>
        <!-- This is the list of files that can be published to the public nuget.org -->
        <!-- To avoid unintended publishing of artefacts, all items must be explicitly specified without wildcard -->

        <ShippedFile Include="$(PackagesDir)\Caravela.Framework.$(Version).nupkg" />
        <ShippedFile Include="$(PackagesDir)\Caravela.Framework.Redist.$(Version).nupkg" />
        <ShippedFile Include="$(PackagesDir)\Caravela.Framework.Sdk.$(Version).nupkg" />
    </ItemGroup>

    <PropertyGroup>
        <!-- We only publish release builds -->
        <Configuration>Release</Configuration>
    </PropertyGroup>

    <Import Project="..\Directory.Build.props" />
    <Import Project="..\.engineering\deploy\CopyToPublishDir.targets" />

</Project>
```

3. Create "Release Build" build configuration using manual build steps configuration.

Build steps:

| # | Name | Type | Configuration |
| - | ---- | ---- | ------------- |
| 1 | Restore | .NET | Command: restore |
| 2 | Build and Pack | .NET | Command: pack; Configuration: Release |
| 3 | Copy to 'publish' directory | .NET | Command: msbuild; Projects: .engineering-local/CopyToPublishDir.proj; MSBuild version: Cross-platform MSBuild |
| 4 | Sign and Verify | PowerShell | Format stderr output as: error; Script: File; Script file: .engineering/deploy/SignAndVerify.ps1; Script arguments: %env.TEAMCITY_PROJECT_NAME% |

Artifact paths:

```
publish/*.nupkg => publish
artifacts/bin/Release/*.nupkg => artifacts/bin/Release
```

Snapshot dependencies:

- Debug Build and Test

4. Create "Publish Debug to Internal Feed" deployment configuration using manual build steps configuration.

Build steps:

| # | Name | Type | Configuration |
| - | ---- | ---- | ------------- |
| 1 | NuGet Push | .NET | Command: NuGet Push; NuGet Packages: artifacts\bin\Debug\*.nupkg; NuGet Server: %env.INTERNAL_NUGET_PUSH_URL%; API key: %env.INTERNAL_NUGET_API_KEY%; Command line parameters: --skip-duplicate |

Snapshot dependencies:

- Debug Build and Test

Artifact dependencies:

- Debug Build and Test

```
+:artifacts/bin/Debug/*.nupkg => artifacts/bin/Debug
```

5. Create "Publish Release to NuGet.Org and Internal Feed" deployment configuration using manual build steps configuration.

Build steps:

| # | Name | Type | Configuration |
| - | ---- | ---- | ------------- |
| 1 | Push to nuget.org | .NET | Command: NuGet Push; NuGet Packages: publish\*.nupkg; NuGet Server: %env.NUGET_ORG_PUSH_URL%; API key: %env.NUGET_ORG_API_KEY%; Command line parameters: --skip-duplicate |
| 2 | Push to internal feed | .NET | Command: NuGet Push; NuGet Packages: artifacts\bin\Release\*.nupkg; NuGet Server: %env.INTERNAL_NUGET_PUSH_URL%; API key: %env.INTERNAL_NUGET_API_KEY%; Command line parameters: --skip-duplicate |

Snapshot dependencies:

- Debug Build and Test
- Release build

Artifact dependencies:

- Release build

```
+:publish/*.nupkg => publish/
+:artifacts/bin/Release/*.nupkg => artifacts\bin\Release
```