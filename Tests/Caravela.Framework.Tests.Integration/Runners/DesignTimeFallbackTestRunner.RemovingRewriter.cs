// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.TestFramework.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Tests.Integration.Runners
{
    internal partial class DesignTimeFallbackTestRunner
    {
        private class RemovingRewriter : CSharpSyntaxRewriter
        {
            public static readonly RemovingRewriter Instance = new();

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                if ( HasRemoveTrivia( node ) )
                {
                    return null;
                }

                return base.VisitClassDeclaration( node );
            }

            public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node )
            {
                if ( HasRemoveTrivia( node ) )
                {
                    return null;
                }

                return base.VisitStructDeclaration( node );
            }

            public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node )
            {
                if ( HasRemoveTrivia( node ) )
                {
                    return null;
                }

                return base.VisitRecordDeclaration( node );
            }

            public override SyntaxNode? VisitEnumDeclaration( EnumDeclarationSyntax node )
            {
                if ( HasRemoveTrivia( node ) )
                {
                    return null;
                }

                return base.VisitEnumDeclaration( node );
            }

            public override SyntaxNode? VisitInterfaceDeclaration( InterfaceDeclarationSyntax node )
            {
                if ( HasRemoveTrivia( node ) )
                {
                    return null;
                }

                return base.VisitInterfaceDeclaration( node );
            }

            private static bool HasRemoveTrivia( SyntaxNode typeDeclaration )
            {
                if ( typeDeclaration.HasLeadingTrivia )
                {
                    var leadingTrivia = typeDeclaration.GetLeadingTrivia();

                    if ( leadingTrivia.ToString().ContainsOrdinal( "<remove>" ) )
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}