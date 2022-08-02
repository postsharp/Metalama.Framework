// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

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
        private readonly string _defaultProjectId = Guid.NewGuid().ToString();
        private readonly IProjectOptionsSource _source;
        private readonly TransformerOptions _transformerOptions;

        public MSBuildProjectOptions( IProjectOptionsSource source, ImmutableArray<object>? plugIns, TransformerOptions? transformerOptions = null )
        {
            this._source = source;
            this._transformerOptions = transformerOptions ?? TransformerOptions.Default;
            this.PlugIns = plugIns ?? ImmutableArray<object>.Empty;
        }

        public MSBuildProjectOptions(
            Microsoft.CodeAnalysis.Project project,
            ImmutableArray<object>? plugIns = null,
            TransformerOptions? transformerOptions = null ) :
            this( new OptionsAdapter( project.AnalyzerOptions.AnalyzerConfigOptionsProvider ), plugIns, transformerOptions ) { }

        public MSBuildProjectOptions(
            AnalyzerConfigOptionsProvider options,
            ImmutableArray<object>? plugIns = null,
            TransformerOptions? transformerOptions = null ) :
            this( new OptionsAdapter( options ), plugIns, transformerOptions ) { }

        public MSBuildProjectOptions( AnalyzerConfigOptions options, ImmutableArray<object>? plugIns = null, TransformerOptions? transformerOptions = null ) :
            this( new OptionsAdapter( options ), plugIns, transformerOptions ) { }

        [Memo]
        public override string ProjectId => this.GetStringOption( "MetalamaProjectId" ) ?? this._defaultProjectId;

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