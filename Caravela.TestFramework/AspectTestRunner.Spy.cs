// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace Caravela.TestFramework
{
    public partial class AspectTestRunner
    {
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