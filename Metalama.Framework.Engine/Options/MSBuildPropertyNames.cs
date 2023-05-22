// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

// ReSharper disable InconsistentNaming

using JetBrains.Annotations;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Options;

[PublicAPI]
public static class MSBuildPropertyNames
{
    public const string MetalamaBuildTouchFile = nameof(MetalamaBuildTouchFile);
    public const string MetalamaSourceGeneratorTouchFile = nameof(MetalamaSourceGeneratorTouchFile);
    public const string AssemblyName = nameof(AssemblyName);
    public const string MetalamaEnabled = nameof(MetalamaEnabled);
    public const string MetalamaCompileTimeProject = nameof(MetalamaCompileTimeProject);
    public const string MetalamaFormatOutput = nameof(MetalamaFormatOutput);
    public const string MetalamaFormatCompileTimeCode = nameof(MetalamaFormatCompileTimeCode);
    public const string MetalamaUserCodeTrusted = nameof(MetalamaUserCodeTrusted);
    public const string MSBuildProjectFullPath = nameof(MSBuildProjectFullPath);
    public const string TargetFramework = nameof(TargetFramework);
    public const string NuGetTargetMoniker = nameof(NuGetTargetMoniker);
    public const string Configuration = nameof(Configuration);
    public const string MetalamaDesignTimeEnabled = nameof(MetalamaDesignTimeEnabled);
    public const string MetalamaAdditionalCompilationOutputDirectory = nameof(MetalamaAdditionalCompilationOutputDirectory);
    public const string MetalamaRemoveCompileTimeOnlyCode = nameof(MetalamaRemoveCompileTimeOnlyCode);
    public const string MetalamaAllowPreviewLanguageFeatures = nameof(MetalamaAllowPreviewLanguageFeatures);
    public const string MetalamaRequireOrderedAspects = nameof(MetalamaRequireOrderedAspects);
    public const string MetalamaConcurrentBuildEnabled = nameof(MetalamaConcurrentBuildEnabled);
    public const string MetalamaCompileTimePackages = nameof(MetalamaCompileTimePackages);
    public const string MetalamaPlugInAssemblyPaths = nameof(MetalamaPlugInAssemblyPaths);
    public const string MetalamaWriteHtml = nameof(MetalamaWriteHtml);
    public const string ProjectAssetsFile = nameof(ProjectAssetsFile);
    public const string MetalamaReferenceAssemblyRestoreTimeout = nameof(MetalamaReferenceAssemblyRestoreTimeout);
    public const string MetalamaLicense = nameof(MetalamaLicense);
    public const string MetalamaWriteLicenseCreditData = nameof(MetalamaWriteLicenseCreditData);
    public const string MetalamaUsesPackagesConfig = nameof(MetalamaUsesPackagesConfig);

    public static ImmutableArray<string> All { get; } = ImmutableArray.Create(
        MetalamaBuildTouchFile,
        MetalamaSourceGeneratorTouchFile,
        AssemblyName,
        MetalamaEnabled,
        MetalamaCompileTimeProject,
        MetalamaFormatOutput,
        MetalamaFormatCompileTimeCode,
        MetalamaUserCodeTrusted,
        MSBuildProjectFullPath,
        TargetFramework,
        NuGetTargetMoniker,
        Configuration,
        MetalamaDesignTimeEnabled,
        MetalamaAdditionalCompilationOutputDirectory,
        MetalamaRemoveCompileTimeOnlyCode,
        MetalamaAllowPreviewLanguageFeatures,
        MetalamaConcurrentBuildEnabled,
        MetalamaConcurrentBuildEnabled,
        MetalamaCompileTimePackages,
        MetalamaWriteHtml,
        ProjectAssetsFile,
        MetalamaReferenceAssemblyRestoreTimeout,
        MetalamaLicense,
        MetalamaWriteLicenseCreditData,
        MetalamaUsesPackagesConfig );
}