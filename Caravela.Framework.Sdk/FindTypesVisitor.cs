// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Caravela.Framework.Sdk
{

    public abstract partial class PartialCompilation
    {
        private class FindTypesVisitor : CSharpSyntaxWalker
        {
            public List<SyntaxNode> Types { get; } = new();

            private void VisitType( SyntaxNode node ) => this.Types.Add( node );

            public override void VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitType( node );

            public override void VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitType( node );

            public override void VisitRecordDeclaration( RecordDeclarationSyntax node ) => this.VisitType( node );

            public override void VisitEnumDeclaration( EnumDeclarationSyntax node ) => this.VisitType( node );

            public override void VisitDelegateDeclaration( DelegateDeclarationSyntax node ) => this.VisitType( node );
        }
    }
}