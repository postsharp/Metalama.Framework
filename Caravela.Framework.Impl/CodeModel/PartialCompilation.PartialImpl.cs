// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract partial class PartialCompilation
    {
        /// <summary>
        /// Represents a partial compilation, containing a subset of syntax trees.
        /// </summary>
        private class PartialImpl : PartialCompilation
        {
            private readonly ImmutableArray<ITypeSymbol>? _types;
            private readonly ImmutableHashSet<SyntaxTree> _syntaxTrees;

            public PartialImpl(
                Compilation compilation,
                ImmutableHashSet<SyntaxTree> syntaxTrees,
                ImmutableArray<ITypeSymbol>? types = null )
                : base( compilation )
            {
                this._types = types;
                this._syntaxTrees = syntaxTrees;
            }

            public override IReadOnlyCollection<SyntaxTree> SyntaxTrees => this._syntaxTrees;

            public override IEnumerable<ITypeSymbol> Types => this._types ?? throw new NotImplementedException();

            public override bool IsPartial => false;

            public override PartialCompilation UpdateSyntaxTrees(
                IReadOnlyList<(SyntaxTree OldTree, SyntaxTree NewTree)> replacedTrees,
                IReadOnlyList<SyntaxTree> addedTrees )
            {
                var compilation = this.Compilation;
                var syntaxTrees = this._syntaxTrees;

                foreach ( var replacement in replacedTrees )
                {
                    if ( !this._syntaxTrees.Contains( replacement.OldTree ) )
                    {
                        throw new KeyNotFoundException();
                    }

                    compilation = compilation.ReplaceSyntaxTree( replacement.OldTree, replacement.NewTree );
                    syntaxTrees = syntaxTrees.Remove( replacement.OldTree ).Add( replacement.NewTree );
                }

                compilation = compilation.AddSyntaxTrees( addedTrees );

                foreach ( var addedTree in addedTrees )
                {
                    syntaxTrees = syntaxTrees.Add( addedTree );
                }

                return new PartialImpl( compilation, syntaxTrees );
            }
        }
    }
}