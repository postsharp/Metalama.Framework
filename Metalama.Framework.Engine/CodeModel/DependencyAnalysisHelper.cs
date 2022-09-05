﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel
{
    public static class DependencyAnalysisHelper
    {
        public static IEnumerable<SyntaxNode> FindDeclaredTypes( this SyntaxTree syntaxTree )
        {
            var visitor = new FindTypesVisitor();
            visitor.Visit( syntaxTree.GetRoot() );

            return visitor.Types;
        }

        private class FindTypesVisitor : SafeSyntaxWalker
        {
            public List<SyntaxNode> Types { get; } = new();

            private void VisitType( SyntaxNode node )
            {
                this.Types.Add( node );

                // Also index nested types.
                if ( node is TypeDeclarationSyntax typeDeclaration )
                {
                    foreach ( var child in typeDeclaration.Members )
                    {
                        if ( child is BaseTypeDeclarationSyntax )
                        {
                            this.VisitType( child );
                        }
                    }
                }
            }

            public override void VisitInterfaceDeclaration( InterfaceDeclarationSyntax node ) => this.VisitType( node );

            public override void VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitType( node );

            public override void VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitType( node );

            public override void VisitRecordDeclaration( RecordDeclarationSyntax node ) => this.VisitType( node );

            public override void VisitEnumDeclaration( EnumDeclarationSyntax node ) => this.VisitType( node );

            public override void VisitDelegateDeclaration( DelegateDeclarationSyntax node ) => this.VisitType( node );
        }
    }
}