// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.IO;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Default implementation of <see cref="IBuildOptions"/>, based on a <see cref="IBuildOptionsSource"/>
    /// reading options passed by MSBuild.
    /// </summary>
    public partial class BuildOptions : IBuildOptions
    {
        private readonly string _defaultProjectId = Guid.NewGuid().ToString();
        private readonly IBuildOptionsSource _source;

        public BuildOptions( IBuildOptionsSource source, ImmutableArray<object>? plugIns )
        {
            this._source = source;
            this.PlugIns = plugIns ?? ImmutableArray<object>.Empty;
        }

        public BuildOptions( AnalyzerConfigOptionsProvider options, ImmutableArray<object>? plugIns = null ) :
            this( new OptionsAdapter( options ), plugIns ) { }

        public BuildOptions( AnalyzerConfigOptions options, ImmutableArray<object>? plugIns = null ) : this( new OptionsAdapter( options ), plugIns ) { }

        public bool CompileTimeAttachDebugger => this.GetBooleanOption( "DebugCaravela" );

        public bool DesignTimeAttachDebugger => this.GetBooleanOption( "DebugCaravelaDesignTime" );

        public bool MapPdbToTransformedCode => this.GetBooleanOption( "CaravelaDebugTransformedCode" );

        public string? CompileTimeProjectDirectory => this.GetStringOption( "CaravelaCompileTimeProjectDirectory" );

        public string? CrashReportDirectory => this.GetStringOption( "CaravelaCrashReportDirectory" );

        public string CacheDirectory => this.GetStringOption( "CaravelaCacheDirectory" ) ?? Path.Combine( Path.GetTempPath(), "Caravela", "Cache", AssemblyMetadataReader.MainBuildId );

        public string ProjectId => this.GetStringOption( "CaravelaProjectId" ) ?? this._defaultProjectId;

        public ImmutableArray<object> PlugIns { get; }

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