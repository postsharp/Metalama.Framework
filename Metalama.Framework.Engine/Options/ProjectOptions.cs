// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.Options
{
    /// <summary>
    /// Default implementation of <see cref="IProjectOptions"/>, based on a <see cref="IProjectOptionsSource"/>
    /// reading options passed by MSBuild.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class ProjectOptions : IProjectOptions
    {
        private readonly string _defaultProjectId = Guid.NewGuid().ToString();
        private readonly IProjectOptionsSource _source;

        public ProjectOptions( IProjectOptionsSource source, ImmutableArray<object>? plugIns )
        {
            this._source = source;
            this.PlugIns = plugIns ?? ImmutableArray<object>.Empty;
        }

        public ProjectOptions( Microsoft.CodeAnalysis.Project project, ImmutableArray<object>? plugIns = null ) :
            this( new OptionsAdapter( project.AnalyzerOptions.AnalyzerConfigOptionsProvider ), plugIns ) { }

        public ProjectOptions( AnalyzerConfigOptionsProvider options, ImmutableArray<object>? plugIns = null ) :
            this( new OptionsAdapter( options ), plugIns ) { }

        public ProjectOptions( AnalyzerConfigOptions options, ImmutableArray<object>? plugIns = null ) : this( new OptionsAdapter( options ), plugIns ) { }

        public bool DebugCompilerProcess => this.GetBooleanOption( "MetalamaDebug" );

        public bool DebugAnalyzerProcess => this.GetBooleanOption( "DebugMetalamaAnalyzer" );

        public bool DebugIdeProcess => this.GetBooleanOption( "DebugMetalamaIde" );

        public string ProjectId => this.GetStringOption( "MetalamaProjectId" ) ?? this._defaultProjectId;

        public string? BuildTouchFile => this.GetStringOption( "MetalamaBuildTouchFile" );

        public string? AssemblyName => this.GetStringOption( "AssemblyName" );

        public ImmutableArray<object> PlugIns { get; }

        public bool IsFrameworkEnabled => this.GetBooleanOption( "MetalamaEnabled", true ) && !this.GetBooleanOption( "MetalamaCompileTimeOnlyProject" );

        public bool FormatOutput => this.GetBooleanOption( "MetalamaFormatOutput" );

        public bool FormatCompileTimeCode => this.GetBooleanOption( "MetalamaFormatCompileTimeCode" );

        public bool IsUserCodeTrusted => this.GetBooleanOption( "MetalamaUserCodeTrusted", true );

        public string? ProjectPath => this.GetStringOption( "MSBuildProjectFullPath" );

        public string? TargetFramework => this.GetStringOption( "Configuration" );

        public string? Configuration => this.GetStringOption( "TargetFramework" );

        public bool IsDesignTimeEnabled => this.GetBooleanOption( "MetalamaDesignTimeEnabled", true );

        public string? AdditionalCompilationOutputDirectory => this.GetStringOption( "MetalamaAdditionalCompilationOutputDirectory" );

        public bool TryGetProperty( string name, [NotNullWhen( true )] out string? value )
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