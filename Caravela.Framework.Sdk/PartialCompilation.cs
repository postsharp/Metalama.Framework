// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Sdk
{

    public abstract partial class PartialCompilation
    {
        public Compilation Compilation { get; }

        public abstract IEnumerable<SyntaxTree> SyntaxTrees { get; }

        public abstract IEnumerable<ITypeSymbol> Types { get; }

        private PartialCompilation( Compilation compilation )
        {
            this.Compilation = compilation;
        }

        public abstract bool IsPartial { get; }

        public static PartialCompilation CreateComplete( Compilation compilation ) => new CompleteImpl( compilation );

        public static PartialCompilation CreatePartial( Compilation compilation, IEnumerable<SyntaxTree> syntaxTrees )
            => new PartialImpl( compilation, syntaxTrees.ToImmutableHashSet() );

        public abstract PartialCompilation ReplaceSyntaxTree( SyntaxTree oldTree, SyntaxTree newTree );

        public static PartialCompilation CreatePartial( SemanticModel semanticModel )
        {
            var closure = GetClosure( semanticModel );

            return new PartialImpl( semanticModel.Compilation, closure.Trees.ToImmutableHashSet(), closure.Types.ToImmutableArray() );
        }

        private static (HashSet<ITypeSymbol> Types, HashSet<SyntaxTree> Trees ) GetClosure( SemanticModel semanticModel )
        {
            FindTypesVisitor findTypesVisitor = new();
            findTypesVisitor.Visit( semanticModel.SyntaxTree.GetRoot() );

            HashSet<ITypeSymbol> types = new();
            HashSet<SyntaxTree> trees = new();

            void AddTypeRecursive( ITypeSymbol type )
            {
                if (!SymbolEqualityComparer.Default.Equals( type.ContainingAssembly, semanticModel.Compilation.Assembly))
                {
                    // The type is defined in a different assembly.
                    return;
                }

                if (types.Add( type ))
                {
                    // Find relevant syntax trees
                    foreach (var syntaxReference in type.DeclaringSyntaxReferences)
                    {
                        trees.Add( syntaxReference.SyntaxTree );
                    }

                    // Add base types recursively.
                    if (type.BaseType != null && SymbolEqualityComparer.Default.Equals( type.ContainingAssembly, type.BaseType.ContainingAssembly))
                    {
                        AddTypeRecursive( type.BaseType );
                    }

                    foreach (var interfaceImpl in type.Interfaces)
                    {
                        AddTypeRecursive( interfaceImpl );
                    }
                }
                else
                {
                    // The type was already processed.
                }
            }

            foreach (var typeNode in findTypesVisitor.Types)
            {
                var type = (INamedTypeSymbol?) semanticModel.GetDeclaredSymbol( typeNode );

                if (type == null)
                {
                    continue;
                }

                AddTypeRecursive( type );
            }

            return (types, trees);
        }
    }
}