// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code.Builders;
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

        /// <summary>
        /// Gets the builder near to which/into which new nodes should be inserted.
        /// </summary>
        public IDeclarationBuilder? Builder { get; }

        public InsertPosition( InsertPositionRelation relation, MemberDeclarationSyntax node )
        {
            this.Relation = relation;
            this.SyntaxNode = node;
            this.Builder = null;
        }

        public InsertPosition( InsertPositionRelation relation, IDeclarationBuilder builder )
        {
            this.Relation = relation;
            this.SyntaxNode = null;
            this.Builder = builder;
        }

        public override bool Equals( object? obj )
        {
            return obj is InsertPosition position && this.Equals( position );
        }

        public bool Equals( InsertPosition other )
        {
            return
                this.Relation == other.Relation &&
                this.SyntaxNode == other.SyntaxNode &&
                this.Builder == other.Builder;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine( this.Relation, this.SyntaxNode, this.Builder );
        }
    }
}