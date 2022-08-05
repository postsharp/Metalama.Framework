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
internal class PartialTypesVisitor : CSharpSyntaxVisitor<ImmutableArray<BaseTypeDeclarationSyntax>>
{
    public static PartialTypesVisitor Instance { get; } = new();
    
    private PartialTypesVisitor() { }

    public override ImmutableArray<BaseTypeDeclarationSyntax> DefaultVisit( SyntaxNode node )
    {
        var partialTypes = ImmutableArray<BaseTypeDeclarationSyntax>.Empty;

        foreach ( var child in node.ChildNodesAndTokens() )
        {
            if ( child.IsNode )
            {
                var newPartialTypes = this.Visit( child.AsNode() );

                if ( !newPartialTypes.IsDefaultOrEmpty )
                {
                    partialTypes = partialTypes.AddRange( newPartialTypes );
                }
            }
        }

        return partialTypes;
    }

    public override ImmutableArray<BaseTypeDeclarationSyntax> VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitBaseTypeDeclaration( node );

    public override ImmutableArray<BaseTypeDeclarationSyntax> VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitBaseTypeDeclaration( node );

    public override ImmutableArray<BaseTypeDeclarationSyntax> VisitRecordDeclaration( RecordDeclarationSyntax node ) => this.VisitBaseTypeDeclaration( node );

    public override ImmutableArray<BaseTypeDeclarationSyntax> VisitGlobalStatement( GlobalStatementSyntax node ) => ImmutableArray<BaseTypeDeclarationSyntax>.Empty;

    public override ImmutableArray<BaseTypeDeclarationSyntax> VisitUsingDirective( UsingDirectiveSyntax node ) => ImmutableArray<BaseTypeDeclarationSyntax>.Empty;

    public override ImmutableArray<BaseTypeDeclarationSyntax> VisitDelegateDeclaration( DelegateDeclarationSyntax node ) => ImmutableArray<BaseTypeDeclarationSyntax>.Empty;

    public override ImmutableArray<BaseTypeDeclarationSyntax> VisitEnumDeclaration( EnumDeclarationSyntax node ) => ImmutableArray<BaseTypeDeclarationSyntax>.Empty;

    public override ImmutableArray<BaseTypeDeclarationSyntax> VisitMethodDeclaration( MethodDeclarationSyntax node ) => ImmutableArray<BaseTypeDeclarationSyntax>.Empty;

    private ImmutableArray<BaseTypeDeclarationSyntax> VisitBaseTypeDeclaration( BaseTypeDeclarationSyntax type )
    {
        foreach ( var modifier in type.Modifiers )
        {
            if ( modifier.IsKind( SyntaxKind.PartialKeyword ) )
            {
                return ImmutableArray.Create( type );
            }
        }
        
        return ImmutableArray<BaseTypeDeclarationSyntax>.Empty;
    }
}