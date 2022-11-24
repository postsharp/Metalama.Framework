// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Testing;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
            ProjectServiceProvider serviceProvider,
            string? projectDirectory,
            TestProjectReferences references,
            ITestOutputHelper? logger )
            => new OutputFormatterAspectTestRunner( serviceProvider, projectDirectory, references, logger );
    }

    internal class OutputFormatterAspectTestRunner : AspectTestRunner
    {
        public OutputFormatterAspectTestRunner(
            ProjectServiceProvider serviceProvider,
            string? projectDirectory,
            TestProjectReferences references,
            ITestOutputHelper? logger )
            : base(
                serviceProvider,
                projectDirectory,
                references,
                logger ) { }

        protected override IProjectOptions GetProjectOptions( TestProjectOptions options ) => new FormattingTestProjectOptions( options );

        protected override Task RunAsync( TestInput testInput, TestResult testResult, IProjectOptions projectOptions, Dictionary<string, object?> state )
        {
            var expectedEol =
                testInput.Options.ExpectedEndOfLine switch
                {
                    null => null,
                    "CR" => "\r",
                    "LF" => "\n",
                    "CRLF" => "\r\n",
                    _ => throw new AssertionFailedException(
                        $"Unexpected value for the ExpectedEndOfLine test option: '{testInput.Options.ExpectedEndOfLine}'." )
                };

            if ( expectedEol != null )
            {
                var sb = new StringBuilder();

                for ( var i = 0; i < testInput.SourceCode.Length; i++ )
                {
                    var current = testInput.SourceCode[i];
                    var next = i < testInput.SourceCode.Length - 1 ? testInput.SourceCode[i + 1] : (char?) null;

                    switch (current, next)
                    {
                        case ('\r', '\n'):
                            sb.Append( expectedEol );
                            i++;

                            break;

                        case ('\r', _):
                        case ('\n', _):
                            sb.Append( expectedEol );

                            break;

                        default:
                            sb.Append( current );

                            break;
                    }
                }

                testInput = testInput.WithSource( sb.ToString() );
            }

            var result = base.RunAsync( testInput, testResult, projectOptions, state );

            if ( expectedEol != null && testResult.OutputProject != null )
            {
                foreach ( var sourceDocument in testResult.OutputProject.Documents )
                {
                    var outputSource = sourceDocument.GetTextAsync().Result.ToString();

                    for ( var i = 0; i < outputSource.Length; i++ )
                    {
                        var current = outputSource[i];
                        var next = i < outputSource.Length - 1 ? outputSource[i + 1] : (char?) null;

                        static string MapEolToString( string value )
                            => value switch
                            {
                                "\r\n" => "\\r\\n",
                                "\r" => "\\r",
                                "\n" => "\\n",
                                _ => throw new AssertionFailedException( $"Unexpected EOL value: '{string.Join( " ", value.Select( x => (int) x ) )}'" )
                            };

                        var error = false;

                        switch (current, next)
                        {
                            case ('\r', '\n'):
                                if ( expectedEol != "\r\n" )
                                {
                                    error = true;
                                    testResult.SetFailed( $"ERROR: Expected \"{MapEolToString( expectedEol )}\" end of lines, but got \"\\r\\n\"." );
                                }

                                i++;

                                break;

                            case ('\r', _):
                            case ('\n', _):
                                if ( expectedEol.Length > 1 || expectedEol[0] != current )
                                {
                                    error = true;

                                    testResult.SetFailed(
                                        $"ERROR: Expected \"{MapEolToString( expectedEol )}\" end of lines, but got \"{MapEolToString( $"{current}" )}\"." );
                                }

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

        private class FormattingTestProjectOptions : ProjectOptionsWrapper
        {
            public FormattingTestProjectOptions( IProjectOptions underlying ) : base( underlying ) { }

            public override bool FormatOutput => true;

            public override bool IsTest => true;

            public override IProjectOptions Apply( IProjectOptions options )
            {
                return new FormattingTestProjectOptions( options );
            }

            public override bool TryGetProperty( string name, [NotNullWhen( true )] out string? value )
            {
                if ( name == nameof(this.FormatOutput) )
                {
                    value = "true";

                    return true;
                }
                else
                {
                    return this.Wrapped.TryGetProperty( name, out value );
                }
            }
        }
    }
}