// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CodeModel
{
    public abstract partial class PartialCompilation
    {
        /// <summary>
        /// Represents a partial compilation, containing a subset of syntax trees.
        /// </summary>
        private class PartialImpl : PartialCompilation
        {
            private readonly ImmutableArray<ITypeSymbol>? _types;
            private readonly ImmutableDictionary<string, SyntaxTree> _syntaxTrees;

            public PartialImpl(
                Compilation compilation,
                ImmutableDictionary<string, SyntaxTree> syntaxTrees,
                ImmutableArray<ITypeSymbol>? types,
                ImmutableArray<ResourceDescription> resources )
                : base( compilation, resources )
            {
                this._types = types;
                this._syntaxTrees = syntaxTrees;
            }

            public PartialImpl(
                ImmutableDictionary<string, SyntaxTree> syntaxTrees,
                ImmutableArray<ITypeSymbol>? types,
                PartialCompilation baseCompilation,
                IReadOnlyList<SyntaxTreeModification>? modifiedSyntaxTrees,
                IReadOnlyList<SyntaxTree>? addedTrees,
                ImmutableArray<ResourceDescription>? resources )
                : base( baseCompilation, modifiedSyntaxTrees, addedTrees, resources )
            {
                this._types = types;
                this._syntaxTrees = syntaxTrees;
            }

            public override ImmutableDictionary<string, SyntaxTree> SyntaxTrees => this._syntaxTrees;

            public override IEnumerable<ITypeSymbol> Types => this._types ?? throw new NotImplementedException();

            public override bool IsPartial => false;

            public override PartialCompilation Update(
                IReadOnlyList<SyntaxTreeModification>? replacedTrees = null,
                IReadOnlyList<SyntaxTree>? addedTrees = null,
                ImmutableArray<ResourceDescription>? resources = null )
            {
                var syntaxTrees = this._syntaxTrees.ToBuilder();

                if ( replacedTrees != null )
                {
                    foreach ( var replacement in replacedTrees )
                    {
                        if ( !this._syntaxTrees.ContainsKey( replacement.FilePath ) )
                        {
                            throw new KeyNotFoundException();
                        }

                        syntaxTrees[replacement.FilePath] = replacement.NewTree;
                    }
                }

                if ( addedTrees != null )
                {
                    foreach ( var addedTree in addedTrees )
                    {
                        syntaxTrees.Add( addedTree.FilePath, addedTree );
                    }
                }

                return new PartialImpl( syntaxTrees.ToImmutable(), null, this, replacedTrees, addedTrees, resources );
            }
        }
    }
}