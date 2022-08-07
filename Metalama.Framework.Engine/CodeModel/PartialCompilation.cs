﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel
{
    /// <summary>
    /// Represents a subset of a Roslyn <see cref="Microsoft.CodeAnalysis.Compilation"/>. The subset is limited
    /// to specific syntax trees.
    /// </summary>
    public abstract partial class PartialCompilation : IPartialCompilationInternal
    {
        public DerivedTypeIndex DerivedTypes { get; }

        /// <summary>
        /// Gets the set of modifications present in the current compilation compared to the <see cref="InitialCompilation"/>.
        /// The key of the dictionary is the <see cref="SyntaxTree.FilePath"/> and the value is a <see cref="SyntaxTree"/>
        /// of <see cref="Compilation"/>. 
        /// </summary>
        public ImmutableDictionary<string, SyntaxTreeTransformation> ModifiedSyntaxTrees { get; }

        /// <summary>
        /// Gets the Roslyn <see cref="Microsoft.CodeAnalysis.Compilation"/>.
        /// </summary>
        public Compilation Compilation { get; }

        /// <summary>
        /// Gets the list of syntax trees in the current subset indexed by path.
        /// </summary>
        public abstract ImmutableDictionary<string, SyntaxTree> SyntaxTrees { get; }

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

        internal LanguageOptions LanguageOptions
        {
            get
            {
                if ( this.SyntaxTrees.Count > 0 )
                {
                    var parseOptions = (CSharpParseOptions) this.SyntaxTrees.Values.First().Options;

                    return new LanguageOptions( parseOptions );
                }
                else
                {
                    return LanguageOptions.Default;
                }
            }
        }

        // Initial constructor.
        private PartialCompilation( Compilation compilation, DerivedTypeIndex derivedTypeIndex, ImmutableArray<ManagedResource> resources )
        {
            this.Compilation = this.InitialCompilation = compilation;
            this.ModifiedSyntaxTrees = ImmutableDictionary<string, SyntaxTreeTransformation>.Empty;
            this.Resources = resources.IsDefault ? ImmutableArray<ManagedResource>.Empty : resources;
            this.DerivedTypes = derivedTypeIndex;
        }

        // Incremental constructor.
        private PartialCompilation(
            PartialCompilation baseCompilation,
            IReadOnlyList<SyntaxTreeTransformation>? modifications,
            ImmutableArray<ManagedResource> newResources )
        {
            this.InitialCompilation = baseCompilation.InitialCompilation;
            var compilation = baseCompilation.Compilation;

            this.DerivedTypes = baseCompilation.DerivedTypes;

            // TODO: accept new relationships to the type index.

            var modifiedTreeBuilder = baseCompilation.ModifiedSyntaxTrees.ToBuilder();

            if ( modifications != null )
            {
                foreach ( var transformation in modifications )
                {
                    if ( transformation.Kind == SyntaxTreeTransformationKind.None )
                    {
                        continue;
                    }

                    // Find the tree in InitialCompilation.
                    SyntaxTree? initialTree;

                    if ( transformation.OldTree == null )
                    {
                        initialTree = null;
                    }
                    else if ( baseCompilation.ModifiedSyntaxTrees.TryGetValue( transformation.FilePath, out var initialTreeReplacement ) )
                    {
                        initialTree = initialTreeReplacement.OldTree;
                    }
                    else if ( !baseCompilation.SyntaxTrees.TryGetValue( transformation.FilePath, out initialTree! ) )
                    {
                        initialTree = transformation.OldTree.AssertNotNull();
                    }

                    SyntaxTreeTransformation? transformationFromInitialCompilation;

                    switch ( transformation.Kind )
                    {
                        case SyntaxTreeTransformationKind.Add:
                            compilation = compilation.AddSyntaxTrees( transformation.NewTree! );
                            transformationFromInitialCompilation = transformation;

                            break;

                        case SyntaxTreeTransformationKind.Replace:
                            var newTree = transformation.NewTree.AssertNotNull();
                            compilation = compilation.ReplaceSyntaxTree( transformation.OldTree.AssertNotNull(), newTree );

                            if ( initialTree != null )
                            {
                                transformationFromInitialCompilation = SyntaxTreeTransformation.ReplaceTree( initialTree, newTree );
                            }
                            else
                            {
                                transformationFromInitialCompilation = SyntaxTreeTransformation.AddTree( newTree );
                            }

                            break;

                        case SyntaxTreeTransformationKind.Remove:
                            compilation = compilation.RemoveSyntaxTrees( transformation.OldTree.AssertNotNull() );

                            if ( initialTree != null )
                            {
                                transformationFromInitialCompilation = SyntaxTreeTransformation.RemoveTree( initialTree );
                            }
                            else
                            {
                                transformationFromInitialCompilation = null;
                            }

                            break;

                        default:
                            throw new AssertionFailedException();
                    }

                    if ( transformationFromInitialCompilation != null )
                    {
                        modifiedTreeBuilder[transformation.FilePath] = transformationFromInitialCompilation.Value;
                    }
                    else
                    {
                        modifiedTreeBuilder.Remove( transformation.FilePath );
                    }
                }
            }

            this.ModifiedSyntaxTrees = modifiedTreeBuilder.ToImmutable();
            this.Compilation = compilation;
            this.Resources = newResources.IsDefault ? ImmutableArray<ManagedResource>.Empty : newResources;
        }

        /// <summary>
        /// Creates a <see cref="PartialCompilation"/> that represents a complete compilation.
        /// </summary>
        public static PartialCompilation CreateComplete( Compilation compilation, ImmutableArray<ManagedResource> resources = default )
            => new CompleteImpl( compilation, GetDerivedTypeIndex( compilation ), resources );

        /// <summary>
        /// Creates a <see cref="PartialCompilation"/> for a single syntax tree and its closure.
        /// </summary>
        public static PartialCompilation CreatePartial(
            Compilation compilation,
            SyntaxTree syntaxTree,
            ImmutableArray<ManagedResource> resources = default )
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
            ImmutableArray<ManagedResource> resources = default )
        {
            var closure = GetClosure( compilation, syntaxTrees );

            return new PartialImpl(
                compilation,
                closure.Trees.ToImmutableDictionary( t => t.FilePath, t => t ),
                closure.Types.ToImmutableHashSet(),
                closure.DerivedTypes,
                resources );
        }

        IPartialCompilation IPartialCompilation.WithSyntaxTreeTransformations( IReadOnlyList<SyntaxTreeTransformation>? transformations )
            => this.Update( transformations );

        public IPartialCompilation WithAdditionalResources( params ManagedResource[] resources ) => this.Update( null, this.Resources.AddRange( resources ) );

        public ImmutableArray<ManagedResource> Resources { get; }

        /// <summary>
        ///  Adds and replaces syntax trees of the current <see cref="PartialCompilation"/> and returns a new <see cref="PartialCompilation"/>
        /// representing the modified object.
        /// </summary>
        public abstract PartialCompilation Update(
            IReadOnlyList<SyntaxTreeTransformation>? transformations = null,
            ImmutableArray<ManagedResource> resources = default );

        /// <summary>
        /// Gets a closure of the syntax trees declaring all base types and interfaces of all types declared in input syntax trees.
        /// </summary>
        private static (ImmutableHashSet<INamedTypeSymbol> Types, ImmutableHashSet<SyntaxTree> Trees, DerivedTypeIndex DerivedTypes)
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
                        var baseType = type.BaseType.OriginalDefinition;
                        derivedTypesBuilder.AddDerivedType( baseType, type );
                        AddTypeRecursive( baseType );
                    }

                    foreach ( var interfaceImpl in type.Interfaces )
                    {
                        var interfaceType = interfaceImpl.OriginalDefinition;
                        derivedTypesBuilder.AddDerivedType( interfaceType, type );
                        AddTypeRecursive( interfaceType );
                    }
                }
                else
                {
                    // The type was already processed.
                }
            }

            foreach ( var syntaxTree in syntaxTrees )
            {
                // We need to add the SyntaxTree even if it does not contain any type.
                trees.Add( syntaxTree );

                var semanticModel = compilation.GetSemanticModel( syntaxTree );

                // Add all types in this syntax tree, as well as all base types.
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

        private static DerivedTypeIndex GetDerivedTypeIndex( Compilation compilation )
        {
            DerivedTypeIndex.Builder builder = new( compilation );

            foreach ( var type in compilation.Assembly.GetTypes() )
            {
                builder.AnalyzeType( type );
            }

            return builder.ToImmutable();
        }

        public ImmutableArray<SyntaxTreeTransformation> ToTransformations() => this.ModifiedSyntaxTrees.Values.ToImmutableArray();

        public override string ToString()
            => $"{{Assembly={this.Compilation.AssemblyName}, SyntaxTrees={this.SyntaxTrees.Count}/{this.Compilation.SyntaxTrees.Count()}}}";

        /// <summary>
        /// Gets the compilation with respect to which the <see cref="ModifiedSyntaxTrees"/> collection has been constructed.
        /// Typically, this is the argument of the <see cref="CreateComplete"/> or <see cref="CreatePartial(Microsoft.CodeAnalysis.Compilation,Microsoft.CodeAnalysis.SyntaxTree,System.Collections.Immutable.ImmutableArray{Metalama.Compiler.ManagedResource})"/>
        /// method, ignoring any modification done by <see cref="Update"/>.
        /// </summary>
        public Compilation InitialCompilation { get; }

        private void Validate( IReadOnlyList<SyntaxTreeTransformation>? transformations )
        {
            // In production scenario, we need weavers to provide SyntaxTree instances with a valid Encoding value.
            // However, we don't need that in test scenarios, and tests currently don't set Encoding properly.
            // The way this test is implemented is to test Encoding in increments only if it is set properly in the initial compilation.
            // It also happens, at design time, that Roslyn does not set the encoding. We also need to be tolerant to this situation.

            bool HasInitialCompilationEncoding() => this.InitialCompilation.SyntaxTrees.All( t => t.Encoding != null );

            if ( transformations != null )
            {
                if ( transformations.Any( t => string.IsNullOrEmpty( t.FilePath ) ) )
                {
                    throw new ArgumentOutOfRangeException( nameof(transformations), "The SyntaxTree.FilePath property must be set to a non-empty value." );
                }

                if ( transformations.Any( t => t.NewTree != null && string.IsNullOrEmpty( t.NewTree.FilePath ) ) )
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(transformations),
                        "The SyntaxTree.FilePath property of the new SyntaxTree must be set to a non-empty value." );
                }

                if ( transformations.Any( t => t.NewTree is { Encoding: null } && t.OldTree?.Encoding != null ) && HasInitialCompilationEncoding() )
                {
                    var invalidTrees = transformations.Where( t => t.NewTree is { Encoding: null } ).Select( x => $"'{x.FilePath}'" );

                    throw new ArgumentOutOfRangeException(
                        nameof(transformations),
                        $"The SyntaxTree.Encoding property of these SyntaxTrees cannot be null: {string.Join( ", ", invalidTrees )}" );
                }
            }
        }
    }
}