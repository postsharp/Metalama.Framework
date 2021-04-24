// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Sdk
{

    public abstract partial class PartialCompilation
    {
        private class PartialImpl : PartialCompilation
        {
            private readonly IReadOnlyList<ITypeSymbol>? _types;
            private readonly ImmutableHashSet<SyntaxTree> _syntaxTrees;

            public PartialImpl( Compilation compilation, ImmutableHashSet<SyntaxTree> syntaxTrees, IReadOnlyList<ITypeSymbol>? types = null ) : base(
                compilation )
            {
                this._types = types;
                this._syntaxTrees = syntaxTrees;
            }

            public override IEnumerable<SyntaxTree> SyntaxTrees => this._syntaxTrees;

            public override IEnumerable<ITypeSymbol> Types => this._types ?? throw new NotImplementedException();

            public override bool IsPartial => false;

            public override PartialCompilation ReplaceSyntaxTree( SyntaxTree oldTree, SyntaxTree newTree )
            {
                if ( !this._syntaxTrees.Contains( oldTree ) )
                {
                    throw new KeyNotFoundException();
                }

                return new PartialImpl( this.Compilation.ReplaceSyntaxTree( oldTree, newTree ), this._syntaxTrees.Remove( oldTree ).Add( newTree ) );
            }
        }
    }
}