﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
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
        /// The compilation with respect to which the <see cref="ModifiedSyntaxTrees"/> collection has been constructed.
        /// Typically, this is the argument of the <see cref="CreateComplete"/> or <see cref="CreatePartial(Microsoft.CodeAnalysis.Compilation,Microsoft.CodeAnalysis.SyntaxTree,System.Collections.Immutable.ImmutableArray{Microsoft.CodeAnalysis.ResourceDescription})"/>
        /// method, ignoring any modification done by <see cref="Update"/>.
        /// </summary>
        private readonly Compilation _initialCompilation;

        internal DerivedTypeIndex DerivedTypes { get; }

        /// <summary>
        /// Gets the set of modifications present in the current compilation compared to the <see cref="_initialCompilation"/>.
        /// The key of the dictionary is the <see cref="SyntaxTree.FilePath"/> and the value is a <see cref="SyntaxTree"/>
        /// of <see cref="Compilation"/>. 
        /// </summary>
        public ImmutableDictionary<string, SyntaxTreeModification> ModifiedSyntaxTrees { get; }

        /// <summary>
        /// Gets the Roslyn <see cref="Microsoft.CodeAnalysis.Compilation"/>.
        /// </summary>
        public Compilation Compilation { get; }

        /// <summary>
        /// Gets the list of syntax trees in the current subset indexed by path.
        /// </summary>
        public abstract ImmutableDictionary<string, SyntaxTree> SyntaxTrees { get; }

        public ImmutableHashSet<INamedTypeSymbol> BaseExternalTypes => this.DerivedTypes.ExternalBaseTypes;

        /// <summary>
        /// Gets the types declared in the current subset.
        /// </summary>
        public abstract ImmutableHashSet<INamedTypeSymbol> Types { get; }

        /// <summary>
        /// Gets the namespaces that contain types.
        /// </summary>
        public abstract ImmutableHashSet<INamespaceSymbol> Namespaces { get; }

        [Memo]
        public ImmutableHashSet<INamespaceSymbol> ParentNamespaces
            => this.Namespaces.SelectRecursive( n => n.IsGlobalNamespace ? null : n.ContainingNamespace )
                .ToImmutableHashSet();

        /// <summary>
        /// Gets a value indicating whether the current <see cref="PartialCompilation"/> is actually partial, or represents a complete compilation.
        /// </summary>
        public abstract bool IsPartial { get; }

        public bool IsEmpty => this.SyntaxTrees.Count == 0;

        // Initial constructor.
        private PartialCompilation( Compilation compilation, DerivedTypeIndex derivedTypeIndex, ImmutableArray<ResourceDescription> resources )
        {
            this.Compilation = this._initialCompilation = compilation;
            this.ModifiedSyntaxTrees = ImmutableDictionary<string, SyntaxTreeModification>.Empty;
            this.Resources = resources;
            this.DerivedTypes = derivedTypeIndex;
        }

        // Incremental constructor.
        private PartialCompilation(
            PartialCompilation baseCompilation,
            IReadOnlyList<SyntaxTreeModification>? modifiedSyntaxTrees,
            IReadOnlyList<SyntaxTree>? addedSyntaxTrees,
            ImmutableArray<ResourceDescription>? newResources )
        {
            this._initialCompilation = baseCompilation._initialCompilation;
            var compilation = baseCompilation.Compilation;

            this.DerivedTypes = baseCompilation.DerivedTypes;

            // TODO: accept new relationships to the type index.

            var modifiedTreeBuilder = baseCompilation.ModifiedSyntaxTrees.ToBuilder();

            if ( addedSyntaxTrees != null )
            {
                compilation = compilation.AddSyntaxTrees( addedSyntaxTrees );

                modifiedTreeBuilder.AddRange(
                    addedSyntaxTrees.Select( t => new KeyValuePair<string, SyntaxTreeModification>( t.FilePath, new SyntaxTreeModification( t ) ) ) );
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

                    modifiedTreeBuilder[replacement.FilePath] = new SyntaxTreeModification( replacement.NewTree, initialTree );
                }
            }

            this.ModifiedSyntaxTrees = modifiedTreeBuilder.ToImmutable();
            this.Compilation = compilation;
            this.Resources = newResources ?? baseCompilation.Resources;
        }

        /// <summary>
        /// Creates a <see cref="PartialCompilation"/> that represents a complete compilation.
        /// </summary>
        public static PartialCompilation CreateComplete( Compilation compilation, ImmutableArray<ResourceDescription> resources = default )
            => new CompleteImpl( compilation, resources );

        /// <summary>
        /// Creates a <see cref="PartialCompilation"/> for a single syntax tree and its closure.
        /// </summary>
        public static PartialCompilation CreatePartial(
            Compilation compilation,
            SyntaxTree syntaxTree,
            ImmutableArray<ResourceDescription> resources = default )
        {
            var syntaxTrees = new[] { syntaxTree };
            var closure = GetClosure( compilation, syntaxTrees );

            return new PartialImpl(
                compilation,
                closure.Trees.ToImmutableDictionary( t => t.FilePath, t => t ),
                closure.Types,
                closure.DerivedTypes,
                resources );
        }

        /// <summary>
        /// Creates a <see cref="PartialCompilation"/> for a given subset of syntax trees and its closure.
        /// </summary>
        public static PartialCompilation CreatePartial(
            Compilation compilation,
            IReadOnlyList<SyntaxTree> syntaxTrees,
            ImmutableArray<ResourceDescription> resources = default )
        {
            var closure = GetClosure( compilation, syntaxTrees );

            return new PartialImpl(
                compilation,
                closure.Trees.ToImmutableDictionary( t => t.FilePath, t => t ),
                closure.Types.ToImmutableHashSet(),
                closure.DerivedTypes,
                resources );
        }

        IPartialCompilation IPartialCompilation.WithSyntaxTreeModifications(
            IReadOnlyList<SyntaxTreeModification>? modifications,
            IReadOnlyList<SyntaxTree>? additions )
            => this.Update( modifications, additions );

        public IPartialCompilation WithAdditionalResources( params ResourceDescription[] resources )
            => this.Update( null, null, this.Resources.AddRange( resources ) );

        public ImmutableArray<ResourceDescription> Resources { get; }

        /// <summary>
        ///  Adds and replaces syntax trees of the current <see cref="PartialCompilation"/> and returns a new <see cref="PartialCompilation"/>
        /// representing the modified object.
        /// </summary>
        public abstract PartialCompilation Update(
            IReadOnlyList<SyntaxTreeModification>? replacedTrees = null,
            IReadOnlyList<SyntaxTree>? addedTrees = null,
            ImmutableArray<ResourceDescription>? resources = null );

        /// <summary>
        /// Gets a closure of the syntax trees declaring all base types and interfaces of all types declared in input syntax trees.
        /// </summary>
        private static (ImmutableHashSet<INamedTypeSymbol> Types, ImmutableHashSet<SyntaxTree> Trees,
            DerivedTypeIndex DerivedTypes )
            GetClosure( Compilation compilation, IReadOnlyList<SyntaxTree> syntaxTrees )
        {
            var assembly = compilation.Assembly;

            var types = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>( SymbolEqualityComparer.Default );
            var trees = ImmutableHashSet.CreateBuilder<SyntaxTree>();
            var derivedTypesBuilder = new DerivedTypeIndex.Builder( compilation );

            void AddTypeRecursive( INamedTypeSymbol type )
            {
                if ( type is IErrorTypeSymbol )
                {
                    return;
                }

                var isExternal = !SymbolEqualityComparer.Default.Equals( type.ContainingAssembly, assembly );

                if ( isExternal )
                {
                    // If the type is not defined in the current assembly, analyze it using the DerivedTypeIndexBuilder so that
                    // it does not get included in the set of types in the current partial compilation.
                    derivedTypesBuilder.AnalyzeType( type );
                }
                else if ( types.Add( type ) )
                {
                    // Find relevant syntax trees
                    foreach ( var syntaxReference in type.DeclaringSyntaxReferences )
                    {
                        trees.Add( syntaxReference.SyntaxTree );
                    }

                    // Add base types recursively.
                    if ( type.BaseType != null )
                    {
                        derivedTypesBuilder.AddDerivedType( type.BaseType.OriginalDefinition, type );
                        AddTypeRecursive( type.BaseType.OriginalDefinition );
                    }

                    foreach ( var interfaceImpl in type.Interfaces )
                    {
                        derivedTypesBuilder.AddDerivedType( interfaceImpl.OriginalDefinition, type );
                        AddTypeRecursive( interfaceImpl.OriginalDefinition );
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

            return (types.ToImmutable(), trees.ToImmutable(), derivedTypesBuilder.ToImmutable());
        }

        public ImmutableArray<SyntaxTreeTransformation> ToTransformations()
            => this.ModifiedSyntaxTrees.Values.Select( t => new SyntaxTreeTransformation( t.NewTree, t.OldTree ) ).ToImmutableArray();

        public override string ToString()
            => $"{{Assembly={this.Compilation.AssemblyName}, SyntaxTrees={this.SyntaxTrees.Count}/{this.Compilation.SyntaxTrees.Count()}}}";
    }
}