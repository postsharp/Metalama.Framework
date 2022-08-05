// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Diff;

/// <summary>
/// Discovers partial types in a syntax tree. The code is optimized to allocate a minimum of memory, ideally
/// zero when there is no partial type.
/// </summary>
internal class PartialTypesHasher : CSharpSyntaxVisitor<int?>
{
    public static PartialTypesHasher Instance { get; } = new();
    
    private PartialTypesHasher() { }

    public override int? DefaultVisit( SyntaxNode node )
    {
        var combinedHash = 0;
        var hasAnyPartialType = false;

        foreach ( var child in node.ChildNodesAndTokens() )
        {
            if ( child.IsNode )
            {
                var childHash = this.Visit( child.AsNode() );

                if ( childHash.HasValue )
                {
                    combinedHash = HashCode.Combine( combinedHash, childHash.Value );
                    hasAnyPartialType = true;
                }
            }
        }

        return hasAnyPartialType ? combinedHash : null;
    }

    public override int? VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitBaseTypeDeclaration( node );

    public override int? VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitBaseTypeDeclaration( node );

    public override int? VisitRecordDeclaration( RecordDeclarationSyntax node ) => this.VisitBaseTypeDeclaration( node );

    public override int? VisitGlobalStatement( GlobalStatementSyntax node ) => null;

    public override int? VisitUsingDirective( UsingDirectiveSyntax node ) => null;

    public override int? VisitDelegateDeclaration( DelegateDeclarationSyntax node ) => null;

    public override int? VisitEnumDeclaration( EnumDeclarationSyntax node ) => null;

    public override int? VisitMethodDeclaration( MethodDeclarationSyntax node ) => null;

    private int? VisitBaseTypeDeclaration( BaseTypeDeclarationSyntax type )
    {
        foreach ( var modifier in type.Modifiers )
        {
            if ( modifier.IsKind( SyntaxKind.PartialKeyword ) )
            {
                return type.Identifier.GetHashCode();
            }
        }
        
        return null;
    }
}