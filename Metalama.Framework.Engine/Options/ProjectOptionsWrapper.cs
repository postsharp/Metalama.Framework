// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Options;

/// <summary>
/// An implementation of <see cref="IProjectOptions"/> that delegates all properties and methods to another <see cref="IProjectOptions"/>.
/// All members are virtual and at least one must be overridden.
/// </summary>
public abstract class ProjectOptionsWrapper : IProjectOptions
{
    private IProjectOptions Wrapped { get; }

    protected ProjectOptionsWrapper( IProjectOptions wrapped )
    {
        this.Wrapped = wrapped;
    }

    public virtual string? BuildTouchFile => this.Wrapped.BuildTouchFile;

    public virtual string? SourceGeneratorTouchFile => this.Wrapped.SourceGeneratorTouchFile;

    public virtual string? AssemblyName => this.Wrapped.AssemblyName;

    public virtual bool IsFrameworkEnabled => this.Wrapped.IsFrameworkEnabled;

    public virtual bool FormatOutput => this.Wrapped.FormatOutput;

    public virtual bool WriteHtml => this.Wrapped.WriteHtml;

    public virtual bool FormatCompileTimeCode => this.Wrapped.FormatCompileTimeCode;

    public virtual bool IsUserCodeTrusted => this.Wrapped.IsUserCodeTrusted;

    public virtual string? ProjectPath => this.Wrapped.ProjectPath;

    public virtual string? ProjectName => this.Wrapped.ProjectName;

    public virtual string? TargetFramework => this.Wrapped.TargetFramework;

    public virtual string? TargetFrameworkMoniker => this.Wrapped.TargetFrameworkMoniker;

    public virtual string? Configuration => this.Wrapped.Configuration;

    public virtual bool IsDesignTimeEnabled => this.Wrapped.IsDesignTimeEnabled;

    public virtual string? AdditionalCompilationOutputDirectory => this.Wrapped.AdditionalCompilationOutputDirectory;

    public virtual IProjectOptions Apply( IProjectOptions options ) => this.Wrapped.Apply( options );

    public virtual bool TryGetProperty( string name, out string? value ) => this.Wrapped.TryGetProperty( name, out value );

    public virtual bool RemoveCompileTimeOnlyCode => this.Wrapped.RemoveCompileTimeOnlyCode;

    public virtual bool RequiresCodeCoverageAnnotations => this.Wrapped.RequiresCodeCoverageAnnotations;

    public virtual bool AllowPreviewLanguageFeatures => this.Wrapped.AllowPreviewLanguageFeatures;

    public virtual bool RequireOrderedAspects => this.Wrapped.RequireOrderedAspects;

    public virtual bool IsConcurrentBuildEnabled => this.Wrapped.IsConcurrentBuildEnabled;

    public virtual ImmutableArray<string> CompileTimePackages => this.Wrapped.CompileTimePackages;

    public virtual string? ProjectAssetsFile => this.Wrapped.ProjectAssetsFile;

    public virtual int? ReferenceAssemblyRestoreTimeout => this.Wrapped.ReferenceAssemblyRestoreTimeout;

    public virtual string? License => this.Wrapped.License;

    public virtual bool? WriteLicenseUsageData => this.Wrapped.WriteLicenseUsageData;

    public virtual bool IsTest => this.Wrapped.IsTest;

    public virtual bool RoslynIsCompileTimeOnly => this.Wrapped.RoslynIsCompileTimeOnly;

    public virtual string? CompileTimeTargetFrameworks => this.Wrapped.CompileTimeTargetFrameworks;

    public virtual string? RestoreSources => this.Wrapped.RestoreSources;

    public virtual string? TemplateLanguageVersion => this.Wrapped.TemplateLanguageVersion;

    public virtual bool? DebugTransformedCode => this.Wrapped.DebugTransformedCode;

    public virtual string? TransformedFilesOutputPath => this.Wrapped.TransformedFilesOutputPath;
}