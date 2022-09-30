// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Caching;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.Options
{
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
        public const string ProjectAssetsFile = nameof(ProjectAssetsFile);
    }

    public static class MSBuildItemNames
    {
        public const string MetalamaCompileTimePackage = nameof(MetalamaCompileTimePackage); 
    }
    /// <summary>
    /// The production implementation of <see cref="IProjectOptions"/>, based on a <see cref="IProjectOptionsSource"/>
    /// reading options passed by MSBuild to the compiler.
    /// </summary>
    [ExcludeFromCodeCoverage]

    // ReSharper disable once InconsistentNaming
    public partial class MSBuildProjectOptions : DefaultProjectOptions
    {
#pragma warning disable CA1805 // Do not initialize unnecessarily
        private static readonly WeakCache<AnalyzerConfigOptions, MSBuildProjectOptions> _cache = new();
#pragma warning restore CA1805 // Do not initialize unnecessarily

        private readonly IProjectOptionsSource _source;
        private readonly TransformerOptions _transformerOptions;

        public static MSBuildProjectOptions GetInstance(
            AnalyzerConfigOptionsProvider options,
            ImmutableArray<object>? plugIns = null,
            TransformerOptions? transformerOptions = null )
            => GetInstance( options.GlobalOptions, plugIns, transformerOptions );

        public static MSBuildProjectOptions GetInstance(
            AnalyzerConfigOptions options,
            ImmutableArray<object>? plugIns = null,
            TransformerOptions? transformerOptions = null )
        {
            if ( plugIns != null || transformerOptions != null )
            {
                // We have a source transformer. Caching is useless.
                return new MSBuildProjectOptions( options, plugIns, transformerOptions );
            }
            else
            {
                // At design time, we should try to cache.
                return _cache.GetOrAdd( options, o => new MSBuildProjectOptions( o ) );
            }
        }

        public static MSBuildProjectOptions GetInstance(
            Microsoft.CodeAnalysis.Project project,
            ImmutableArray<object>? plugIns = null,
            TransformerOptions? transformerOptions = null )
            => GetInstance( project.AnalyzerOptions.AnalyzerConfigOptionsProvider.GlobalOptions, plugIns, transformerOptions );

        protected MSBuildProjectOptions( IProjectOptionsSource source, ImmutableArray<object>? plugIns, TransformerOptions? transformerOptions = null )
        {
            this._source = source;
            this._transformerOptions = transformerOptions ?? TransformerOptions.Default;
            this.PlugIns = plugIns ?? ImmutableArray<object>.Empty;
        }

        private MSBuildProjectOptions( AnalyzerConfigOptions options, ImmutableArray<object>? plugIns = null, TransformerOptions? transformerOptions = null ) :
            this( new OptionsAdapter( options ), plugIns, transformerOptions ) { }

        [Memo]
        public override string? BuildTouchFile => this.GetStringOption( MSBuildPropertyNames.MetalamaBuildTouchFile );

        [Memo]
        public override string? SourceGeneratorTouchFile => this.GetStringOption( MSBuildPropertyNames.MetalamaSourceGeneratorTouchFile );

        [Memo]
        public override string? AssemblyName => this.GetStringOption( MSBuildPropertyNames.AssemblyName );

        public override ImmutableArray<object> PlugIns { get; }

        [Memo]
        public override bool IsFrameworkEnabled => this.GetBooleanOption( MSBuildPropertyNames.MetalamaEnabled, true ) && !this.GetBooleanOption( MSBuildPropertyNames.MetalamaCompileTimeProject );

        [Memo]
        public override bool FormatOutput => this.GetBooleanOption( MSBuildPropertyNames.MetalamaFormatOutput);

        [Memo]
        public override bool FormatCompileTimeCode => this.GetBooleanOption( MSBuildPropertyNames.MetalamaFormatCompileTimeCode );

        [Memo]
        public override bool IsUserCodeTrusted => this.GetBooleanOption( MSBuildPropertyNames.MetalamaUserCodeTrusted, true );

        [Memo]
        public override string? ProjectPath => this.GetStringOption( MSBuildPropertyNames.MSBuildProjectFullPath );

        [Memo]
        public override string? TargetFramework => this.GetStringOption( MSBuildPropertyNames.TargetFramework );

        [Memo]
        public override string? TargetFrameworkMoniker => this.GetStringOption( MSBuildPropertyNames.NuGetTargetMoniker );

        [Memo]
        public override string? Configuration => this.GetStringOption( MSBuildPropertyNames.Configuration );

        [Memo]
        public override bool IsDesignTimeEnabled => this.GetBooleanOption( MSBuildPropertyNames.MetalamaDesignTimeEnabled, true );

        [Memo]
        public override string? AdditionalCompilationOutputDirectory => this.GetStringOption( MSBuildPropertyNames.MetalamaAdditionalCompilationOutputDirectory );

        [Memo]
        public override bool RemoveCompileTimeOnlyCode => this.GetBooleanOption( MSBuildPropertyNames.MetalamaRemoveCompileTimeOnlyCode, true );

        [Memo]
        public override bool AllowPreviewLanguageFeatures => this.GetBooleanOption( MSBuildPropertyNames.MetalamaAllowPreviewLanguageFeatures );

        [Memo]
        public override bool RequireOrderedAspects => this.GetBooleanOption( MSBuildPropertyNames.MetalamaRequireOrderedAspects );

        public override bool IsConcurrentBuildEnabled => this.GetBooleanOption( MSBuildPropertyNames.MetalamaConcurrentBuildEnabled, true );

        public override bool RequiresCodeCoverageAnnotations => this._transformerOptions.RequiresCodeCoverageAnnotations;

        [Memo]
        public override ImmutableArray<string> CompileTimePackages
            => this.GetStringOption( MSBuildPropertyNames.MetalamaCompileTimePackages, "" )!
                .Split( ',', ';', ' ' )
                .Select( p => p.Trim() )
                .Where( p => !string.IsNullOrEmpty( p ) )
                .ToImmutableArray();

        [Memo]
        public override string? ProjectAssetsFile => this.GetStringOption( MSBuildPropertyNames.ProjectAssetsFile );

        public override bool TryGetProperty( string name, [NotNullWhen( true )] out string? value )
        {
            value = this.GetStringOption( name );

            return value != null;
        }

        private bool GetBooleanOption( string name, bool defaultValue = false )
        {
            if ( this._source.TryGetValue( $"build_property.{name}", out var flagString ) && bool.TryParse( flagString, out var flagValue ) )
            {
                return flagValue;
            }

            return defaultValue;
        }

        private string? GetStringOption( string name, string? defaultValue = null )
        {
            if ( this._source.TryGetValue( $"build_property.{name}", out var flagString ) )
            {
                return flagString;
            }

            return defaultValue;
        }
    }
}