// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    public abstract partial class PartialCompilation
    {
        /// <summary>
        /// Represents a partial compilation, containing a subset of syntax trees.
        /// </summary>
        private class PartialImpl : PartialCompilation
        {
            private readonly ImmutableHashSet<INamedTypeSymbol>? _types;
            private readonly ImmutableDictionary<string, SyntaxTree> _syntaxTrees;

            public PartialImpl(
                Compilation compilation,
                ImmutableDictionary<string, SyntaxTree> syntaxTrees,
                ImmutableHashSet<INamedTypeSymbol>? types,
                ImmutableArray<ResourceDescription> resources )
                : base( compilation, resources )
            {
                this._types = types;
                this._syntaxTrees = syntaxTrees;
            }

            private PartialImpl(
                ImmutableDictionary<string, SyntaxTree> syntaxTrees,
                ImmutableHashSet<INamedTypeSymbol>? types,
                PartialCompilation baseCompilation,
                IReadOnlyList<ModifiedSyntaxTree>? modifiedSyntaxTrees,
                IReadOnlyList<SyntaxTree>? addedTrees,
                ImmutableArray<ResourceDescription>? resources )
                : base( baseCompilation, modifiedSyntaxTrees, addedTrees, resources )
            {
                this._types = types;
                this._syntaxTrees = syntaxTrees;
            }

            public override ImmutableDictionary<string, SyntaxTree> SyntaxTrees => this._syntaxTrees;

            public override ImmutableHashSet<INamedTypeSymbol> Types => this._types ?? throw new NotImplementedException();

            public override ImmutableHashSet<INamespaceSymbol> Namespaces => this.Types.Select( t => t.ContainingNamespace ).ToImmutableHashSet();

            public override bool IsPartial => true;

            public override PartialCompilation Update(
                IReadOnlyList<ModifiedSyntaxTree>? replacedTrees = null,
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