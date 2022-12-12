﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text.RegularExpressions;

namespace Metalama.Testing.AspectTesting
{
    /// <summary>
    /// A set of test options, which can be included in the source text of tests using special comments like <c>// @ReportOutputWarnings</c>.
    /// This class is JSON-serializable. Another way to define options is to add a file named <c>metalamaTests.json</c> into the test directory or
    /// any parent directory.
    /// </summary>
    public class TestOptions
    {
        private static readonly Regex _optionRegex = new( @"^\s*//\s*@(?<name>\w+)\s*(\((?<arg>[^\)]*)\))?", RegexOptions.Multiline );
        private readonly List<string> _invalidSourceOptions = new();
        private bool? _writeOutputHtml;

        /// <summary>
        /// Gets or sets the reason for which the test must be skipped, or <c>null</c> if the test must not be skipped.
        /// To skip a test, add this comment to your test file: <c>// @Skipped(reason)</c>. 
        /// </summary>
        public string? SkipReason { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current test must be skipped.
        /// </summary>
        public bool IsSkipped => this.SkipReason != null;

        /// <summary>
        /// Gets or sets a value indicating whether the diagnostics of the compilation of the transformed target code should be included in the test result.
        /// This is useful when diagnostic suppression is being tested.
        /// To enable this option in a test, add this comment to your test file: <c>// @ReportOutputWarnings</c>. 
        /// </summary>
        public bool? ReportOutputWarnings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the output file must be compiled into a binary (e.g. emitted).
        /// To enable this option in a test, add this comment to your test file: <c>// @OutputCompilationDisabled</c>.
        /// </summary>
        public bool? OutputCompilationDisabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether diagnostics of all severities should be included in the rest result. By default, only
        /// warnings and errors are included.
        /// To enable this option in a test, add this comment to your test file: <c>// @IncludeAllSeverities</c>.  
        /// </summary>
        public bool? IncludeAllSeverities { get; set; }

        /// <summary>
        /// Gets or sets the fully-qualified name of the test runner factory type (implementing <see cref="ITestRunnerFactory"/>).
        /// You can only define this option in the <c>metalamaTests.json</c> file of a directory. This setting is for Metalama internal use only.
        /// </summary>
        public string? TestRunnerFactoryType { get; set; }

        /// <summary>
        /// Gets the list of assembly names that should be included in the compilation.
        /// To add a named assembly reference, add this comment to your test file: <c>// @AssemblyReference(assemblyName)</c>.
        /// </summary>
        public List<TestAssemblyReference> References { get; } = new();

        /// <summary>
        /// Gets the list of source code files that should be included in the compilation.
        /// To enable this option in a test, add this comment to your test file: <c>// @IncludedFiles(relativePath)</c>. 
        /// </summary>
        public List<string> IncludedFiles { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether HTML of syntax-highlighted files should be produced for input files. If <c>true</c>, these files
        /// are created to the <c>obj/html</c> directory.
        /// To enable this option in a test, add this comment to your test file: <c>// @WriteInputHtml</c>. 
        /// </summary>
        public bool? WriteInputHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether HTML of syntax-highlighted files should be produced for the consolidated output file. If <c>true</c>, this file
        /// is created to the <c>obj/html</c> directory. Setting this property to <c>true</c> automatically sets the <see cref="FormatOutput"/> property to <c>true</c>. 
        /// To enable this option in a test, add this comment to your test file: <c>// @WriteOutputHtml</c>. 
        /// </summary>
        public bool? WriteOutputHtml
        {
            get => this._writeOutputHtml;

            set
            {
                this._writeOutputHtml = value;

                if ( value.GetValueOrDefault() )
                {
                    this.FormatOutput = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether titles (tooltips) should be added to HTML files.
        /// To enable this option in a test, add this comment to your test file: <c>// @AddHtmlTitles</c>. 
        /// </summary>
        public bool? AddHtmlTitles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="TestResult.ErrorMessage"/> should be added to
        /// the test output.
        /// You can only define this option in the <c>metalamaTests.json</c> file of a directory. 
        /// </summary>
        public bool? ReportErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the output <c>t.cs</c> file should be formatted, which includes simplifying the code and adding <c>using</c> directives. The default behavior is <c>true</c>.
        /// To enable this option in a test, add this comment to your test file: <c>// @FormatOutput</c>.
        /// </summary>
        public bool? FormatOutput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether whitespace are taken into account while comparing the the expected output <c>t.cs</c> file with the actual output. The default behavior is <c>false</c>.
        /// To enable this option in a test, add this comment to your test file: <c>// @PreserveWhitespace</c>.
        /// </summary>
        public bool? PreserveWhitespace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether C# nullability is disabled for the compilation.
        /// To enable this option in a test, add this comment to your test file: <c>// @NullabilityDisabled</c>.
        /// </summary>
        public bool? NullabilityDisabled { get; set; }

        /// <summary>
        /// Gets a list of warnings that are not reported even if <see cref="ReportOutputWarnings"/> is set to <c>true</c>.
        /// To add an item into this collection from a test, add this comment to your test file: <c>// @IgnoredDiagnostic(id)</c>.
        /// </summary>
        public List<string> IgnoredDiagnostics { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether the list of <see cref="IgnoredDiagnostics"/> inherited from the parent level (directory or base directory)
        /// must be cleared before new diagnostics are added to this list. This option is not inherited from the base level.
        /// To enable this option in a test, add this comment to your test file: <c>// @ClearIgnoredDiagnostics</c>.
        /// </summary>
        public bool? ClearIgnoredDiagnostics { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the test is allowed to have compile-time code that has dynamic calls.
        /// To enable this option in a test, add this comment to your test file: <c>// @AllowCompileTimeDynamicCode</c>.
        /// </summary>
        public bool? AllowCompileTimeDynamicCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the `Program.Main` method should be executed if it exists. The default value is <c>true</c>.
        /// To enable this option in a test, add this comment to your test file: <c>// @ExecuteProgram</c>.
        /// </summary>
        public bool? ExecuteProgram { get; set; }

        /// <summary>
        /// Gets or sets a value indicating which type of the output assembly should be used for the test. Currently valid values are <c>Dll</c> <c>Exe</c> (default).
        /// </summary>
        public string? OutputAssemblyType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the test should be executed even if the input compilation has errors.
        /// To enable this option in a test, add this comment to your test file: <c>// @AcceptInvalidInput</c>.
        /// </summary>
        public bool? AcceptInvalidInput { get; set; }

        /// <summary>
        /// Gets the set of preprocessor symbols that are required for this test, otherwise the test would be skipped.
        /// To add an item into this collection from a test, add this comment to your test file: <c>// @RequiredConstant(constant)</c>.
        /// </summary>
        public List<string> RequiredConstants { get; } = new();

        /// <summary>
        /// Gets the set of preprocessor symbols that are defined for this test.
        /// To add an item into this collection from a test, add this comment to your test file: <c>// @DefinedConstant(constant)</c>.
        /// All constants of the test project and TESTRUNNER and METALAMA are defined by default.
        /// /// Constants added via <see cref="DependencyDefinedConstants"/> option are not added.
        /// </summary>
        public List<string> DefinedConstants { get; } = new();

        /// <summary>
        /// Gets the set of preprocessor symbols that are defined for this test dependency.
        /// To add an item into this collection from a test, add this comment to your test file: <c>// @DependencyDefinedConstant(constant)</c>.
        /// All constants of the test project and TESTRUNNER and METALAMA are defined by default.
        /// Constants added via <see cref="DefinedConstants"/> option are not added.
        /// </summary>
        public List<string> DependencyDefinedConstants { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating the test scenario.
        /// See <see cref="TestScenario"/> enum values for details.
        /// </summary>
        public TestScenario? TestScenario { get; set; }

        /// <summary>
        /// Gets or sets the zero-based index of the code fix to be applied
        /// when <see cref="TestScenario"/> is set to <see cref="Metalama.Testing.AspectTesting.TestScenario.ApplyCodeFix"/> or <see cref="Metalama.Testing.AspectTesting.TestScenario.PreviewCodeFix"/>.
        /// To set this option in a test, add this comment to your test file: <c>// @AppliedCodeFixIndex(id)</c>.
        /// </summary>
        public int? AppliedCodeFixIndex { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether disabled code should be kept as trivia.
        /// To set this option in a test, add this comment to your test file: <c>// @KeepDisabledCode</c>.
        /// </summary>
        public bool? KeepDisabledCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating which end-of-line sequence is expected.
        /// To set this option in a test, add this comment to your test file: <c>// @ExpectedEndOfLine(eol)</c> where EOL is <c>CR</c>, <c>LF</c> or <c>CRLF</c>.
        /// </summary>
        public string? ExpectedEndOfLine { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all input syntax trees should be used as a part of test output. 
        /// If <c>false</c> only the primary syntax tree is processed and used as test output. If <c>true</c>, all syntax trees are processed and used as test output.
        /// To set this option in a test, add this comment to your test file: <c>// @OutputAllSyntaxTrees</c>.
        /// </summary>
        public bool? OutputAllSyntaxTrees { get; set; }

        /// <summary>
        /// Gets or sets the version of the C# language that the test should be compiled with.
        /// To set this option in a test, add this comment to your test file: <c>// @LanguageVersion(version)</c>.
        /// </summary>
        public LanguageVersion? LanguageVersion { get; set; }

        /// <summary>
        /// Gets or sets the list of C# language features that the test should be compiled with.
        /// To set this option in a test, add this comment to your test file: <c>// @LanguageFeature(feature)</c> or <c>// @LanguageFeature(feature=value)</c>.
        /// </summary>
        public ImmutableDictionary<string, string> LanguageFeatures { get; set; } = ImmutableDictionary<string, string>.Empty;

        /// <summary>
        /// Gets or sets the name of a file in the project directory containing the license key.
        /// To set this option in a test, add this comment to your test file: <c>// @LicenseFile(file)</c>.
        /// </summary>
        public string? LicenseFile { get; set; }

        /// <summary>
        /// Gets or sets the name of a file in the project directory containing the license key to be used to compile the dependency.
        /// To set this option in a test, add this comment to your test file: <c>// @DependencyLicenseFile(file)</c>.
        /// </summary>
        public string? DependencyLicenseFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an error should be reported if the compilation uses aspects that
        /// are not explicitly ordered.
        /// To enable this option in a test, add this comment to your test file: <c>// @RequireOrderedAspects</c>. 
        /// </summary>
        public bool? RequireOrderedAspects { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Metalama should produce logs and output them to the Xunit test log.
        /// To enable this option in a test, add this comment to your test file: <c>// @EnableLogging</c>. 
        /// </summary>
        public bool? EnableLogging { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified name of expected exception type to be thrown.
        /// To set this option in a test, add this comment to your test file: <c>// @ExpectedException(fully qualified exception type name)</c>.
        /// </summary>
        public string? ExpectedException { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the <c>Main</c> method. The default value of this property is <c>Main</c>. This option
        /// is useful to work around the <c>CS0017</c> error.
        /// To set this option in a test, add this comment to your test file: <c>// @MainMethod(name)</c>. 
        /// </summary>
        public string? MainMethod { get; set; }

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

            this.PreserveWhitespace ??= baseOptions.PreserveWhitespace;

            this.IncludedFiles.AddRange( baseOptions.IncludedFiles );

            this.References.AddRange( baseOptions.References );

            this.AllowCompileTimeDynamicCode ??= baseOptions.AllowCompileTimeDynamicCode;

            this.ExecuteProgram ??= baseOptions.ExecuteProgram;

            this.OutputAssemblyType ??= baseOptions.OutputAssemblyType;

            this.AcceptInvalidInput ??= baseOptions.AcceptInvalidInput;

            this.TestScenario ??= baseOptions.TestScenario;

            this.KeepDisabledCode ??= baseOptions.KeepDisabledCode;

            this.AppliedCodeFixIndex ??= baseOptions.AppliedCodeFixIndex;

            if ( !this.ClearIgnoredDiagnostics.GetValueOrDefault() )
            {
                this.IgnoredDiagnostics.AddRange( baseOptions.IgnoredDiagnostics );
            }

            this.RequiredConstants.AddRange( baseOptions.RequiredConstants );

            this.DefinedConstants.AddRange( baseOptions.DefinedConstants );

            this.DependencyDefinedConstants.AddRange( baseOptions.DependencyDefinedConstants );

            this.OutputAllSyntaxTrees ??= baseOptions.OutputAllSyntaxTrees;

            this.LicenseFile ??= baseOptions.LicenseFile;

            this.DependencyLicenseFile ??= baseOptions.DependencyLicenseFile;

            this.RequireOrderedAspects ??= baseOptions.RequireOrderedAspects;

            this.EnableLogging ??= baseOptions.EnableLogging;

            this.ExpectedException ??= baseOptions.ExpectedException;

            this.MainMethod ??= baseOptions.MainMethod;
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

                    case "TestScenario":
                        if ( Enum.TryParse<TestScenario>( optionArg, out var testScenario ) )
                        {
                            this.TestScenario = testScenario;

                            switch ( testScenario )
                            {
                                case AspectTesting.TestScenario.PreviewLiveTemplate:
                                    this.TestRunnerFactoryType =
                                        "Metalama.Framework.Tests.Integration.Runners.LiveTemplateTestRunnerFactory, Metalama.Framework.Tests.Integration.Internals";

                                    break;

                                case AspectTesting.TestScenario.ApplyLiveTemplate:
                                    this.TestRunnerFactoryType =
                                        "Metalama.Framework.Tests.Integration.Runners.LiveTemplateTestRunnerFactory, Metalama.Framework.Tests.Integration.Internals";

                                    break;
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"'{optionArg} is not a TestScenario value. Use one of following: {Enum.GetValues( typeof(TestScenario) )}." );
                        }

                        break;

                    case "WriteInputHtml":
                        this.WriteInputHtml = true;

                        break;

                    case "WriteOutputHtml":
                        this.WriteOutputHtml = true;

                        break;

                    case "AddHtmlTitles":
                        this.AddHtmlTitles = true;

                        break;

                    case "FormatOutput":
                        this.FormatOutput = true;

                        break;

                    case "PreserveWhitespace":
                        this.PreserveWhitespace = true;

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

                    case "DefinedConstant":
                        this.DefinedConstants.Add( optionArg );

                        break;

                    case "DependencyDefinedConstant":
                        this.DependencyDefinedConstants.Add( optionArg );

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

                    case "AppliedCodeFixIndex":
                        if ( int.TryParse( optionArg, out var index ) )
                        {
                            this.AppliedCodeFixIndex = index;
                        }
                        else
                        {
                            throw new InvalidOperationException( $"'{optionArg} is not a valid code fix index number." );
                        }

                        break;

                    case "KeepDisabledCode":
                        this.KeepDisabledCode = true;

                        break;

                    case "ExecuteProgram":
                        this.ExecuteProgram = true;

                        break;

                    case "OutputAssemblyType":
                        this.OutputAssemblyType = optionArg;

                        break;

                    case "ExpectedEndOfLine":
                        this.ExpectedEndOfLine = optionArg;

                        break;

                    case "OutputAllSyntaxTrees":
                        this.OutputAllSyntaxTrees = true;

                        break;

                    case "AssemblyReference":
                        this.References.Add( new TestAssemblyReference { Name = optionArg } );

                        break;

                    case "LanguageVersion":
                        if ( LanguageVersionFacts.TryParse( optionArg, out var result ) )
                        {
                            this.LanguageVersion = result;
                        }
                        else
                        {
                            throw new InvalidOperationException( $"'{optionArg} is not a valid language version." );
                        }

                        break;

                    case "LanguageFeature":
                        {
                            var parts = optionArg.Split( '=' );

                            if ( parts.Length == 1 )
                            {
                                this.LanguageFeatures = this.LanguageFeatures.SetItem( parts[0], "" );
                            }
                            else
                            {
                                this.LanguageFeatures = this.LanguageFeatures.SetItem( parts[0], parts[1] );
                            }
                        }

                        break;

                    case "LicenseFile":

                        this.LicenseFile = optionArg;

                        break;

                    case "DependencyLicenseFile":

                        this.DependencyLicenseFile = optionArg;

                        break;

                    case "ExpectedException":

                        this.ExpectedException = optionArg;

                        break;

                    case "RequireOrderedAspects":
                        this.RequireOrderedAspects = true;

                        break;

                    case "EnableLogging":
                        this.EnableLogging = true;

                        break;
                    
                    case "MainMethod":
                        this.MainMethod = optionArg;
                        
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