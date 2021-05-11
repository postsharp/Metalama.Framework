// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CodeModel
{
    /// <summary>
    /// Represents a subset of a Roslyn <see cref="Microsoft.CodeAnalysis.Compilation"/>. The subset is limited
    /// to specific syntax trees.
    /// </summary>
    public abstract partial class PartialCompilation : IPartialCompilation
    {
        /// <summary>
        /// Gets the Roslyn <see cref="Microsoft.CodeAnalysis.Compilation"/>.
        /// </summary>
        public Compilation Compilation { get; }

        /// <summary>
        /// Gets the list of syntax trees in the current subset.
        /// </summary>
        public abstract IReadOnlyCollection<SyntaxTree> SyntaxTrees { get; }

        /// <summary>
        /// Gets the types declared in the current subset.
        /// </summary>
        public abstract IEnumerable<ITypeSymbol> Types { get; }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="PartialCompilation"/> is actually partial, or represents a complete compilation.
        /// </summary>
        public abstract bool IsPartial { get; }

        public bool IsEmpty => this.SyntaxTrees.Count == 0;

        private PartialCompilation( Compilation compilation )
        {
            this.Compilation = compilation;
        }

        /// <summary>
        /// Creates a <see cref="PartialCompilation"/> that represents a complete compilation.
        /// </summary>
        public static PartialCompilation CreateComplete( Compilation compilation ) => new CompleteImpl( compilation );

        /// <summary>
        /// Creates a <see cref="PartialCompilation"/> for a given subset of syntax trees and its closure.
        /// </summary>
        public static PartialCompilation CreatePartial( Compilation compilation, IEnumerable<SyntaxTree> syntaxTrees )
            => new PartialImpl( compilation, syntaxTrees.ToImmutableHashSet() );

        /// <summary>
        /// Creates a <see cref="PartialCompilation"/> for a single syntax tree and its closure.
        /// </summary>
        public static PartialCompilation CreatePartial( Compilation compilation, SyntaxTree syntaxTree )
        {
            var syntaxTrees = new[] { syntaxTree };
            var closure = GetClosure( compilation, syntaxTrees );

            return new PartialImpl( compilation, closure.Trees.ToImmutableHashSet(), closure.Types.ToImmutableArray() );
        }

        /// <summary>
        /// Creates a <see cref="PartialCompilation"/> for a given subset of syntax trees and its closure.
        /// </summary>
        public static PartialCompilation CreatePartial( Compilation compilation, IReadOnlyList<SyntaxTree> syntaxTrees )
        {
            if ( syntaxTrees.Count == 0 )
            {
                throw new ArgumentOutOfRangeException();
            }

            var closure = GetClosure( compilation, syntaxTrees );

            return new PartialImpl( compilation, closure.Trees.ToImmutableHashSet(), closure.Types.ToImmutableArray() );
        }

        IPartialCompilation IPartialCompilation.UpdateSyntaxTrees(
            IReadOnlyList<(SyntaxTree OldTree, SyntaxTree NewTree)> replacedTrees,
            IReadOnlyList<SyntaxTree> addedTrees )
            => this.UpdateSyntaxTrees( replacedTrees, addedTrees );

        /// <summary>
        ///  Adds and replaces syntax trees of the current <see cref="PartialCompilation"/> and returns a new <see cref="PartialCompilation"/>
        /// representing the modified object.
        /// </summary>
        public abstract PartialCompilation UpdateSyntaxTrees(
            IReadOnlyList<(SyntaxTree OldTree, SyntaxTree NewTree)> replacedTrees,
            IReadOnlyList<SyntaxTree> addedTrees );

        /// <summary>
        /// Gets a closure of the syntax trees declaring all base types and interfaces of all types declared in input syntax trees.
        /// </summary>
        private static (HashSet<ITypeSymbol> Types, HashSet<SyntaxTree> Trees) GetClosure( Compilation compilation, IReadOnlyList<SyntaxTree> syntaxTrees )
        {
            var assembly = compilation.Assembly;

            HashSet<ITypeSymbol> types = new();
            HashSet<SyntaxTree> trees = new();

            void AddTypeRecursive( ITypeSymbol type )
            {
                if ( !SymbolEqualityComparer.Default.Equals( type.ContainingAssembly, assembly ) )
                {
                    // The type is defined in a different assembly.
                    return;
                }

                if ( types.Add( type ) )
                {
                    // Find relevant syntax trees
                    foreach ( var syntaxReference in type.DeclaringSyntaxReferences )
                    {
                        trees.Add( syntaxReference.SyntaxTree );
                    }

                    // Add base types recursively.
                    if ( type.BaseType != null && SymbolEqualityComparer.Default.Equals( type.ContainingAssembly, type.BaseType.ContainingAssembly ) )
                    {
                        AddTypeRecursive( type.BaseType );
                    }

                    foreach ( var interfaceImpl in type.Interfaces )
                    {
                        AddTypeRecursive( interfaceImpl );
                    }
                }
                else
                {
                    // The type was already processed.
                }
            }

            foreach ( var syntaxTree in syntaxTrees )
            {
                var semanticModel = compilation.GetSemanticModel( syntaxTree );

                foreach ( var typeNode in syntaxTree.FindDeclaredTypes() )
                {
                    var type = (INamedTypeSymbol?) semanticModel.GetDeclaredSymbol( typeNode );

                    if ( type == null )
                    {
                        continue;
                    }

                    AddTypeRecursive( type );
                }
            }

            return (types, trees);
        }
    }
}