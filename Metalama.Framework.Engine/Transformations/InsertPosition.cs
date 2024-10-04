// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel.Introductions.Builders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Transformations;

internal readonly struct InsertPosition : IEquatable<InsertPosition>
{
    private readonly SyntaxTree? _syntaxTree;

    /// <summary>
    /// Gets the relation of the insert position to the specified node.
    /// </summary>
    public InsertPositionRelation Relation { get; }

    /// <summary>
    /// Gets the node near to which/into which new nodes should be inserted.
    /// </summary>
    public MemberDeclarationSyntax? SyntaxNode { get; }

    /// <summary>
    /// Gets the builder into which the new node should be inserted.
    /// </summary>
    public NamedDeclarationBuilder? DeclarationBuilder { get; }

    /// <summary>
    /// Gets the target syntax tree of the insertion.
    /// </summary>
    public SyntaxTree SyntaxTree => this._syntaxTree.AssertNotNull();

    public InsertPosition( InsertPositionRelation relation, MemberDeclarationSyntax? node )
    {
        this.Relation = relation;
        this.SyntaxNode = node;
        this._syntaxTree = node?.SyntaxTree;
    }

    public InsertPosition( InsertPositionRelation relation, NamedTypeBuilder builder )
    {
        this.Relation = relation;
        this.DeclarationBuilder = builder;
        this._syntaxTree = builder.PrimarySyntaxTree.AssertNotNull();
    }
    
    public InsertPosition( SyntaxTree introducedSyntaxTree )
    {
        this.Relation = InsertPositionRelation.Root;
        this._syntaxTree = introducedSyntaxTree;
    }

    public override bool Equals( object? obj ) => obj is InsertPosition position && this.Equals( position );

    public bool Equals( InsertPosition other )
        => this.Relation == other.Relation
           && this.SyntaxNode == other.SyntaxNode
           && this.DeclarationBuilder == other.DeclarationBuilder
           && this.SyntaxTree == other.SyntaxTree;

    public override int GetHashCode() => HashCode.Combine( this.Relation, this.SyntaxNode, this.DeclarationBuilder, this.SyntaxTree );

    public override string ToString()
        => this.SyntaxNode != null
            ? $"{this.Relation} {this.SyntaxNode.Kind()} in {this.SyntaxNode.SyntaxTree.FilePath}"
            : this.DeclarationBuilder switch
            {
                NamedTypeBuilder namedTypeBuilder => $"{this.Relation} {namedTypeBuilder.AssertNotNull().FullName} (built type)",
                NamespaceBuilder namespaceBuilder => $"{this.Relation} {namespaceBuilder.AssertNotNull().FullName} (built namespace)",
                _ => throw new AssertionFailedException( $"Unexpected: {this.DeclarationBuilder}" ),
            };
}