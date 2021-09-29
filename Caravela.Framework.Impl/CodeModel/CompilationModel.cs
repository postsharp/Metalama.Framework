﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Collections;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.References;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.ServiceProvider;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    internal partial class CompilationModel : ICompilationInternal, IDeclarationImpl
    {
        
        public static CompilationModel CreateInitialInstance( IProject project, PartialCompilation compilation ) => new( project, compilation );

        public static CompilationModel CreateInitialInstance(
            IProject project,
            Compilation compilation,
            ImmutableArray<ResourceDescription> resources = default )
            => new( project, PartialCompilation.CreateComplete( compilation, resources ) );

        public static CompilationModel CreateInitialInstance(
            IProject project,
            Compilation compilation,
            SyntaxTree syntaxTree,
            ImmutableArray<ResourceDescription> resources = default )
            => new( project, PartialCompilation.CreatePartial( compilation, syntaxTree, resources ) );

     

        private readonly ImmutableMultiValueDictionary<DeclarationRef<IDeclaration>, IObservableTransformation> _transformations;

        // This collection index all attributes on types and members, but not attributes on the assembly and the module.
        private readonly ImmutableMultiValueDictionary<string, AttributeRef> _allMemberAttributesByTypeName;

        private readonly ImmutableMultiValueDictionary<DeclarationRef<IDeclaration>, IAspectInstance> _aspects;

        private ImmutableDictionary<DeclarationRef<IDeclaration>, int> _depthsCache = ImmutableDictionary.Create<DeclarationRef<IDeclaration>, int>();

        public DeclarationFactory Factory { get; }
        
        public int Revision { get; }

        public IProject Project { get; }
        
        public PartialCompilation PartialCompilation { get; }

        public ReflectionMapper ReflectionMapper { get; }

        public ISymbolClassifier SymbolClassifier { get; }

        private CompilationModel( IProject project, PartialCompilation partialCompilation )
        {
            this.PartialCompilation = partialCompilation;
            this.Project = project;
            this.ReflectionMapper = ReflectionMapper.GetInstance( this.RoslynCompilation );
            this.InvariantComparer = new DeclarationEqualityComparer( this.ReflectionMapper, this.RoslynCompilation );

            this._transformations = ImmutableMultiValueDictionary<DeclarationRef<IDeclaration>, IObservableTransformation>
                .Empty
                .WithKeyComparer( DeclarationRefEqualityComparer<DeclarationRef<IDeclaration>>.Instance );

            this.Factory = new DeclarationFactory( this );

            AttributeDiscoveryVisitor attributeDiscoveryVisitor = new();

            foreach ( var tree in partialCompilation.SyntaxTrees )
            {
                attributeDiscoveryVisitor.Visit( tree.Value.GetRoot() );
            }

            this._allMemberAttributesByTypeName = attributeDiscoveryVisitor.GetDiscoveredAttributes();

            this._aspects = ImmutableMultiValueDictionary<DeclarationRef<IDeclaration>, IAspectInstance>.Empty;
            this.SymbolClassifier = project.ServiceProvider.GetService<SymbolClassificationService>().GetClassifier( this.RoslynCompilation );
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

            // TODO: Performance. The next line essentially instantiates the complete code model. We should look at attributes without doing that. 
            var allNewDeclarations =
                observableTransformations
                    .OfType<IDeclaration>()
                    .SelectMany( declaration => declaration.GetContainedDeclarations() );

            var allAttributes =
                allNewDeclarations.SelectMany( c => c.Attributes )
                    .Cast<AttributeBuilder>()
                    .Concat( observableTransformations.OfType<AttributeBuilder>() )
                    .Select( a => new AttributeRef( a ) );

            // TODO: Process IRemoveMember.

            // TODO: this cache may need to be smartly invalidated when we have interface introductions.

            this._allMemberAttributesByTypeName = prototype._allMemberAttributesByTypeName.AddRange( allAttributes, a => a.AttributeTypeName! );
        }

        private CompilationModel( CompilationModel prototype )
        {
            this.Project = prototype.Project;
            this.Revision = prototype.Revision + 1;

            this.AspectLayerId = prototype.AspectLayerId;
            this.PartialCompilation = prototype.PartialCompilation;
            this.ReflectionMapper = prototype.ReflectionMapper;
            this.InvariantComparer = prototype.InvariantComparer;
            this._transformations = prototype._transformations;
            this.Factory = new DeclarationFactory( this );
            this._depthsCache = prototype._depthsCache;
            this._allMemberAttributesByTypeName = prototype._allMemberAttributesByTypeName;
            this._aspects = prototype._aspects;
            this.SymbolClassifier = prototype.SymbolClassifier;
        }

        private CompilationModel( CompilationModel prototype, AspectLayerId aspectLayerId ) : this( prototype )
        {
            this.AspectLayerId = aspectLayerId;
        }

        private CompilationModel( CompilationModel prototype, IReadOnlyList<IAspectInstance> aspectInstances ) : this( prototype )
        {
            this._aspects = this._aspects.AddRange( aspectInstances, a => a.TargetDeclaration.ToRef() );
        }
        
        
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


        public string AssemblyName => this.RoslynCompilation.AssemblyName ?? "";

        [Memo]
        public INamedTypeList Types
            => new NamedTypeList(
                this,
                this.PartialCompilation.Types
                    .Where( t => this.SymbolClassifier.GetTemplatingScope( t ) != TemplatingScope.CompileTimeOnly )
                    .Select( t => new MemberRef<INamedType>( t ) ) );

        [Memo]
        public IAttributeList Attributes
            => new AttributeList(
                this,
                this.RoslynCompilation.Assembly
                    .GetAttributes()
                    .Union( this.RoslynCompilation.SourceModule.GetAttributes() )
                    .Where( a => a.AttributeConstructor != null )
                    .Select( a => new AttributeRef( a, this.RoslynCompilation.Assembly.ToRef() ) ) );

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.RoslynCompilation.AssemblyName ?? "<Anonymous>";

        public Compilation RoslynCompilation => this.PartialCompilation.Compilation;

        ITypeFactory ICompilation.TypeFactory => this.Factory;

        public IReadOnlyList<IManagedResource> ManagedResources => throw new NotImplementedException();

        public IDeclarationComparer InvariantComparer { get; }

        public INamespace RootNamespace => throw new NotImplementedException();

        public INamespace? GetNamespace( string ns ) => throw new NotImplementedException();

        public IEnumerable<T> GetAspectsOf<T>( IDeclaration declaration )
            where T : IAspect
            => this._aspects[declaration.ToRef()].Select( a => a.Aspect ).OfType<T>();

        // TODO: throw an exception when the caller tries to get aspects that have not been initialized yet.

        IDeclaration? IDeclaration.ContainingDeclaration => null;

        DeclarationKind IDeclaration.DeclarationKind => DeclarationKind.Compilation;

        public bool Equals( IDeclaration other ) => throw new NotImplementedException();

        ICompilation ICompilationElement.Compilation => this;

        public IEnumerable<IAttribute> GetAllAttributesOfType( INamedType type )
            => this._allMemberAttributesByTypeName[AttributeRef.GetShortName( type.Name )]
                .Select( a => a.Resolve( this ) )
                .WhereNotNull()
                .Where( a => a.Type.Equals( type ) );

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

        DeclarationRef<IDeclaration> IDeclarationImpl.ToRef() => DeclarationRef.Compilation();

        ImmutableArray<SyntaxReference> IDeclarationImpl.DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

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

        IDeclaration IDeclarationInternal.OriginalDefinition => this;
    }
}