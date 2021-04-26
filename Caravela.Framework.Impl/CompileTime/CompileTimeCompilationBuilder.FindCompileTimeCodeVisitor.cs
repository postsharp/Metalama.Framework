// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.CompileTime
{
    internal partial class CompileTimeCompilationBuilder
    {
        private class FindCompileTimeCodeVisitor : CSharpSyntaxWalker
        {
            private readonly SemanticModel _semanticModel;
            private readonly ISymbolClassifier _classifier;

            public bool HasCompileTimeCode { get; private set; }

            public FindCompileTimeCodeVisitor( SemanticModel semanticModel, ISymbolClassifier classifier )
            {
                this._semanticModel = semanticModel;
                this._classifier = classifier;
            }

            private void VisitTypeDeclaration( SyntaxNode node )
            {
                if ( this.HasCompileTimeCode )
                {
                    // No need to do anything more.
                    return;
                }

                var declaredSymbol = this._semanticModel.GetDeclaredSymbol( node );

                if ( declaredSymbol != null && this._classifier.GetSymbolDeclarationScope( declaredSymbol ) != SymbolDeclarationScope.RunTimeOnly )
                {
                    this.HasCompileTimeCode = true;
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