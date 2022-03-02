// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Metalama.TestFramework
{
    // This whole file is temporary before we have FormatOutput in TestOptions properly controlling FormatOutput in ProjectOptions.
    // Additionally rewriting the test input before running the aspect pipeline test runner does not seem to be.

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name
    internal class OutputFormatterAspectTestRunnerFactory : ITestRunnerFactory
#pragma warning restore SA1649 // File name should match first type name
#pragma warning restore SA1402 // File may only contain a single type
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
        public OutputFormatterAspectTestRunner(
            ServiceProvider serviceProvider,
            string? projectDirectory,
            IEnumerable<MetadataReference> metadataReferences,
            ITestOutputHelper? logger )
            : base(
                serviceProvider.WithService( new OptionsWrapper( serviceProvider.GetRequiredService<IProjectOptions>() ) ),
                projectDirectory,
                metadataReferences,
                logger ) 
        {
        }

        protected override Task RunAsync( TestInput testInput, TestResult testResult, Dictionary<string, object?> state )
        {
            var expectedEol =
                testInput.Options.ExpectedEndOfLine switch
                {
                    null => null,
                    "CR" => "\r",
                    "LF" => "\n",
                    "CRLF" => "\r\n",
                    _ => throw new AssertionFailedException(),
                };

            if ( expectedEol != null)
            {
                var sb = new StringBuilder();

                for ( var i = 0; i < testInput.SourceCode.Length; i++)
                {
                    var current = testInput.SourceCode[i];
                    var next = (i < testInput.SourceCode.Length - 1) ? testInput.SourceCode[i + 1] : (char?)null;

                    switch ( (current, next) )
                    {
                        case ('\r', '\n' ):
                            sb.Append( expectedEol );
                            i++;
                            break;
                        case ('\r', _ ):
                        case ('\n', _ ):
                            sb.Append( expectedEol );
                            break;
                        default:
                            sb.Append( current );
                            break;
                    }
                }

                testInput = testInput.WithSource(sb.ToString());
            }

            var result = base.RunAsync( testInput, testResult, state );

            if ( expectedEol != null && testResult.OutputProject != null )
            {
                foreach ( var sourceDocument in testResult.OutputProject.Documents )
                {
                    var outputSource = sourceDocument.GetTextAsync().Result.ToString();

                    for ( var i = 0; i < outputSource.Length; i++ )
                    {
                        var current = outputSource[i];
                        var next = (i < outputSource.Length - 1) ? outputSource[i + 1] : (char?) null;

                        static string MapEolToString( string value ) => value switch
                        {
                            "\r\n" => "\\r\\n",
                            "\r" => "\\r",
                            "\n" => "\\n",
                            _ => throw new AssertionFailedException(),
                        };

                        var error = false;

                        switch ( (current, next) )
                        {
                            case ('\r', '\n' ):
                                if ( expectedEol != "\r\n" )
                                {
                                    error = true;
                                    testResult.SetFailed( $"ERROR: Expected \"{MapEolToString( expectedEol )}\" end of lines, but got \"\\r\\n\"." );
                                }

                                i++;
                                break;
                            case ('\r', _ ):
                            case ('\n', _ ):
                                if ( expectedEol.Length > 1 || expectedEol[0] != current )
                                {
                                    error = true;
                                    testResult.SetFailed( $"ERROR: Expected \"{MapEolToString( expectedEol )}\" end of lines, but got \"{MapEolToString( $"{current}" )}\"." );
                                }

                                break;
                            default:
                                break;
                        }

                        if ( error )
                        {
                            break;
                        }
                    }
                }
            }

            return result;
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
                if ( name == nameof(this.FormatOutput) )
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