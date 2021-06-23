// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Options
{
    /// <summary>
    /// Default implementation of <see cref="IProjectOptions"/>, based on a <see cref="IProjectOptionsSource"/>
    /// reading options passed by MSBuild.
    /// </summary>
    public partial class ProjectOptions : IProjectOptions
    {
        private readonly string _defaultProjectId = Guid.NewGuid().ToString();
        private readonly IProjectOptionsSource _source;

        public ProjectOptions( IProjectOptionsSource source, ImmutableArray<object>? plugIns )
        {
            this._source = source;
            this.PlugIns = plugIns ?? ImmutableArray<object>.Empty;
        }

        public ProjectOptions( AnalyzerConfigOptionsProvider options, ImmutableArray<object>? plugIns = null ) :
            this( new OptionsAdapter( options ), plugIns ) { }

        public ProjectOptions( AnalyzerConfigOptions options, ImmutableArray<object>? plugIns = null ) : this( new OptionsAdapter( options ), plugIns ) { }

        public bool DebugCompilerProcess => this.GetBooleanOption( "DebugCaravela" );

        public bool DebugAnalyzerProcess => this.GetBooleanOption( "DebugCaravelaAnalyzer" );

        public bool DebugIdeProcess => this.GetBooleanOption( "DebugCaravelaIde" );

        public string ProjectId => this.GetStringOption( "CaravelaProjectId" ) ?? this._defaultProjectId;

        public string? BuildTouchFile => this.GetStringOption( "CaravelaBuildTouchFile" );

        public string? AssemblyName => this.GetStringOption( "AssemblyName" );

        public ImmutableArray<object> PlugIns { get; }

        public bool IsFrameworkEnabled => this.GetBooleanOption( "CaravelaEnabled", true );

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