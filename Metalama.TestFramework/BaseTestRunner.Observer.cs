// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Observers;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.TestFramework
{
    public partial class BaseTestRunner
    {
        protected class Observer : ICompileTimeCompilationBuilderObserver, ITemplateCompilerObserver, ICompilationModelObserver, ILinkerObserver
        {
            private readonly TestResult _testResult;

            public Observer( TestResult testResult )
            {
                this._testResult = testResult;
            }

            public void OnCompileTimeCompilation( Compilation compilation )
                => Task.Run( () => this._testResult.SetCompileTimeCompilationAsync( compilation ) ).Wait();

            public void OnCompileTimeCompilationEmit( Compilation compilation, ImmutableArray<Diagnostic> diagnostics )
                => this._testResult.CompileTimeCompilationDiagnostics.Report( diagnostics );

            public void OnAnnotatedSyntaxNode( SyntaxNode sourceSyntaxRoot, SyntaxNode annotatedSyntaxRoot )
            {
                var originalSyntaxTree =
                    this._testResult.SyntaxTrees
                        .Select( ( item, index ) => (item, index) )
                        .Single( x => x.item.InputSyntaxTree.FilePath == sourceSyntaxRoot.SyntaxTree.FilePath )
                        .item;

                SyntaxNode previousRoot;
                SyntaxNode previousNode;

                if ( originalSyntaxTree.AnnotatedSyntaxRoot == null )
                {
                    // This is the first time we are called.
                    previousRoot = sourceSyntaxRoot.SyntaxTree.GetRoot();
                    previousNode = sourceSyntaxRoot;
                }
                else
                {
                    // This is the second time we are called. We need to locate the node in the tree we created the previous
                    // time we were called.
                    previousRoot = originalSyntaxTree.AnnotatedSyntaxRoot;
                    Assert.True( NodeFinder.TryFindOldNodeInNewRoot( sourceSyntaxRoot, previousRoot, out previousNode ) );
                }

                originalSyntaxTree.AnnotatedSyntaxRoot = previousRoot.ReplaceNode( previousNode, annotatedSyntaxRoot );

                Assert.NotSame( originalSyntaxTree.AnnotatedSyntaxRoot, previousRoot );
            }

            public void OnInitialCompilationModelCreated( ICompilation compilation ) => this._testResult.InitialCompilationModel = compilation;

            public void OnIntermediateCompilationCreated( PartialCompilation compilation ) => this._testResult.IntermediateLinkerCompilation = compilation;
        }
    }
}