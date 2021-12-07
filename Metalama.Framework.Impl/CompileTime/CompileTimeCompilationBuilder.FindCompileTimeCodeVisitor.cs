// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;

namespace Caravela.Framework.Impl.CompileTime
{
    internal partial class CompileTimeCompilationBuilder
    {
        /// <summary>
        /// Determines if a syntax tree has compile-time code. The result is exposed in the <see cref="HasCompileTimeCode"/> property.
        /// </summary>
        private class FindCompileTimeCodeVisitor : CSharpSyntaxWalker
        {
            private readonly SemanticModel _semanticModel;
            private readonly ISymbolClassifier _classifier;
            private readonly CancellationToken _cancellationToken;

            public bool HasCompileTimeCode { get; private set; }

            public FindCompileTimeCodeVisitor( SemanticModel semanticModel, ISymbolClassifier classifier, CancellationToken cancellationToken )
            {
                this._semanticModel = semanticModel;
                this._classifier = classifier;
                this._cancellationToken = cancellationToken;
            }

            private void VisitTypeDeclaration( SyntaxNode node )
            {
                this._cancellationToken.ThrowIfCancellationRequested();

                if ( this.HasCompileTimeCode )
                {
                    // No need to do anything more.
                    return;
                }

                var declaredSymbol = this._semanticModel.GetDeclaredSymbol( node );

                if ( declaredSymbol != null && this._classifier.GetTemplatingScope( declaredSymbol ) != TemplatingScope.RunTimeOnly )
                {
                    this.HasCompileTimeCode = true;
                }

                if ( node is TypeDeclarationSyntax typeWithMembers )
                {
                    foreach ( var childType in typeWithMembers.Members )
                    {
                        if ( childType is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax )
                        {
                            this.VisitTypeDeclaration( childType );
                        }
                    }
                }
            }

            public override void VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

            public override void VisitEnumDeclaration( EnumDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

            public override void VisitDelegateDeclaration( DelegateDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

            public override void VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

            public override void VisitRecordDeclaration( RecordDeclarationSyntax node ) => this.VisitTypeDeclaration( node );
        }
    }
}