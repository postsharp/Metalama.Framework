// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract partial class PartialCompilation
    {
        /// <summary>
        /// Represents a complete compilation, containing all syntax trees.
        /// </summary>
        private class CompleteImpl : PartialCompilation
        {
            public CompleteImpl( Compilation compilation ) : base( compilation ) { }

            [Memo]
            public override IReadOnlyCollection<SyntaxTree> SyntaxTrees => this.Compilation.SyntaxTrees.ToImmutableArray();

            public override IEnumerable<ITypeSymbol> Types => this.Compilation.Assembly.GetTypes();

            public override bool IsPartial => false;

            public override PartialCompilation UpdateSyntaxTrees(
                IReadOnlyList<(SyntaxTree OldTree, SyntaxTree NewTree)> replacedTrees,
                IReadOnlyList<SyntaxTree> addedTrees )
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