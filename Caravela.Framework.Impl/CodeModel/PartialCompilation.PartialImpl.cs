// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Sdk;
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
                PartialCompilation? baseCompilation,
                IReadOnlyList<ModifiedSyntaxTree>? modifiedSyntaxTrees )
                : base( compilation, baseCompilation, modifiedSyntaxTrees )
            {
                this._types = types;
                this._syntaxTrees = syntaxTrees;
            }

            public override ImmutableDictionary<string, SyntaxTree> SyntaxTrees => this._syntaxTrees;

            public override IEnumerable<ITypeSymbol> Types => this._types ?? throw new NotImplementedException();

            public override bool IsPartial => false;

            public override PartialCompilation UpdateSyntaxTrees(
                IReadOnlyList<ModifiedSyntaxTree>? replacedTrees = null,
                IReadOnlyList<SyntaxTree>? addedTrees = null )
            {
                var compilation = this.Compilation;
                var syntaxTrees = this._syntaxTrees;

                if ( replacedTrees != null )
                {
                    foreach ( var replacement in replacedTrees )
                    {
                        if ( !this._syntaxTrees.ContainsKey( replacement.FilePath ) )
                        {
                            throw new KeyNotFoundException();
                        }

                        compilation = compilation.ReplaceSyntaxTree( replacement.OldTree, replacement.NewTree );
                        syntaxTrees = syntaxTrees.SetItem( replacement.FilePath, replacement.NewTree );
                    }
                }

                if ( addedTrees != null )
                {
                    compilation = compilation.AddSyntaxTrees( addedTrees );

                    foreach ( var addedTree in addedTrees )
                    {
                        syntaxTrees = syntaxTrees.Add( addedTree.FilePath, addedTree );
                    }
                }

                return new PartialImpl( compilation, syntaxTrees, null, this, replacedTrees );
            }
        }
    }
}