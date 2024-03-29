// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel.Builders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Transformations;

internal readonly struct InsertPosition : IEquatable<InsertPosition>
{
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
    public NamedTypeBuilder? TypeBuilder { get; }

    /// <summary>
    /// Gets the target syntax tree of the insertion.
    /// </summary>
    public SyntaxTree SyntaxTree => this.SyntaxNode?.SyntaxTree ?? this.TypeBuilder.AssertNotNull().PrimarySyntaxTree.AssertNotNull();

    public InsertPosition( InsertPositionRelation relation, MemberDeclarationSyntax node )
    {
        this.Relation = relation;
        this.SyntaxNode = node;
    }

    public InsertPosition( InsertPositionRelation relation, NamedTypeBuilder builder )
    {
        this.Relation = relation;
        this.TypeBuilder = builder;
    }

    public override bool Equals( object? obj ) => obj is InsertPosition position && this.Equals( position );

    public bool Equals( InsertPosition other )
        => this.Relation == other.Relation && this.SyntaxNode == other.SyntaxNode && this.TypeBuilder == other.TypeBuilder;

    public override int GetHashCode() => HashCode.Combine( this.Relation, this.SyntaxNode, this.TypeBuilder );

    public override string ToString()
        => this.SyntaxNode != null
            ? $"{this.Relation} {this.SyntaxNode.Kind()} in {this.SyntaxNode.SyntaxTree.FilePath}"
            : $"{this.Relation} {this.TypeBuilder.AssertNotNull().FullName} (built type)";
}