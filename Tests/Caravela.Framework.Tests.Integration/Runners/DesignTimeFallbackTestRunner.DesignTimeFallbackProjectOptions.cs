// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Options;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Tests.Integration.Runners
{
    internal partial class DesignTimeFallbackTestRunner
    {
        private class DesignTimeFallbackProjectOptions : IProjectOptions
        {
            private readonly IProjectOptions _underlying;

            public string ProjectId => this._underlying.ProjectId;

            public string? BuildTouchFile => this._underlying.BuildTouchFile;

            public string? AssemblyName => this._underlying.AssemblyName;

            public ImmutableArray<object> PlugIns => this._underlying.PlugIns;

            public bool IsFrameworkEnabled => this._underlying.IsFrameworkEnabled;

            public bool FormatOutput => this._underlying.FormatOutput;

            public bool FormatCompileTimeCode => this._underlying.FormatCompileTimeCode;

            public bool IsUserCodeTrusted => this._underlying.IsUserCodeTrusted;

            public string? ProjectPath => this._underlying.ProjectPath;

            public string? TargetFramework => this._underlying.TargetFramework;

            public string? Configuration => this._underlying.Configuration;

            public bool DesignTimeEnabled => false;

            public string? AuxiliaryFilePath => null;

            public bool DebugCompilerProcess => this._underlying.DebugCompilerProcess;

            public bool DebugAnalyzerProcess => this._underlying.DebugAnalyzerProcess;

            public bool DebugIdeProcess => this._underlying.DebugIdeProcess;

            public DesignTimeFallbackProjectOptions( IProjectOptions underlying )
            {
                this._underlying = underlying;
            }

            public IProjectOptions Apply( IProjectOptions options )
            {
                throw new NotSupportedException();
            }

            public bool TryGetProperty( string name, [NotNullWhen( true )] out string? value )
            {
                return this._underlying.TryGetProperty( name, out value );
            }
        }
    }
}