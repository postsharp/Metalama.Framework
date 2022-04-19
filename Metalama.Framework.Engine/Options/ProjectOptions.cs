// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;

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

        [Memo]
        public string ProjectId => this.GetStringOption( "MetalamaProjectId" ) ?? this._defaultProjectId;

        [Memo]
        public string? BuildTouchFile => this.GetStringOption( "MetalamaBuildTouchFile" );

        [Memo]
        public string? SourceGeneratorTouchFile => this.GetStringOption( "MetalamaSourceGeneratorTouchFile" );

        [Memo]
        public string? AssemblyName => this.GetStringOption( "AssemblyName" );

        public ImmutableArray<object> PlugIns { get; }

        [Memo]
        public bool IsFrameworkEnabled => this.GetBooleanOption( "MetalamaEnabled", true ) && !this.GetBooleanOption( "MetalamaCompileTimeProject" );

        [Memo]
        public bool FormatOutput => this.GetBooleanOption( "MetalamaFormatOutput" );

        [Memo]
        public bool FormatCompileTimeCode => this.GetBooleanOption( "MetalamaFormatCompileTimeCode" );

        [Memo]
        public bool IsUserCodeTrusted => this.GetBooleanOption( "MetalamaUserCodeTrusted", true );

        [Memo]
        public string? ProjectPath => this.GetStringOption( "MSBuildProjectFullPath" );

        [Memo]
        public string? TargetFramework => this.GetStringOption( "TargetFramework" );

        [Memo]
        public string? Configuration => this.GetStringOption( "Configuration" );

        [Memo]
        public bool IsDesignTimeEnabled => this.GetBooleanOption( "MetalamaDesignTimeEnabled", true );

        [Memo]
        public string? AdditionalCompilationOutputDirectory => this.GetStringOption( "MetalamaAdditionalCompilationOutputDirectory" );

        public string? DotNetSdkDirectory
        {
            get
            {
                var propsFilePath = this.GetStringOption( "NETCoreSdkBundledVersionsProps" );

                if ( propsFilePath == null )
                {
                    return null;
                }

                return Path.GetFullPath( Path.GetDirectoryName( propsFilePath )! );
            }
        }

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