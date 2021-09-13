// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
            public CompleteImpl( Compilation compilation, ImmutableArray<ResourceDescription> resources )
                : base( compilation, resources ) { }

            public CompleteImpl(
                PartialCompilation baseCompilation,
                IReadOnlyList<ModifiedSyntaxTree>? modifiedSyntaxTrees,
                IReadOnlyList<SyntaxTree>? addedTrees,
                ImmutableArray<ResourceDescription>? resources )
                : base( baseCompilation, modifiedSyntaxTrees, addedTrees, resources ) { }

            [Memo]
            public override ImmutableDictionary<string, SyntaxTree> SyntaxTrees
                => this.Compilation.SyntaxTrees.ToImmutableDictionary( s => s.FilePath, s => s );

            public override IEnumerable<ITypeSymbol> Types => this.Compilation.Assembly.GetTypes();

            public override bool IsPartial => false;

            public override PartialCompilation Update(
                IReadOnlyList<ModifiedSyntaxTree>? replacedTrees = null,
                IReadOnlyList<SyntaxTree>? addedTrees = null,
                ImmutableArray<ResourceDescription>? resources = null )
                => new CompleteImpl( this, replacedTrees, addedTrees, resources );
        }
    }
}