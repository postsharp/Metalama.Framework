// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CodeModel
{
    public abstract partial class PartialCompilation
    {
        /// <summary>
        /// Represents a complete compilation, containing all syntax trees.
        /// </summary>
        private class CompleteImpl : PartialCompilation
        {
            public CompleteImpl( Compilation compilation, PartialCompilation? baseCompilation, IReadOnlyList<ModifiedSyntaxTree>? modifiedSyntaxTrees )
                : base( compilation, baseCompilation, modifiedSyntaxTrees ) { }

            [Memo]
            public override ImmutableDictionary<string, SyntaxTree> SyntaxTrees
                => this.Compilation.SyntaxTrees.ToImmutableDictionary( s => s.FilePath, s => s );

            public override IEnumerable<ITypeSymbol> Types => this.Compilation.Assembly.GetTypes();

            public override bool IsPartial => false;

            public override PartialCompilation UpdateSyntaxTrees(
                IReadOnlyList<ModifiedSyntaxTree>? replacedTrees = null,
                IReadOnlyList<SyntaxTree>? addedTrees = null )
            {
                var compilation = this.Compilation;

                if ( replacedTrees != null )
                {
                    foreach ( var replacedTree in replacedTrees )
                    {
                        compilation = compilation.ReplaceSyntaxTree( replacedTree.OldTree, replacedTree.NewTree );
                    }
                }

                if ( addedTrees != null )
                {
                    compilation = compilation.AddSyntaxTrees( addedTrees );
                }

                return new CompleteImpl( compilation, this, replacedTrees );
            }
        }
    }
}