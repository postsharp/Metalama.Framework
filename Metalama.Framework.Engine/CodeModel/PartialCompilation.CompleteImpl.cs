﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Utilities;
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
                IReadOnlyList<SyntaxTreeModification>? modifiedSyntaxTrees,
                IReadOnlyList<SyntaxTree>? addedTrees,
                IReadOnlyList<SyntaxTree>? removedTrees,
                ImmutableArray<ManagedResource> resources )
                : base( baseCompilation, modifiedSyntaxTrees, addedTrees, removedTrees, resources ) { }

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
                IReadOnlyList<SyntaxTree>? removedTrees = null,
                ImmutableArray<ManagedResource> resources = default )
            {
                this.Validate( addedTrees, replacedTrees );

                return new CompleteImpl( this, replacedTrees, addedTrees, removedTrees, resources );
            }
        }
    }
}