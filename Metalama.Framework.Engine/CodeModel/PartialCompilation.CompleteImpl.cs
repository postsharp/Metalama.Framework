// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel
{
    public abstract partial class PartialCompilation
    {
        /// <summary>
        /// Represents a complete compilation, containing all syntax trees.
        /// </summary>
        private class CompleteImpl : PartialCompilation
        {
            public CompleteImpl( Compilation compilation, ImmutableArray<ManagedResource> resources )
                : base( compilation, GetDerivedTypeIndex( compilation ), resources ) { }

            private CompleteImpl(
                PartialCompilation baseCompilation,
                IReadOnlyList<SyntaxTreeTransformation>? modifications,
                ImmutableArray<ManagedResource> resources )
                : base( baseCompilation, modifications, resources ) { }

            [Memo]
            public override ImmutableDictionary<string, SyntaxTree> SyntaxTrees
                => this.Compilation.SyntaxTrees.ToImmutableDictionary( s => s.FilePath, s => s );

            private static DerivedTypeIndex GetDerivedTypeIndex( Compilation compilation )
            {
                DerivedTypeIndex.Builder builder = new( compilation );

                foreach ( var type in compilation.Assembly.GetTypes() )
                {
                    builder.AnalyzeType( type );
                }

                return builder.ToImmutable();
            }

            [Memo]
            public override ImmutableHashSet<INamedTypeSymbol> Types => this.Compilation.Assembly.GetTypes().ToImmutableHashSet();

            [Memo]
            public override ImmutableHashSet<INamespaceSymbol> Namespaces
                => this.Compilation.Assembly.GlobalNamespace.SelectManyRecursive( n => n.GetNamespaceMembers() ).ToImmutableHashSet();

            public override bool IsPartial => false;

            public override PartialCompilation Update(
                IReadOnlyList<SyntaxTreeTransformation>? transformations = null,
                ImmutableArray<ManagedResource> resources = default )
            {
                this.Validate( transformations );

                return new CompleteImpl( this, transformations, resources );
            }
        }
    }
}