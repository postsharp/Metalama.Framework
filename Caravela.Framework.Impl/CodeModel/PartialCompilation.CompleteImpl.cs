﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Utilities;
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
            public CompleteImpl( Compilation compilation, ImmutableArray<ManagedResource> resources )
                : base( compilation, GetDerivedTypeIndex( compilation ), resources ) { }

            private CompleteImpl(
                PartialCompilation baseCompilation,
                IReadOnlyList<SyntaxTreeModification>? modifiedSyntaxTrees,
                IReadOnlyList<SyntaxTree>? addedTrees,
                ImmutableArray<ManagedResource>? resources )
                : base( baseCompilation, modifiedSyntaxTrees, addedTrees, resources ) { }

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
                IReadOnlyList<SyntaxTreeModification>? replacedTrees = null,
                IReadOnlyList<SyntaxTree>? addedTrees = null,
                ImmutableArray<ManagedResource>? resources = null )
                => new CompleteImpl( this, replacedTrees, addedTrees, resources );
        }
    }
}