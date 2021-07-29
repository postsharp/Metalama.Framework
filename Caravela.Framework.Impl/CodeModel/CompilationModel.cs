﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Impl.CodeModel;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class CompilationModel : ICompilationInternal, IDeclarationInternal
    {
        public PartialCompilation PartialCompilation { get; }

        public static CompilationModel CreateInitialInstance( PartialCompilation compilation ) => new( compilation );

        public static CompilationModel CreateInitialInstance( Compilation compilation ) => new( PartialCompilation.CreateComplete( compilation ) );

        public static CompilationModel CreateInitialInstance( Compilation compilation, SyntaxTree syntaxTree )
            => new( PartialCompilation.CreatePartial( compilation, syntaxTree ) );

        internal CompilationModel WithTransformations( IReadOnlyList<IObservableTransformation> introducedDeclarations )
        {
            if ( !introducedDeclarations.Any() )
            {
                return this;
            }

            return new CompilationModel( this, introducedDeclarations );
        }

        /// <summary>
        /// Returns a shallow clone of the current compilation, but annotated with a given <see cref="AspectLayerId"/>.
        /// </summary>
        internal CompilationModel WithAspectLayer( AspectLayerId aspectLayerId ) => new( this, aspectLayerId );

        public CompilationModel WithAspectInstances( IReadOnlyList<AspectInstance> aspectInstances )
            => aspectInstances.Count == 0 ? this : new CompilationModel( this, aspectInstances );

        internal ReflectionMapper ReflectionMapper { get; }

        private readonly ImmutableMultiValueDictionary<DeclarationRef<IDeclaration>, IObservableTransformation> _transformations;

        // This collection index all attributes on types and members, but not attributes on the assembly and the module.
        private readonly ImmutableMultiValueDictionary<DeclarationRef<INamedType>, AttributeRef> _allMemberAttributesByType;

        private readonly ImmutableMultiValueDictionary<DeclarationRef<IDeclaration>, IAspectInstance> _aspects;

        private ImmutableDictionary<DeclarationRef<IDeclaration>, int> _depthsCache = ImmutableDictionary.Create<DeclarationRef<IDeclaration>, int>();

        public DeclarationFactory Factory { get; }

        private CompilationModel( PartialCompilation partialCompilation )
        {
            this.PartialCompilation = partialCompilation;
            this.ReflectionMapper = ReflectionMapper.GetInstance( this.RoslynCompilation );
            this.InvariantComparer = new DeclarationEqualityComparer( this.ReflectionMapper, this.RoslynCompilation );

            this._transformations = ImmutableMultiValueDictionary<DeclarationRef<IDeclaration>, IObservableTransformation>
                .Empty
                .WithKeyComparer( DeclarationRefEqualityComparer<DeclarationRef<IDeclaration>>.Instance );

            this.Factory = new DeclarationFactory( this );

            var allDeclarations =
                this.PartialCompilation.Types.SelectManyRecursive<ISymbol>( s => s.GetContainedSymbols(), includeFirstLevel: true, throwOnDuplicate: false )
                    .Concat( new[] { (ISymbol) this.PartialCompilation.Compilation.Assembly, this.PartialCompilation.Compilation.SourceModule } );

            var allAttributes = allDeclarations.SelectMany( c => c.GetAllAttributes() );

            this._allMemberAttributesByType = ImmutableMultiValueDictionary<DeclarationRef<INamedType>, AttributeRef>
                .Create( allAttributes, a => a.AttributeType, DeclarationRefEqualityComparer<DeclarationRef<INamedType>>.Instance );

            this._aspects = ImmutableMultiValueDictionary<DeclarationRef<IDeclaration>, IAspectInstance>.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilationModel"/> class that is based on a prototype instance but appends transformations.
        /// </summary>
        /// <param name="prototype"></param>
        /// <param name="observableTransformations"></param>
        private CompilationModel( CompilationModel prototype, IReadOnlyList<IObservableTransformation> observableTransformations ) : this( prototype )
        {
            this._transformations = prototype._transformations.AddRange(
                observableTransformations,
                t => t.ContainingDeclaration.ToRef(),
                t => t );

            var allNewDeclarations =
                observableTransformations
                    .OfType<IDeclaration>()
                    .SelectManyRecursive( declaration => declaration.GetContainedElements() );

            var allAttributes =
                allNewDeclarations.SelectMany( c => c.Attributes )
                    .Cast<AttributeBuilder>()
                    .Concat( observableTransformations.OfType<AttributeBuilder>() )
                    .Select( a => new AttributeRef( a ) );

            // TODO: Process IRemoveMember.

            // TODO: this cache may need to be smartly invalidated when we have interface introductions.

            this._allMemberAttributesByType = prototype._allMemberAttributesByType.AddRange( allAttributes, a => a.AttributeType );
        }

        private CompilationModel( CompilationModel prototype )
        {
            this.Revision = prototype.Revision + 1;

            this.AspectLayerId = prototype.AspectLayerId;
            this.PartialCompilation = prototype.PartialCompilation;
            this.ReflectionMapper = prototype.ReflectionMapper;
            this.InvariantComparer = prototype.InvariantComparer;
            this._transformations = prototype._transformations;
            this.Factory = new DeclarationFactory( this );
            this._depthsCache = prototype._depthsCache;
            this._allMemberAttributesByType = prototype._allMemberAttributesByType;
            this._aspects = prototype._aspects;
        }

        private CompilationModel( CompilationModel prototype, AspectLayerId aspectLayerId ) : this( prototype )
        {
            this.AspectLayerId = aspectLayerId;
        }

        private CompilationModel( CompilationModel prototype, IReadOnlyList<IAspectInstance> aspectInstances ) : this( prototype )
        {
            this._aspects = this._aspects.AddRange( aspectInstances, a => a.TargetDeclaration.ToRef() );
        }

        public int Revision { get; }

        [Memo]
        public INamedTypeList DeclaredTypes
            => new NamedTypeList(
                this,
                this.PartialCompilation.Types
                    .Select( t => new MemberRef<INamedType>( t ) ) );

        [Memo]
        public IAttributeList Attributes
            => new AttributeList(
                this,
                this.RoslynCompilation.Assembly
                    .GetAttributes()
                    .Union( this.RoslynCompilation.SourceModule.GetAttributes() )
                    .Select( a => new AttributeRef( a, this.RoslynCompilation.Assembly.ToRef() ) ) );

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.RoslynCompilation.AssemblyName ?? "<Anonymous>";

        public Compilation RoslynCompilation => this.PartialCompilation.Compilation;

        ITypeFactory ICompilation.TypeFactory => this.Factory;

        public IReadOnlyList<IManagedResource> ManagedResources => throw new NotImplementedException();

        public IDeclarationComparer InvariantComparer { get; }

        public IEnumerable<T> GetAspectsOf<T>( IDeclaration declaration )
            where T : IAspect
            => this._aspects[declaration.ToRef()].Select( a => a.Aspect ).OfType<T>();

        // TODO: throw an exception when the caller tries to get aspects that have not been initialized yet.

        IDeclaration? IDeclaration.ContainingDeclaration => null;

        DeclarationKind IDeclaration.DeclarationKind => DeclarationKind.Compilation;

        public bool Equals( IDeclaration other ) => throw new NotImplementedException();

        ICompilation ICompilationElement.Compilation => this;

        public IEnumerable<IAttribute> GetAllAttributesOfType( INamedType type )
            => this._allMemberAttributesByType[type.ToRef()].Select( a => a.Resolve( this ) );

        internal ImmutableArray<IObservableTransformation> GetObservableTransformationsOnElement( IDeclaration declaration )
            => this._transformations[declaration.ToRef()];

        internal IEnumerable<(IDeclaration DeclaringDeclaration, IEnumerable<IObservableTransformation> Transformations)> GetAllObservableTransformations()
        {
            foreach ( var group in this._transformations )
            {
                yield return (group.Key.Resolve( this ), group);
            }
        }

        internal int GetDepth( IDeclaration declaration )
        {
            var reference = declaration.ToRef();

            if ( this._depthsCache.TryGetValue( reference, out var value ) )
            {
                return value;
            }

            switch ( declaration )
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
                        var depth = this.GetDepth( declaration.ContainingDeclaration! ) + 1;
                        this._depthsCache = this._depthsCache.SetItem( reference, depth );

                        return depth;
                    }
            }
        }

        internal int GetDepth( INamedType namedType )
        {
            var reference = namedType.ToRef<IDeclaration>();

            if ( this._depthsCache.TryGetValue( reference, out var depth ) )
            {
                return depth;
            }

            depth = this.GetDepth( namedType.ContainingDeclaration! );

            if ( namedType.BaseType != null )
            {
                depth = Math.Max( depth, this.GetDepth( namedType.BaseType ) );
            }

            foreach ( var interfaceImplementation in namedType.ImplementedInterfaces )
            {
                depth = Math.Max( depth, this.GetDepth( interfaceImplementation ) );
            }

            depth++;

            this._depthsCache = this._depthsCache.SetItem( reference, depth );

            return depth;
        }

        DeclarationRef<IDeclaration> IDeclarationInternal.ToRef() => DeclarationRef.Compilation();

        ImmutableArray<SyntaxReference> IDeclarationInternal.DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

        string IDisplayable.ToDisplayString( CodeDisplayFormat? format, CodeDisplayContext? context )
        {
            throw new NotImplementedException();
        }

        DeclarationOrigin IDeclaration.Origin => DeclarationOrigin.Source;

        ISymbol? ISdkDeclaration.Symbol => this.RoslynCompilation.Assembly;

        IAttributeList IDeclaration.Attributes => throw new NotSupportedException();

        IDiagnosticLocation? IDiagnosticScope.DiagnosticLocation => null;

        public string? Name => this.RoslynCompilation.AssemblyName;

        public AspectLayerId AspectLayerId
        {
            get;
        }

        public override string ToString() => $"{this.RoslynCompilation.AssemblyName}, rev={this.Revision}";

        public ICompilationHelpers Helpers { get; } = new CompilationHelpers();
    }
}