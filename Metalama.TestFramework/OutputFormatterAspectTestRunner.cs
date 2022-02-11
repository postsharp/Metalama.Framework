// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Metalama.TestFramework
{
    // Temporary
    internal class OutputFormatterAspectTestRunnerFactory : ITestRunnerFactory
    {
        public BaseTestRunner CreateTestRunner(
            ServiceProvider serviceProvider,
            string? projectDirectory,
            MetadataReference[] metadataReferences,
            ITestOutputHelper? logger )
            => new OutputFormatterAspectTestRunner( serviceProvider, projectDirectory, metadataReferences, logger );
    }

    internal class OutputFormatterAspectTestRunner : AspectTestRunner
    {
        public OutputFormatterAspectTestRunner( ServiceProvider serviceProvider, string? projectDirectory, IEnumerable<MetadataReference> metadataReferences, ITestOutputHelper? logger )
            : base( serviceProvider.WithService(new OptionsWrapper(serviceProvider.GetRequiredService<IProjectOptions>() ) ), projectDirectory, metadataReferences, logger )
        {
        }

        protected override Task RunAsync( TestInput testInput, TestResult testResult, Dictionary<string, object?> state )
        {
            return base.RunAsync( testInput, testResult, state );
        }

        private class OptionsWrapper : IProjectOptions
        {
            private readonly IProjectOptions _underlying;

            public OptionsWrapper( IProjectOptions underlying )
            {
                this._underlying = underlying;
            }

            public string ProjectId => this._underlying.ProjectId;

            public string? BuildTouchFile => this._underlying.BuildTouchFile;

            public string? SourceGeneratorTouchFile => this._underlying.SourceGeneratorTouchFile;

            public string? AssemblyName => this._underlying.AssemblyName;

            public ImmutableArray<object> PlugIns => this._underlying.PlugIns;

            public bool IsFrameworkEnabled => this._underlying.IsFrameworkEnabled;

            public bool FormatOutput => true;

            public bool FormatCompileTimeCode => this._underlying.FormatCompileTimeCode;

            public bool IsUserCodeTrusted => this._underlying.IsUserCodeTrusted;

            public string? ProjectPath => this._underlying.ProjectPath;

            public string? TargetFramework => this._underlying.TargetFramework;

            public string? Configuration => this._underlying.Configuration;

            public bool IsDesignTimeEnabled => this._underlying.IsDesignTimeEnabled;

            public string? AdditionalCompilationOutputDirectory => this._underlying.AdditionalCompilationOutputDirectory;

            public IProjectOptions Apply( IProjectOptions options )
            {
                return new OptionsWrapper( options );
            }

            public bool TryGetProperty( string name, [NotNullWhen( true )] out string? value )
            {
                if (name == nameof(this.FormatOutput))
                {
                    value = "true";
                    return true;
                }
                else
                {
                    return this._underlying.TryGetProperty( name, out value );
                }
            }
        }
    }
}
