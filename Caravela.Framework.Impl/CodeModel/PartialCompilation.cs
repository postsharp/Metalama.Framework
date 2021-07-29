// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    /// <summary>
    /// Represents a subset of a Roslyn <see cref="Microsoft.CodeAnalysis.Compilation"/>. The subset is limited
    /// to specific syntax trees.
    /// </summary>
    public abstract partial class PartialCompilation : IPartialCompilation
    {
        /// <summary>
        /// Gets the compilation with respect to which the <see cref="ModifiedSyntaxTrees"/> has been constructed.
        /// Typically, this is the argument of the <see cref="CreateComplete"/> or <see cref="CreatePartial(Microsoft.CodeAnalysis.Compilation,Microsoft.CodeAnalysis.SyntaxTree)"/>
        /// method, ignoring any modification done by <see cref="UpdateSyntaxTrees(System.Collections.Generic.IReadOnlyList{ModifiedSyntaxTree}?,System.Collections.Generic.IReadOnlyList{Microsoft.CodeAnalysis.SyntaxTree}?)"/>.
        /// </summary>
        public Compilation InitialCompilation { get; }

        /// <summary>
        /// Gets the set of modifications present in the current compilation compared to the <see cref="InitialCompilation"/>.
        /// The key of the dictionary is the <see cref="SyntaxTree.FilePath"/> and the value is a <see cref="SyntaxTree"/>
        /// of <see cref="Compilation"/>. 
        /// </summary>
        public ImmutableDictionary<string, ModifiedSyntaxTree> ModifiedSyntaxTrees { get; }

        /// <summary>
        /// Gets the Roslyn <see cref="Microsoft.CodeAnalysis.Compilation"/>.
        /// </summary>
        public Compilation Compilation { get; }

        /// <summary>
        /// Gets the list of syntax trees in the current subset.
        /// </summary>
        public abstract ImmutableDictionary<string, SyntaxTree> SyntaxTrees { get; }

        /// <summary>
        /// Gets the types declared in the current subset.
        /// </summary>
        public abstract IEnumerable<ITypeSymbol> Types { get; }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="PartialCompilation"/> is actually partial, or represents a complete compilation.
        /// </summary>
        public abstract bool IsPartial { get; }

        public bool IsEmpty => this.SyntaxTrees.Count == 0;

        // Initial constructor.
        private PartialCompilation( Compilation compilation )
        {
            this.Compilation = this.InitialCompilation = compilation;
            this.ModifiedSyntaxTrees = ImmutableDictionary<string, ModifiedSyntaxTree>.Empty;
        }

        // Incremental constructor.
        private PartialCompilation(
            PartialCompilation baseCompilation,
            IReadOnlyList<ModifiedSyntaxTree>? modifiedSyntaxTrees,
            IReadOnlyList<SyntaxTree>? addedSyntaxTrees )
        {
            this.InitialCompilation = baseCompilation.InitialCompilation;
            var compilation = baseCompilation.Compilation;

            var modifiedTreeBuilder = baseCompilation.ModifiedSyntaxTrees.ToBuilder();

            if ( addedSyntaxTrees != null )
            {
                compilation = compilation.AddSyntaxTrees( addedSyntaxTrees );

                modifiedTreeBuilder.AddRange(
                    addedSyntaxTrees.Select( t => new KeyValuePair<string, ModifiedSyntaxTree>( t.FilePath, new ModifiedSyntaxTree( t ) ) ) );
            }

            if ( modifiedSyntaxTrees != null )
            {
                foreach ( var replacement in modifiedSyntaxTrees )
                {
                    var oldTree = replacement.OldTree.AssertNotNull();
                    compilation = compilation.ReplaceSyntaxTree( oldTree, replacement.NewTree );

                    // Find the tree in InitialCompilation.
                    SyntaxTree initialTree;

                    if ( baseCompilation.ModifiedSyntaxTrees.TryGetValue( replacement.FilePath, out var initialTreeReplacement )
                         && initialTreeReplacement.OldTree != null )
                    {
                        initialTree = initialTreeReplacement.OldTree;
                    }
                    else if ( !baseCompilation.SyntaxTrees.TryGetValue( replacement.FilePath, out initialTree! ) )
                    {
                        initialTree = replacement.OldTree.AssertNotNull();
                    }

                    modifiedTreeBuilder[replacement.FilePath] = new ModifiedSyntaxTree( replacement.NewTree, initialTree );
                }
            }

            this.ModifiedSyntaxTrees = modifiedTreeBuilder.ToImmutable();
            this.Compilation = compilation;
        }

        /// <summary>
        /// Creates a <see cref="PartialCompilation"/> that represents a complete compilation.
        /// </summary>
        public static PartialCompilation CreateComplete( Compilation compilation ) => new CompleteImpl( compilation );

        /// <summary>
        /// Creates a <see cref="PartialCompilation"/> for a single syntax tree and its closure.
        /// </summary>
        public static PartialCompilation CreatePartial( Compilation compilation, SyntaxTree syntaxTree )
        {
            var syntaxTrees = new[] { syntaxTree };
            var closure = GetClosure( compilation, syntaxTrees );

            return new PartialImpl( compilation, closure.Trees.ToImmutableDictionary( t => t.FilePath, t => t ), closure.Types.ToImmutableArray() );
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

            return new PartialImpl( compilation, closure.Trees.ToImmutableDictionary( t => t.FilePath, t => t ), closure.Types.ToImmutableArray() );
        }

        IPartialCompilation IPartialCompilation.UpdateSyntaxTrees(
            IReadOnlyList<ModifiedSyntaxTree>? replacedTrees,
            IReadOnlyList<SyntaxTree>? addedTrees )
            => this.UpdateSyntaxTrees( replacedTrees, addedTrees );

        /// <summary>
        ///  Adds and replaces syntax trees of the current <see cref="PartialCompilation"/> and returns a new <see cref="PartialCompilation"/>
        /// representing the modified object.
        /// </summary>
        public abstract PartialCompilation UpdateSyntaxTrees(
            IReadOnlyList<ModifiedSyntaxTree>? replacedTrees = null,
            IReadOnlyList<SyntaxTree>? addedTrees = null );

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
                if ( type is IErrorTypeSymbol )
                {
                    return;
                }

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

        public override string ToString()
            => $"{{Assembly={this.Compilation.AssemblyName}, SyntaxTrees={this.SyntaxTrees.Count}/{this.Compilation.SyntaxTrees.Count()}}}";
    }
}