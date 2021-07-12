// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// A set of test options, which can be included in the source text of tests using special comments like <c>// @IncludeFinalDiagnostics</c>.
    /// This class is JSON-serializable.
    /// </summary>
    public class TestOptions
    {
        private static readonly Regex _directivesRegex = new( @"^\s*//\s*@(?<name>\w+)\s*(\((?<arg>[^\)]*)\))?", RegexOptions.Multiline );

        public string? SkipReason { get; set; }

        public bool IsSkipped => this.SkipReason != null;

        /// <summary>
        /// Gets or sets a value indicating whether the diagnostics of the compilation of the transformed target code should be included in the test result.
        /// This is useful when diagnostic suppression is being tested.
        /// </summary>
        public bool? IncludeFinalDiagnostics { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether diagnostics of all severities should be included in the rest result. By default, only
        /// warnings and errors are included. 
        /// </summary>
        public bool? IncludeAllSeverities { get; set; }

        /// <summary>
        /// Gets or sets the fully-qualified name of the test runner factory type (implementing <see cref="ITestRunnerFactory"/>).
        /// </summary>
        public string? TestRunnerFactoryType { get; set; }

        /// <summary>
        /// Gets the list of assembly names that should be included in the compilation.
        /// </summary>
        public List<TestAssemblyReference> References { get; } = new();

        /// <summary>
        /// Gets the list of source code files that should be included in the compilation.
        /// </summary>
        public List<string> IncludedFiles { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether HTML of syntax-highlighted files should be produced for input files. If <c>true</c>, these files
        /// are created to the <c>obj/html</c> directory.
        /// </summary>
        public bool? WriteInputHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether HTML of syntax-highlighted files should be produced for the consolidated output file. If <c>true</c>, this file
        /// is created to the <c>obj/html</c> directory.
        /// </summary>
        public bool? WriteOutputHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether titles (tooltips) should be added to HTML files.
        /// </summary>
        public bool? AddHtmlTitles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="TestResult.ErrorMessage"/> should be added to
        /// the test output.
        /// </summary>
        public bool? ReportErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the output <c>t.cs</c> file should be formatted. The default behavior is <c>true</c>.
        /// </summary>
        public bool? FormatOutput { get; set; }

        /// <summary>
        /// Applies <see cref="TestDirectoryOptions"/> to the current object by overriding any property
        /// that is not defined in the current object but defined in the argument.
        /// </summary>
        internal virtual void ApplyDirectoryOptions( TestDirectoryOptions directoryOptions )
        {
            this.SkipReason ??= directoryOptions.SkipReason;

            this.IncludeFinalDiagnostics ??= directoryOptions.IncludeFinalDiagnostics;

            this.IncludeAllSeverities ??= directoryOptions.IncludeAllSeverities;

            this.TestRunnerFactoryType ??= directoryOptions.TestRunnerFactoryType;

            this.WriteInputHtml ??= directoryOptions.WriteInputHtml;
            
            this.WriteOutputHtml ??= directoryOptions.WriteOutputHtml;

            this.AddHtmlTitles ??= directoryOptions.AddHtmlTitles;

            this.ReportErrorMessage ??= directoryOptions.ReportErrorMessage;

            this.FormatOutput ??= directoryOptions.FormatOutput;

            this.IncludedFiles.AddRange( directoryOptions.IncludedFiles );

            this.References.AddRange( directoryOptions.References );
        }

        /// <summary>
        /// Parses <c>// @</c> directives from source code and apply them to the current object. 
        /// </summary>
        internal void ApplySourceDirectives( string sourceCode )
        {
            foreach ( Match? directive in _directivesRegex.Matches( sourceCode ) )
            {
                if ( directive == null )
                {
                    continue;
                }

                var directiveName = directive.Groups["name"].Value;
                var directiveArg = (directive.Groups["arg"]?.Value ?? "").Trim();

                switch ( directiveName )
                {
                    case "IncludeFinalDiagnostics":
                        this.IncludeFinalDiagnostics = true;

                        break;

                    case "IncludeAllSeverities":
                        this.IncludeAllSeverities = true;

                        break;

                    case "Skipped":
                        this.SkipReason = string.IsNullOrEmpty( directiveArg ) ? "Skipped by directive in source code." : directiveArg;

                        break;

                    case "Include":
                        this.IncludedFiles.Add( directiveArg );

                        break;

                    case "DesignTime":
                        this.TestRunnerFactoryType =
                            "Caravela.Framework.Tests.Integration.Runners.DesignTimeTestRunnerFactory, Caravela.Framework.Tests.PublicPipeline";

                        break;

                    case "WriteInputHtml":
                        this.WriteInputHtml = true;

                        break;
                    
                    case "WriteOutputHtml":
                        this.WriteOutputHtml = true;
                        this.FormatOutput = true;
                        
                        break;

                    case "AddHtmlTitles":
                        this.AddHtmlTitles = true;

                        break;

                    case "FormatOutput":
                        this.FormatOutput = true;

                        break;
                }
            }
        }

        /// <summary>
        /// Apply all relevant options for a test, both from the source code and from the <c>caravelaTests.config</c> file. 
        /// </summary>
        internal void ApplyOptions( string sourceCode, string path, TestDirectoryOptionsReader optionsReader )
        {
            this.ApplySourceDirectives( sourceCode );
            this.ApplyDirectoryOptions( optionsReader.GetDirectoryOptions( Path.GetDirectoryName( path )! ) );
        }
    }
}