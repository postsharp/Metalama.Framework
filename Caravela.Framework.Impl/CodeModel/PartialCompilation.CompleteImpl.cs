// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Sdk
{
    public abstract partial class PartialCompilation
    {
        private class CompleteImpl : PartialCompilation
        {
            public CompleteImpl( Compilation compilation ) : base( compilation ) { }

            [Memo]
            public override IReadOnlyCollection<SyntaxTree> SyntaxTrees => this.Compilation.SyntaxTrees.ToImmutableArray();

            public override IEnumerable<ITypeSymbol> Types => this.Compilation.Assembly.GetTypes();

            public override bool IsPartial { get; }

            public override PartialCompilation Update( IEnumerable<(SyntaxTree OldTree, SyntaxTree NewTree)> replacedTrees, IEnumerable<SyntaxTree> addedTrees )
            {
                var compilation = this.Compilation;

                foreach ( var replacedTree in replacedTrees )
                {
                    compilation = compilation.ReplaceSyntaxTree( replacedTree.OldTree, replacedTree.NewTree );
                }

                compilation = compilation.AddSyntaxTrees( addedTrees );

                return new CompleteImpl( compilation );
            }
        }
    }
}