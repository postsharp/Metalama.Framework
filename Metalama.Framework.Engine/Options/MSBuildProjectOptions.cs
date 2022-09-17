// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Caching;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.Options
{
    /// <summary>
    /// The production implementation of <see cref="IProjectOptions"/>, based on a <see cref="IProjectOptionsSource"/>
    /// reading options passed by MSBuild to the compiler.
    /// </summary>
    [ExcludeFromCodeCoverage]

    // ReSharper disable once InconsistentNaming
    public partial class MSBuildProjectOptions : DefaultProjectOptions
    {
        private static readonly ConditionalWeakTable<AnalyzerConfigOptions, MSBuildProjectOptions> _cache = new();

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

        private MSBuildProjectOptions( IProjectOptionsSource source, ImmutableArray<object>? plugIns, TransformerOptions? transformerOptions = null )
        {
            this._source = source;
            this._transformerOptions = transformerOptions ?? TransformerOptions.Default;
            this.PlugIns = plugIns ?? ImmutableArray<object>.Empty;
        }

        private MSBuildProjectOptions( AnalyzerConfigOptions options, ImmutableArray<object>? plugIns = null, TransformerOptions? transformerOptions = null ) :
            this( new OptionsAdapter( options ), plugIns, transformerOptions ) { }

        [Memo]
        public override string? BuildTouchFile => this.GetStringOption( "MetalamaBuildTouchFile" );

        [Memo]
        public override string? SourceGeneratorTouchFile => this.GetStringOption( "MetalamaSourceGeneratorTouchFile" );

        [Memo]
        public override string? AssemblyName => this.GetStringOption( "AssemblyName" );

        public override ImmutableArray<object> PlugIns { get; }

        [Memo]
        public override bool IsFrameworkEnabled => this.GetBooleanOption( "MetalamaEnabled", true ) && !this.GetBooleanOption( "MetalamaCompileTimeProject" );

        [Memo]
        public override bool FormatOutput => this.GetBooleanOption( "MetalamaFormatOutput" );

        [Memo]
        public override bool FormatCompileTimeCode => this.GetBooleanOption( "MetalamaFormatCompileTimeCode" );

        [Memo]
        public override bool IsUserCodeTrusted => this.GetBooleanOption( "MetalamaUserCodeTrusted", true );

        [Memo]
        public override string? ProjectPath => this.GetStringOption( "MSBuildProjectFullPath" );

        [Memo]
        public override string? TargetFramework => this.GetStringOption( "TargetFramework" );

        [Memo]
        public override string? Configuration => this.GetStringOption( "Configuration" );

        [Memo]
        public override bool IsDesignTimeEnabled => this.GetBooleanOption( "MetalamaDesignTimeEnabled", true );

        [Memo]
        public override string? AdditionalCompilationOutputDirectory => this.GetStringOption( "MetalamaAdditionalCompilationOutputDirectory" );

        [Memo]
        public override bool RemoveCompileTimeOnlyCode => this.GetBooleanOption( "MetalamaRemoveCompileTimeOnlyCode", true );

        [Memo]
        public override bool AllowPreviewLanguageFeatures => this.GetBooleanOption( "MetalamaAllowPreviewLanguageFeatures" );

        public override bool RequiresCodeCoverageAnnotations => this._transformerOptions.RequiresCodeCoverageAnnotations;

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