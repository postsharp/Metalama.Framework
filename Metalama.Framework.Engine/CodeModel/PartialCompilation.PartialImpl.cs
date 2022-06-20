// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
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
                DerivedTypeIndex derivedTypeIndex,
                ImmutableArray<ManagedResource> resources )
                : base( compilation, derivedTypeIndex, resources )
            {
                this._types = types;
                this._syntaxTrees = syntaxTrees;

                this.CheckTrees();
            }

            private PartialImpl(
                ImmutableDictionary<string, SyntaxTree> syntaxTrees,
                ImmutableHashSet<INamedTypeSymbol>? types,
                PartialCompilation baseCompilation,
                IReadOnlyList<SyntaxTreeModification>? modifiedSyntaxTrees,
                IReadOnlyList<SyntaxTree>? addedTrees,
                IReadOnlyList<SyntaxTree>? removedTrees,
                ImmutableArray<ManagedResource> resources )
                : base( baseCompilation, modifiedSyntaxTrees, addedTrees, removedTrees, resources )
            {
                this._types = types;
                this._syntaxTrees = syntaxTrees;

                this.CheckTrees();
            }

            private void CheckTrees()
            {
#if DEBUG
                if ( this._syntaxTrees.Any( t => string.IsNullOrEmpty( t.Key ) ) )
                {
                    throw new AssertionFailedException( "A syntax tree has no name." );
                }
#endif
            }

            public override ImmutableDictionary<string, SyntaxTree> SyntaxTrees => this._syntaxTrees;

            public override ImmutableHashSet<INamedTypeSymbol> Types => this._types ?? throw new NotImplementedException();

            public override ImmutableHashSet<INamespaceSymbol> Namespaces => this.Types.Select( t => t.ContainingNamespace ).ToImmutableHashSet();

            public override bool IsPartial => true;

            public override PartialCompilation Update(
                IReadOnlyList<SyntaxTreeModification>? replacedTrees = null,
                IReadOnlyList<SyntaxTree>? addedTrees = null,
                IReadOnlyList<SyntaxTree>? removedTrees = null,
                ImmutableArray<ManagedResource> resources = default )
            {
                this.Validate( addedTrees, replacedTrees );

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

                if ( removedTrees != null )
                {
                    foreach ( var removedTree in removedTrees )
                    {
                        syntaxTrees.Remove( removedTree.FilePath );
                    }
                }

                // TODO: when the compilation is modified, we should update the set of types and derived types.
                return new PartialImpl( syntaxTrees.ToImmutable(), null, this, replacedTrees, addedTrees, removedTrees, resources );
            }
        }
    }
}