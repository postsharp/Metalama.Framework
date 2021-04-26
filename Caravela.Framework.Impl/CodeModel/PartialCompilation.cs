// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Sdk
{
    public abstract partial class PartialCompilation : IPartialCompilation
    {
        public Compilation Compilation { get; }

        public abstract IReadOnlyCollection<SyntaxTree> SyntaxTrees { get; }

        public abstract IEnumerable<ITypeSymbol> Types { get; }

        public abstract bool IsPartial { get; }

        private PartialCompilation( Compilation compilation )
        {
            this.Compilation = compilation;
        }

        public static PartialCompilation CreateComplete( Compilation compilation ) => new CompleteImpl( compilation );

        public static PartialCompilation CreatePartial( Compilation compilation, IEnumerable<SyntaxTree> syntaxTrees )
            => new PartialImpl( compilation, syntaxTrees.ToImmutableHashSet() );

        public abstract PartialCompilation Update( IEnumerable<(SyntaxTree OldTree, SyntaxTree NewTree)> replacedTrees, IEnumerable<SyntaxTree> addedTrees );

        IPartialCompilation IPartialCompilation.UpdateSyntaxTrees(
            IEnumerable<(SyntaxTree OldTree, SyntaxTree NewTree)> replacedTrees,
            IEnumerable<SyntaxTree> addedTrees )
            => this.Update( replacedTrees, addedTrees );

        public static PartialCompilation CreatePartial( Compilation compilation, SyntaxTree syntaxTree )
        {
            var syntaxTrees = new[] { syntaxTree };
            var closure = GetClosure( compilation, syntaxTrees );

            return new PartialImpl( compilation, closure.Trees.ToImmutableHashSet(), closure.Types.ToImmutableArray() );
        }

        public static PartialCompilation CreatePartial( Compilation compilation, IReadOnlyList<SyntaxTree> syntaxTrees )
        {
            if ( syntaxTrees.Count == 0 )
            {
                throw new ArgumentOutOfRangeException();
            }

            var closure = GetClosure( compilation, syntaxTrees );

            return new PartialImpl( compilation, closure.Trees.ToImmutableHashSet(), closure.Types.ToImmutableArray() );
        }

        private static (HashSet<ITypeSymbol> Types, HashSet<SyntaxTree> Trees ) GetClosure( Compilation compilation, IReadOnlyList<SyntaxTree> syntaxTrees )
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