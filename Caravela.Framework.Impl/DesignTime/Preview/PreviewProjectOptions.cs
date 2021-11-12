// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Options;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Impl.DesignTime.Preview
{
    internal class PreviewProjectOptions : IProjectOptions
    {
        private readonly IProjectOptions _underlying;

        public PreviewProjectOptions( IProjectOptions underlying )
        {
            this._underlying = underlying;
        }

        bool IDebuggingOptions.DebugCompilerProcess => false;

        bool IDebuggingOptions.DebugAnalyzerProcess => false;

        bool IDebuggingOptions.DebugIdeProcess => false;

        string IProjectOptions.ProjectId => this._underlying.ProjectId;

        string? IProjectOptions.BuildTouchFile => this._underlying.BuildTouchFile;

        string? IProjectOptions.AssemblyName => this._underlying.AssemblyName;

        ImmutableArray<object> IProjectOptions.PlugIns => this._underlying.PlugIns;

        bool IProjectOptions.IsFrameworkEnabled => true;

        bool IProjectOptions.FormatOutput => true;

        bool IProjectOptions.FormatCompileTimeCode => false;

        bool IProjectOptions.IsUserCodeTrusted => true;

        string? IProjectOptions.ProjectPath => this._underlying.ProjectPath;

        string? IProjectOptions.TargetFramework => this._underlying.TargetFramework;

        string? IProjectOptions.Configuration => this._underlying.Configuration;

        IProjectOptions IProjectOptions.Apply( IProjectOptions options ) => throw new NotSupportedException();

        bool IProjectOptions.TryGetProperty( string name, [NotNullWhen( true )] out string? value ) => this._underlying.TryGetProperty( name, out value );

        bool IProjectOptions.DesignTimeEnabled => true;

        string? IProjectOptions.AuxiliaryFileDirectoryPath => null;
    }
}