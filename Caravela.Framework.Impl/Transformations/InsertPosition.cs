// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Transformations
{
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

        public InsertPosition( InsertPositionRelation relation, MemberDeclarationSyntax node )
        {
            this.Relation = relation;
            this.SyntaxNode = node;
        }

        public override bool Equals( object? obj )
        {
            return obj is InsertPosition position && this.Equals( position );
        }

        public bool Equals( InsertPosition other )
        {
            return
                this.Relation == other.Relation &&
                this.SyntaxNode == other.SyntaxNode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine( this.Relation, this.SyntaxNode );
        }
    }
}