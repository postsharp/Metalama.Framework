// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Metalama.TestFramework
{
    /// <summary>
    /// A set of test options, which can be included in the source text of tests using special comments like <c>// @ReportOutputWarnings</c>.
    /// This class is JSON-serializable.
    /// </summary>
    public class TestOptions
    {
        private static readonly Regex _optionRegex = new( @"^\s*//\s*@(?<name>\w+)\s*(\((?<arg>[^\)]*)\))?", RegexOptions.Multiline );
        private readonly List<string> _invalidSourceOptions = new();

        /// <summary>
        /// Gets or sets the reason for which the test must be skipped, or <c>null</c> if the test must not be skipped. 
        /// </summary>
        public string? SkipReason { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current test must be skipped.
        /// </summary>
        public bool IsSkipped => this.SkipReason != null;

        /// <summary>
        /// Gets or sets a value indicating whether the diagnostics of the compilation of the transformed target code should be included in the test result.
        /// This is useful when diagnostic suppression is being tested.
        /// </summary>
        public bool? ReportOutputWarnings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the output file must be compiled into a binary (e.g. emitted).
        /// </summary>
        public bool? OutputCompilationDisabled { get; set; }

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
        /// Gets or sets a value indicating whether C# nullability is disabled for the compilation.
        /// </summary>
        public bool? NullabilityDisabled { get; set; }

        /// <summary>
        /// Gets a list of warnings that are not reported even if <see cref="ReportOutputWarnings"/> is set to <c>true</c>.
        /// </summary>
        public List<string> IgnoredDiagnostics { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether the list of <see cref="IgnoredDiagnostics"/> inherited from the parent level (directory or base directory)
        /// must be cleared before new diagnostics are added to this list. This option is not inherited from the base level.
        /// </summary>
        public bool? ClearIgnoredDiagnostics { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the test is allowed to have compile-time code that has dynamic calls.
        /// </summary>
        internal bool? AllowCompileTimeDynamicCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the `Program.Main` method should be executed if it exists. The default value is <c>true</c>.
        /// </summary>
        public bool? ExecuteProgram { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the test should be executed even if the input compilation has errors.
        /// </summary>
        public bool? AcceptInvalidInput { get; set; }

        /// <summary>
        /// Gets the set of preprocessor symbols that are required for this test, otherwise the test would be skipped.
        /// </summary>
        public List<string> RequiredConstants { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether a code fix should be applied. When this value is true, the output buffer
        /// of the test is not the one transformed by the aspect, but the one transformed by the code fix. The test will fail
        /// if it does not generate any diagnostic with a code fix. By default, the first emitted code fix is applied.
        /// To apply a different code fix, use the <see cref="AppliedCodeFixIndex"/> property.
        /// </summary>
        public bool? ApplyCodeFix { get; set; }

        /// <summary>
        /// Gets or sets the zero-based index of the code fix to be applied when <see cref="ApplyCodeFix"/> is <c>true</c>.
        /// </summary>
        public int? AppliedCodeFixIndex { get; set; }

        /// <summary>
        /// Applies <see cref="TestDirectoryOptions"/> to the current object by overriding any property
        /// that is not defined in the current object but defined in the argument.
        /// </summary>
        internal virtual void ApplyBaseOptions( TestDirectoryOptions baseOptions )
        {
            this.SkipReason ??= baseOptions.SkipReason;

            this.ReportOutputWarnings ??= baseOptions.ReportOutputWarnings;

            this.OutputCompilationDisabled ??= baseOptions.OutputCompilationDisabled;

            this.IncludeAllSeverities ??= baseOptions.IncludeAllSeverities;

            this.TestRunnerFactoryType ??= baseOptions.TestRunnerFactoryType;

            this.WriteInputHtml ??= baseOptions.WriteInputHtml;

            this.WriteOutputHtml ??= baseOptions.WriteOutputHtml;

            this.AddHtmlTitles ??= baseOptions.AddHtmlTitles;

            this.ReportErrorMessage ??= baseOptions.ReportErrorMessage;

            this.FormatOutput ??= baseOptions.FormatOutput;

            this.IncludedFiles.AddRange( baseOptions.IncludedFiles );

            this.References.AddRange( baseOptions.References );

            this.AllowCompileTimeDynamicCode ??= baseOptions.AllowCompileTimeDynamicCode;

            this.ExecuteProgram ??= baseOptions.ExecuteProgram;

            this.AcceptInvalidInput ??= baseOptions.AcceptInvalidInput;

            this.ApplyCodeFix ??= baseOptions.ApplyCodeFix;

            this.AppliedCodeFixIndex ??= baseOptions.AppliedCodeFixIndex;

            if ( !this.ClearIgnoredDiagnostics.GetValueOrDefault() )
            {
                this.IgnoredDiagnostics.AddRange( baseOptions.IgnoredDiagnostics );
            }

            this.RequiredConstants.AddRange( baseOptions.RequiredConstants );
        }

        public IReadOnlyList<string> InvalidSourceOptions => this._invalidSourceOptions;

        /// <summary>
        /// Parses <c>// @</c> directives from source code and apply them to the current object. 
        /// </summary>
        internal void ApplySourceDirectives( string sourceCode )
        {
            foreach ( Match? option in _optionRegex.Matches( sourceCode ) )
            {
                if ( option == null )
                {
                    continue;
                }

                var optionName = option.Groups["name"].Value;
                var optionArg = option.Groups["arg"].Value;

                switch ( optionName )
                {
                    case "ReportOutputWarnings":
                        this.ReportOutputWarnings = true;

                        break;

                    case "OutputCompilationDisabled":
                        this.OutputCompilationDisabled = true;

                        break;

                    case "IncludeAllSeverities":
                        this.IncludeAllSeverities = true;

                        break;

                    case "Skipped":
                        this.SkipReason = string.IsNullOrEmpty( optionArg ) ? "Skipped by directive in source code." : optionArg;

                        break;

                    case "Include":
                        this.IncludedFiles.Add( optionArg );

                        break;

                    case "DesignTime":
                        this.TestRunnerFactoryType =
                            "Metalama.Framework.Tests.Integration.Runners.DesignTimeTestRunnerFactory, Metalama.Framework.Tests.Integration";

                        break;

                    case "LiveTemplate":
                        this.TestRunnerFactoryType =
                            "Metalama.Framework.Tests.Integration.Runners.LiveTemplateTestRunnerFactory, Metalama.Framework.Tests.Integration.Internals";

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

                    case "NullabilityDisabled":
                        this.NullabilityDisabled = true;

                        break;

                    case "IgnoredDiagnostic":
                        this.IgnoredDiagnostics.Add( optionArg );

                        break;

                    case "RequiredConstant":
                        this.RequiredConstants.Add( optionArg );

                        break;

                    case "ClearIgnoredDiagnostics":
                        this.ClearIgnoredDiagnostics = true;

                        break;

                    case "AllowCompileTimeDynamicCode":
                        this.AllowCompileTimeDynamicCode = true;

                        break;

                    case "AcceptInvalidInput":
                        this.AcceptInvalidInput = true;

                        break;

                    case "ApplyCodeFix":
                        this.ApplyCodeFix = true;

                        if ( !string.IsNullOrEmpty( optionArg ) && int.TryParse( optionArg, out var index ) )
                        {
                            this.AppliedCodeFixIndex = index;
                        }

                        break;

                    default:
                        this._invalidSourceOptions.Add( "@" + optionName );

                        break;
                }
            }
        }

        /// <summary>
        /// Apply all relevant options for a test, both from the source code and from the <c>metalamaTests.config</c> file. 
        /// </summary>
        internal void ApplyOptions( string sourceCode, string path, TestDirectoryOptionsReader optionsReader )
        {
            this.ApplySourceDirectives( sourceCode );
            this.ApplyBaseOptions( optionsReader.GetDirectoryOptions( Path.GetDirectoryName( path )! ) );
        }
    }
}