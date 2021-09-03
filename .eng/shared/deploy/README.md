# PostSharp Engineering: Deployment Features

Make sure you have read and understood [PostSharp Engineering](../README.md) before reading this doc.

## Table of contents

## Continuous deployment

### Instalation

1. Set up continuos integration as described at [PostSharp Engineering: Build Features](../build/README.md#continuous-integration).

2. Create `.eng\CopyToPublishDir.proj` file. The content should look like this:

```
<Project>

    <PropertyGroup>
        <!-- We only publish release builds -->
        <Configuration>Release</Configuration>
    </PropertyGroup>

    <Import Project="$(MSBuildThisFileDirectory)\..\Directory.Build.props" />
    <Import Project="$(MSBuildThisFileDirectory)\shared\deploy\CopyToPublishDir.targets" />

</Project>
```

3. Create "Publish Debug to Internal Feed" deployment configuration using manual build steps configuration.

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

4. Create "Publish Release to NuGet.Org and Internal Feed" deployment configuration using manual build steps configuration.

Build steps:

| # | Name | Type | Configuration |
| - | ---- | ---- | ------------- |
| 1 | Push to nuget.org | .NET | Command: NuGet Push; NuGet Packages: artifacts\publish\*.nupkg; NuGet Server: %env.NUGET_ORG_PUSH_URL%; API key: %env.NUGET_ORG_API_KEY%; Command line parameters: --skip-duplicate |
| 2 | Push to internal feed | .NET | Command: NuGet Push; NuGet Packages: artifacts\bin\Release\*.nupkg; NuGet Server: %env.INTERNAL_NUGET_PUSH_URL%; API key: %env.INTERNAL_NUGET_API_KEY%; Command line parameters: --skip-duplicate |

Snapshot dependencies:

- Debug Build and Test
- Release Build and Test

Artifact dependencies:

- Release Build and Test

```
+:artifacts/publish/public/*.nupkg => artifacts\publish\public
+:artifacts/bin/Release/*.nupkg => artifacts\bin\Release
```