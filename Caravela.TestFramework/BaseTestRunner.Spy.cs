// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Observers;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Caravela.TestFramework
{
    public partial class BaseTestRunner
    {
        protected class Observer : ICompileTimeCompilationBuilderObserver, ITemplateCompilerObserver, ICompilationModelObserver
        {
            private readonly TestResult _testResult;

            public Observer( TestResult testResult )
            {
                this._testResult = testResult;
            }

            public void OnCompileTimeCompilation( Compilation compilation )
                => Task.Run( () => this._testResult.SetCompileTimeCompilationAsync( compilation ) ).Wait();

            public void OnAnnotatedSyntaxNode( SyntaxNode sourceSyntaxRoot, SyntaxNode annotatedSyntaxRoot )
            {
                var originalSyntaxTree =
                    this._testResult.SyntaxTrees
                        .Select( ( item, index ) => (item, index) )
                        .Single( x => x.item.InputSyntaxTree.FilePath == sourceSyntaxRoot.SyntaxTree.FilePath )
                        .item;

                var previousRoot = originalSyntaxTree.AnnotatedSyntaxRoot ?? sourceSyntaxRoot.SyntaxTree.GetRoot();

                originalSyntaxTree.AnnotatedSyntaxRoot = previousRoot.ReplaceNode( sourceSyntaxRoot, annotatedSyntaxRoot );
            }

            public void OnInitialCompilationModelCreated( ICompilation compilation ) => this._testResult.InitialCompilationModel = compilation;
        }
    }
}