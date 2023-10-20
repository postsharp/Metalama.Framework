// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Compiler;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

// ReSharper disable ClassCanBeSealed.Global

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
        private readonly IProjectOptionsSource _source;
        private readonly TransformerOptions _transformerOptions;

        [UsedImplicitly]
        protected MSBuildProjectOptions( IProjectOptionsSource source, TransformerOptions? transformerOptions = null )
        {
            this._source = source;
            this._transformerOptions = transformerOptions ?? TransformerOptions.Default;
        }

        public MSBuildProjectOptions( AnalyzerConfigOptions options, TransformerOptions? transformerOptions = null ) :
            this( new OptionsAdapter( options ), transformerOptions ) { }

        [Memo]
        public override string? BuildTouchFile => this.GetStringOption( MSBuildPropertyNames.MetalamaBuildTouchFile );

        [Memo]
        public override string? SourceGeneratorTouchFile => this.GetStringOption( MSBuildPropertyNames.MetalamaSourceGeneratorTouchFile );

        [Memo]
        public override string? AssemblyName => this.GetStringOption( MSBuildPropertyNames.AssemblyName );

        [Memo]
        public override bool IsFrameworkEnabled
            => this.GetBooleanOption( MSBuildPropertyNames.MetalamaEnabled, true ) && !this.GetBooleanOption( MSBuildPropertyNames.MetalamaCompileTimeProject );

        [Memo]
        public override bool FormatOutput => this.GetBooleanOption( MSBuildPropertyNames.MetalamaFormatOutput );

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
        public override string? AdditionalCompilationOutputDirectory
            => this.GetStringOption( MSBuildPropertyNames.MetalamaAdditionalCompilationOutputDirectory );

        [Memo]
        public override bool RemoveCompileTimeOnlyCode => this.GetBooleanOption( MSBuildPropertyNames.MetalamaRemoveCompileTimeOnlyCode, true );

        [Memo]
        public override bool AllowPreviewLanguageFeatures => this.GetBooleanOption( MSBuildPropertyNames.MetalamaAllowPreviewLanguageFeatures );

        [Memo]
        public override bool RequireOrderedAspects => this.GetBooleanOption( MSBuildPropertyNames.MetalamaRequireOrderedAspects );

        [Memo]
        public override bool IsConcurrentBuildEnabled => this.GetBooleanOption( MSBuildPropertyNames.MetalamaConcurrentBuildEnabled, true );

        public override bool RequiresCodeCoverageAnnotations => this._transformerOptions.RequiresCodeCoverageAnnotations;

        [Memo]
        public override ImmutableArray<string> CompileTimePackages
            => this.GetStringOption( MSBuildPropertyNames.MetalamaCompileTimePackages, "" )!
                .Split( ',' )
                .SelectAsReadOnlyList( p => p.Trim() )
                .Where( p => !string.IsNullOrEmpty( p ) )
                .ToImmutableArray();

        [Memo]
        public override string? ProjectAssetsFile => this.GetStringOption( MSBuildPropertyNames.ProjectAssetsFile );

        [Memo]
        public override int? ReferenceAssemblyRestoreTimeout => this.GetNullableInt32Option( MSBuildPropertyNames.MetalamaReferenceAssemblyRestoreTimeout );

        [Memo]
        public override string? License => this.GetStringOption( MSBuildPropertyNames.MetalamaLicense );

        [Memo]
        public override bool WriteHtml => this.GetBooleanOption( MSBuildPropertyNames.MetalamaWriteHtml );

        [Memo]
        public override bool? WriteLicenseUsageData => this.GetNullableBooleanOption( MSBuildPropertyNames.MetalamaWriteLicenseUsageData );

        [Memo]
        public override bool RoslynIsCompileTimeOnly => this.GetBooleanOption( MSBuildPropertyNames.MetalamaRoslynIsCompileTimeOnly, defaultValue: true );

        [Memo]
        public override string? CompileTimeTargetFrameworks => this.GetStringOption( MSBuildPropertyNames.MetalamaCompileTimeTargetFrameworks );

        [Memo]
        public override string? RestoreSources => this.GetStringOption( MSBuildPropertyNames.MetalamaRestoreSources );

        public override bool TryGetProperty( string name, [NotNullWhen( true )] out string? value )
        {
            value = this.GetStringOption( name );

            return value != null;
        }

        private bool GetBooleanOption( string name, bool defaultValue = false )
        {
            if ( this._source.TryGetValue( name, out var flagString ) && bool.TryParse( flagString, out var flagValue ) )
            {
                return flagValue;
            }

            return defaultValue;
        }

        private bool? GetNullableBooleanOption( string name )
        {
            if ( this._source.TryGetValue( name, out var flagString ) && bool.TryParse( flagString, out var flagValue ) )
            {
                return flagValue;
            }

            return null;
        }

        private int? GetNullableInt32Option( string name )
        {
            if ( this._source.TryGetValue( name, out var flagString ) && int.TryParse( flagString, out var value ) )
            {
                return value;
            }

            return null;
        }

        private string? GetStringOption( string name, string? defaultValue = null )
        {
            if ( this._source.TryGetValue( name, out var flagString ) )
            {
                return flagString;
            }

            return defaultValue;
        }
    }
}