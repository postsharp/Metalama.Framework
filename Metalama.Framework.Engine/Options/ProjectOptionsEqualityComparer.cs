// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Linq;

namespace Metalama.Framework.Engine.Options;

public static class ProjectOptionsEqualityComparer
{
    public static bool Equals( IProjectOptions x, IProjectOptions y )
        => ReferenceEquals( x, y ) ||
           (x.BuildTouchFile == y.BuildTouchFile &&
            x.SourceGeneratorTouchFile == y.SourceGeneratorTouchFile &&
            x.AssemblyName == y.AssemblyName &&
            x.IsFrameworkEnabled == y.IsFrameworkEnabled &&
            x.CodeFormattingOptions == y.CodeFormattingOptions &&
            x.WriteHtml == y.WriteHtml &&
            x.FormatCompileTimeCode == y.FormatCompileTimeCode &&
            x.IsUserCodeTrusted == y.IsUserCodeTrusted &&
            x.ProjectPath == y.ProjectPath &&
            x.ProjectName == y.ProjectName &&
            x.TargetFramework == y.TargetFramework &&
            x.TargetFrameworkMoniker == y.TargetFrameworkMoniker &&
            x.Configuration == y.Configuration &&
            x.IsDesignTimeEnabled == y.IsDesignTimeEnabled &&
            x.AdditionalCompilationOutputDirectory == y.AdditionalCompilationOutputDirectory &&
            x.RemoveCompileTimeOnlyCode == y.RemoveCompileTimeOnlyCode &&
            x.RequiresCodeCoverageAnnotations == y.RequiresCodeCoverageAnnotations &&
            x.AllowPreviewLanguageFeatures == y.AllowPreviewLanguageFeatures &&
            x.RequireOrderedAspects == y.RequireOrderedAspects &&
            x.IsConcurrentBuildEnabled == y.IsConcurrentBuildEnabled &&
            x.CompileTimePackages.SequenceEqual( y.CompileTimePackages ) &&
            x.ProjectAssetsFile == y.ProjectAssetsFile &&
            x.ReferenceAssemblyRestoreTimeout == y.ReferenceAssemblyRestoreTimeout &&
            x.License == y.License &&
            x.WriteLicenseUsageData == y.WriteLicenseUsageData &&
            x.RoslynIsCompileTimeOnly == y.RoslynIsCompileTimeOnly &&
            x.CompileTimeTargetFrameworks == y.CompileTimeTargetFrameworks &&
            x.RestoreSources == y.RestoreSources &&
            x.TemplateLanguageVersion == y.TemplateLanguageVersion &&
            x.DebugTransformedCode == y.DebugTransformedCode &&
            x.TransformedFilesOutputPath == y.TransformedFilesOutputPath &&
            x.IsTest == y.IsTest);
}