// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class CompilationModel : ICompilation, ICodeElementInternal
    {
        public PartialCompilation PartialCompilation { get; }

        public static CompilationModel CreateInitialInstance( PartialCompilation compilation ) => new( compilation );

        public static CompilationModel CreateInitialInstance( Compilation compilation ) => new( PartialCompilation.CreateComplete( compilation ) );

        public static CompilationModel CreateInitialInstance( Compilation compilation, SyntaxTree syntaxTree )
            => new( PartialCompilation.CreatePartial( compilation, syntaxTree ) );

        internal static CompilationModel CreateRevisedInstance( CompilationModel prototype, IReadOnlyList<IObservableTransformation> introducedElements )
        {
            if ( !introducedElements.Any() )
            {
                return prototype;
            }

            return new CompilationModel( prototype, introducedElements );
        }

        internal ReflectionMapper ReflectionMapper { get; }

        private readonly ImmutableMultiValueDictionary<CodeElementLink<ICodeElement>, IObservableTransformation> _transformations;

        // This collection index all attributes on types and members, but not attributes on the assembly and the module.
        private readonly ImmutableMultiValueDictionary<CodeElementLink<INamedType>, AttributeLink> _allMemberAttributesByType;

        private ImmutableDictionary<CodeElementLink<ICodeElement>, int> _depthsCache = ImmutableDictionary.Create<CodeElementLink<ICodeElement>, int>();

        public CodeElementFactory Factory { get; }

        protected CompilationModel( PartialCompilation partialCompilation )
        {
            this.PartialCompilation = partialCompilation;
            this.ReflectionMapper = ReflectionMapper.GetInstance( this.RoslynCompilation );
            this.InvariantComparer = new CodeElementEqualityComparer( this.ReflectionMapper, this.RoslynCompilation );

            this._transformations = ImmutableMultiValueDictionary<CodeElementLink<ICodeElement>, IObservableTransformation>
                .Empty
                .WithKeyComparer( CodeElementLinkEqualityComparer<CodeElementLink<ICodeElement>>.Instance );

            this.Factory = new CodeElementFactory( this );

            // TODO: Move this to a virtual/lazy method because this should not be done for a partial model.

            var allCodeElements = this.PartialCompilation.Types.SelectManyRecursive<ISymbol>( s => s.GetContainedSymbols(), includeFirstLevel: true );

            var allAttributes = allCodeElements.SelectMany( c => c.GetAllAttributes() );

            this._allMemberAttributesByType = ImmutableMultiValueDictionary<CodeElementLink<INamedType>, AttributeLink>
                .Create( allAttributes, a => a.AttributeType, CodeElementLinkEqualityComparer<CodeElementLink<INamedType>>.Instance );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilationModel"/> class that is based on a prototype instance but appends transformations.
        /// </summary>
        /// <param name="prototype"></param>
        /// <param name="observableTransformations"></param>
        private CompilationModel( CompilationModel prototype, IReadOnlyList<IObservableTransformation> observableTransformations )
        {
            this.Revision = prototype.Revision + 1;
            this.PartialCompilation = prototype.PartialCompilation;
            this.ReflectionMapper = prototype.ReflectionMapper;
            this.InvariantComparer = prototype.InvariantComparer;

            this._transformations = prototype._transformations.AddRange(
                observableTransformations,
                t => t.ContainingElement.ToLink(),
                t => t );

            this.Factory = new CodeElementFactory( this );

            var allNewCodeElements =
                observableTransformations
                    .OfType<ICodeElement>()
                    .SelectManyRecursive( codeElement => codeElement.GetContainedElements() );

            var allAttributes =
                allNewCodeElements.SelectMany( c => c.Attributes )
                    .Cast<AttributeBuilder>()
                    .Concat( observableTransformations.OfType<AttributeBuilder>() )
                    .Select( a => new AttributeLink( a ) );

            // TODO: Process IRemoveMember.

            // TODO: this cache may need to be smartly invalidated when we have interface introductions.
            this._depthsCache = prototype._depthsCache;

            this._allMemberAttributesByType = prototype._allMemberAttributesByType.AddRange( allAttributes, a => a.AttributeType );
        }

        internal CSharpSyntaxGenerator SyntaxGenerator { get; } = new();

        public int Revision { get; }

        [Memo]
        public INamedTypeList DeclaredTypes
            => new NamedTypeList(
                this.PartialCompilation.Types
                    .Select( t => new MemberLink<INamedType>( t ) ),
                this );

        [Memo]
        public IAttributeList Attributes
            => new AttributeList(
                this.RoslynCompilation.Assembly
                    .GetAttributes()
                    .Union( this.RoslynCompilation.SourceModule.GetAttributes() )
                    .Select( a => new AttributeLink( a, this.RoslynCompilation.Assembly.ToLink() ) ),
                this );

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.RoslynCompilation.AssemblyName ?? "<Anonymous>";

        public Compilation RoslynCompilation => this.PartialCompilation.Compilation;

        ITypeFactory ICompilation.TypeFactory => this.Factory;

        public IReadOnlyList<IManagedResource> ManagedResources => throw new NotImplementedException();

        public ICodeElementComparer InvariantComparer { get; }

        ICodeElement? ICodeElement.ContainingElement => null;

        CodeElementKind ICodeElement.ElementKind => CodeElementKind.Compilation;

        public bool Equals( ICodeElement other ) => throw new NotImplementedException();

        ICompilation ICompilationElement.Compilation => this;

        public IEnumerable<IAttribute> GetAllAttributesOfType( INamedType type )
            => this._allMemberAttributesByType[type.ToLink()].Select( a => a.GetForCompilation( this ) );

        internal ImmutableArray<IObservableTransformation> GetObservableTransformationsOnElement( ICodeElement codeElement )
            => this._transformations[codeElement.ToLink()];

        internal IEnumerable<(ICodeElement DeclaringElement, IEnumerable<IObservableTransformation> Transformations)> GetAllObservableTransformations()
        {
            foreach ( var group in this._transformations )
            {
                yield return (group.Key.GetForCompilation( this ), group);
            }
        }

        internal int GetDepth( ICodeElement codeElement )
        {
            var link = codeElement.ToLink();

            if ( this._depthsCache.TryGetValue( link, out var value ) )
            {
                return value;
            }

            switch ( codeElement )
            {
                case INamedType namedType:
                    return this.GetDepth( namedType );

                case ICompilation:
                    return 0;

                case IAssembly:
                    // Order with Compilation matters. We want the root compilation to be ordered first.
                    return 1;

                default:
                    {
                        var depth = this.GetDepth( codeElement.ContainingElement! ) + 1;
                        this._depthsCache = this._depthsCache.SetItem( link, depth );

                        return depth;
                    }
            }
        }

        internal int GetDepth( INamedType namedType )
        {
            var link = namedType.ToLink<ICodeElement>();

            if ( this._depthsCache.TryGetValue( link, out var depth ) )
            {
                return depth;
            }

            depth = this.GetDepth( namedType.ContainingElement! );

            if ( namedType.BaseType != null )
            {
                depth = Math.Max( depth, this.GetDepth( namedType.BaseType ) );
            }

            foreach ( var interfaceImplementation in namedType.ImplementedInterfaces )
            {
                depth = Math.Max( depth, this.GetDepth( interfaceImplementation ) );
            }

            depth++;

            this._depthsCache = this._depthsCache.SetItem( link, depth );

            return depth;
        }

        CodeElementLink<ICodeElement> ICodeElementInternal.ToLink() => CodeElementLink.Compilation();

        ImmutableArray<SyntaxReference> ICodeElementInternal.DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

        string IDisplayable.ToDisplayString( CodeDisplayFormat? format, CodeDisplayContext? context )
        {
            throw new NotImplementedException();
        }

        CodeOrigin ICodeElement.Origin => CodeOrigin.Source;

        ISymbol? ISdkCodeElement.Symbol => this.RoslynCompilation.Assembly;

        IAttributeList ICodeElement.Attributes => throw new NotSupportedException();

        IDiagnosticLocation? IDiagnosticScope.DiagnosticLocation => null;

        public string? Name => this.RoslynCompilation.AssemblyName;
    }
}