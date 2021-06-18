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

            public Spy( TestResult testResult )
            {
                this._testResult = testResult;
            }

            public void ReportCompileTimeCompilation( Compilation compilation )
            {
                this._testResult.SetOutputCompilation( compilation );
            }

            public void ReportAnnotatedSyntaxNode( SyntaxNode sourceSyntaxRoot, SyntaxNode annotatedSyntaxRoot )
            {
                var originalSyntaxTree =
                    this._testResult.SyntaxTrees
                        .Select( ( item, index ) => (item, index) )
                        .Single( x => x.item.InputSyntaxTree.FilePath == sourceSyntaxRoot.SyntaxTree.FilePath )
                        .item;
                
                if ( originalSyntaxTree.AnnotatedSyntaxRoot == null )
                {
                    originalSyntaxTree.AnnotatedSyntaxRoot = annotatedSyntaxRoot.SyntaxTree.GetRoot();
                }

                originalSyntaxTree.AnnotatedSyntaxRoot = originalSyntaxTree.AnnotatedSyntaxRoot.ReplaceNode( sourceSyntaxRoot, annotatedSyntaxRoot );
            }
        }
    }
}