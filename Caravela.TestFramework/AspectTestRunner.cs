﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Executes aspect integration tests by running the full aspect pipeline on the input source file.
    /// </summary>
    public class AspectTestRunner : BaseTestRunner
    {
        public AspectTestRunner( IServiceProvider serviceProvider, string? projectDirectory = null, IEnumerable<Assembly>? additionalAssemblies = null )
            : base( serviceProvider, projectDirectory, additionalAssemblies ) { }

        /// <summary>
        /// Runs the aspect test with the given name and source.
        /// </summary>
        /// <returns>The result of the test execution.</returns>
        public override TestResult RunTest( TestInput testInput )
        {
            var testResult = base.RunTest( testInput );

            using var buildOptions = new TestProjectOptions();
            using var domain = new UnloadableCompileTimeDomain();

            var pipeline = new CompileTimeAspectPipeline( buildOptions, domain );
            var spy = new Spy( testResult );
            pipeline.ServiceProvider.AddService<ICompileTimeCompilationBuilderSpy>( spy );
            pipeline.ServiceProvider.AddService<ITemplateCompilerSpy>( spy );

            if ( pipeline.TryExecute( testResult, testResult.InitialCompilation!, CancellationToken.None, out var resultCompilation, out _ ) )
            {
                testResult.ResultCompilation = resultCompilation;
                var syntaxRoot = resultCompilation.SyntaxTrees.Single().GetRoot();

                if ( testInput.Options.IncludeFinalDiagnostics.GetValueOrDefault() )
                {
                    testResult.Report( resultCompilation.GetDiagnostics().Where( d => d.Severity >= DiagnosticSeverity.Warning ) );
                }

                testResult.SetTransformedTarget( syntaxRoot );
            }
            else
            {
                testResult.SetFailed( "CompileTimeAspectPipeline.TryExecute failed." );
            }

            return testResult;
        }

        // We don't want the base class to report errors in the input compilation because the pipeline does.
        protected override bool ReportInvalidInputCompilation => false;

        private class Spy : ICompileTimeCompilationBuilderSpy, ITemplateCompilerSpy
        {
            private readonly TestResult _testResult;
            private SyntaxNode? _annotatedSyntaxRoot;

            public Spy( TestResult testResult )
            {
                this._testResult = testResult;
            }

            public void ReportCompileTimeCompilation( Compilation compilation )
            {
                this._testResult.TransformedTemplateSyntax = compilation.SyntaxTrees.First().GetRoot();
            }

            public void ReportAnnotatedSyntaxNode( SyntaxNode sourceSyntaxRoot, SyntaxNode annotatedSyntaxRoot )
            {
                if ( this._annotatedSyntaxRoot == null )
                {
                    this._annotatedSyntaxRoot = sourceSyntaxRoot.SyntaxTree.GetRoot();
                }

                this._annotatedSyntaxRoot = this._annotatedSyntaxRoot.ReplaceNode( sourceSyntaxRoot, annotatedSyntaxRoot );
                this._testResult.AnnotatedTemplateSyntax = this._annotatedSyntaxRoot;
            }
        }
    }
}