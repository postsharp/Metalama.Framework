// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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
    public MemberDeclarationSyntax SyntaxNode { get; }

    public InsertPosition( InsertPositionRelation relation, MemberDeclarationSyntax node )
    {
        this.Relation = relation;
        this.SyntaxNode = node;
    }

    public override bool Equals( object? obj ) => obj is InsertPosition position && this.Equals( position );

    public bool Equals( InsertPosition other ) => this.Relation == other.Relation && this.SyntaxNode == other.SyntaxNode;

    public override int GetHashCode() => HashCode.Combine( this.Relation, this.SyntaxNode );

    public override string ToString() => $"{this.Relation} {this.SyntaxNode.Kind()} in {this.SyntaxNode.SyntaxTree.FilePath}";
}