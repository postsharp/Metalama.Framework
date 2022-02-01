// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Metrics;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

namespace Metalama.Framework.Engine.CodeModel
{
    public partial class CompilationModel : SymbolBasedDeclaration, ICompilationInternal
    {
        public static CompilationModel CreateInitialInstance( IProject project, PartialCompilation compilation ) => new( project, compilation );

        public static CompilationModel CreateInitialInstance(
            IProject project,
            Compilation compilation,
            ImmutableArray<ManagedResource> resources = default )
            => new( project, PartialCompilation.CreateComplete( compilation, resources ) );

        public static CompilationModel CreateInitialInstance(
            IProject project,
            Compilation compilation,
            SyntaxTree syntaxTree,
            ImmutableArray<ManagedResource> resources = default )
            => new( project, PartialCompilation.CreatePartial( compilation, syntaxTree, resources ) );

        private readonly ImmutableDictionaryOfArray<Ref<IDeclaration>, IObservableTransformation> _transformations;

        // This collection index all attributes on types and members, but not attributes on the assembly and the module.
        private readonly ImmutableDictionaryOfArray<string, AttributeRef> _allMemberAttributesByTypeName;

        private readonly ImmutableDictionaryOfArray<Ref<IDeclaration>, IAspectInstanceInternal> _aspects;

        private readonly DerivedTypeIndex _derivedTypes;

        private ImmutableDictionary<Ref<IDeclaration>, int> _depthsCache = ImmutableDictionary.Create<Ref<IDeclaration>, int>();

        public DeclarationFactory Factory { get; }

        public IProject Project { get; }

        public PartialCompilation PartialCompilation { get; }

        internal ReflectionMapper ReflectionMapper { get; }

        internal ISymbolClassifier SymbolClassifier { get; }

        public MetricManager MetricManager { get; }

        private CompilationModel( IProject project, PartialCompilation partialCompilation )
        {
            this.PartialCompilation = partialCompilation;
            this.Project = project;
            this.ReflectionMapper = project.ServiceProvider.GetRequiredService<ReflectionMapperFactory>().GetInstance( this.RoslynCompilation );
            this.InvariantComparer = new DeclarationEqualityComparer( this.ReflectionMapper, this.RoslynCompilation );
            this._derivedTypes = partialCompilation.DerivedTypes;

            this._transformations = ImmutableDictionaryOfArray<Ref<IDeclaration>, IObservableTransformation>
                .Empty
                .WithKeyComparer( DeclarationRefEqualityComparer<Ref<IDeclaration>>.Instance );

            this.Factory = new DeclarationFactory( this );

            AttributeDiscoveryVisitor attributeDiscoveryVisitor = new( this.RoslynCompilation );

            foreach ( var tree in partialCompilation.SyntaxTrees )
            {
                attributeDiscoveryVisitor.Visit( tree.Value.GetRoot() );
            }

            this._allMemberAttributesByTypeName = attributeDiscoveryVisitor.GetDiscoveredAttributes();

            this._aspects = ImmutableDictionaryOfArray<Ref<IDeclaration>, IAspectInstanceInternal>.Empty;
            this.SymbolClassifier = project.ServiceProvider.GetRequiredService<SymbolClassificationService>().GetClassifier( this.RoslynCompilation );
            this.MetricManager = project.ServiceProvider.GetService<MetricManager>() ?? new MetricManager( project.ServiceProvider );
            this.EmptyGenericMap = new GenericMap( partialCompilation.Compilation );
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
                t => t.ContainingDeclaration.ToTypedRef(),
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

            this._derivedTypes = prototype._derivedTypes.WithIntroducedInterfaces( observableTransformations.OfType<IIntroducedInterface>() );

            // TODO: Process IRemoveMember.

            // TODO: this cache may need to be smartly invalidated when we have interface introductions.

            this._allMemberAttributesByTypeName = prototype._allMemberAttributesByTypeName.AddRange( allAttributes, a => a.AttributeTypeName! );
        }

        private CompilationModel( CompilationModel prototype )
        {
            this.Project = prototype.Project;
            this.Revision = prototype.Revision + 1;

            this._derivedTypes = prototype._derivedTypes;
            this.PartialCompilation = prototype.PartialCompilation;
            this.ReflectionMapper = prototype.ReflectionMapper;
            this.InvariantComparer = prototype.InvariantComparer;
            this._transformations = prototype._transformations;
            this.Factory = new DeclarationFactory( this );
            this._depthsCache = prototype._depthsCache;
            this._allMemberAttributesByTypeName = prototype._allMemberAttributesByTypeName;
            this._aspects = prototype._aspects;
            this.SymbolClassifier = prototype.SymbolClassifier;
            this.MetricManager = prototype.MetricManager;
            this.EmptyGenericMap = prototype.EmptyGenericMap;
        }

        private CompilationModel( CompilationModel prototype, IReadOnlyList<IAspectInstanceInternal> aspectInstances ) : this( prototype )
        {
            this._aspects = this._aspects.AddRange( aspectInstances, a => a.TargetDeclaration );
        }

        internal CompilationModel WithTransformations( IReadOnlyList<IObservableTransformation> introducedDeclarations )
        {
            if ( !introducedDeclarations.Any() )
            {
                return this;
            }

            return new CompilationModel( this, introducedDeclarations );
        }

        internal CompilationModel WithAspectInstances( ImmutableArray<AspectInstance> aspectInstances )
            => aspectInstances.Length == 0 ? this : new CompilationModel( this, aspectInstances );

        public string AssemblyName => this.RoslynCompilation.AssemblyName ?? "";

        [Memo]
        public INamedTypeList Types
            => new NamedTypeList(
                this,
                this.PartialCompilation.Types
                    .Where( t => this.SymbolClassifier.GetTemplatingScope( t ) != TemplatingScope.CompileTimeOnly )
                    .Select( t => new MemberRef<INamedType>( t, this.RoslynCompilation ) ) );

        [Memo]
        public override IAttributeList Attributes
            => new AttributeList(
                this,
                this.RoslynCompilation.Assembly
                    .GetAttributes()
                    .Union( this.RoslynCompilation.SourceModule.GetAttributes() )
                    .Where( a => a.AttributeConstructor != null )
                    .Select( a => new AttributeRef( a, Ref.FromSymbol( this.RoslynCompilation.Assembly, this.RoslynCompilation ) ) ) );

        public override DeclarationKind DeclarationKind => DeclarationKind.Compilation;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.RoslynCompilation.AssemblyName ?? "<Anonymous>";

        public override CompilationModel Compilation => this;

        public Compilation RoslynCompilation => this.PartialCompilation.Compilation;

        ITypeFactory ICompilation.TypeFactory => this.Factory;

        public IReadOnlyList<IManagedResource> ManagedResources => throw new NotImplementedException();

        public IDeclarationComparer InvariantComparer { get; }

        public INamespace GlobalNamespace => this.Factory.GetNamespace( this.RoslynCompilation.Assembly.GlobalNamespace );

        public INamespace? GetNamespace( string ns )
        {
            if ( string.IsNullOrEmpty( ns ) )
            {
                return this.GlobalNamespace;
            }
            else
            {
                var namespaceCursor = this.RoslynCompilation.Assembly.GlobalNamespace;

                foreach ( var part in ns.Split( '.' ) )
                {
                    namespaceCursor = namespaceCursor.GetMembers( part ).OfType<INamespaceSymbol>().SingleOrDefault();

                    if ( namespaceCursor == null )
                    {
                        return null;
                    }
                }

                return this.Factory.GetNamespace( namespaceCursor );
            }
        }

        public IEnumerable<T> GetAspectsOf<T>( IDeclaration declaration )
            where T : IAspect
            => this._aspects[declaration.ToTypedRef()].Select( a => a.Aspect ).OfType<T>();

        public IEnumerable<INamedType> GetDerivedTypes( INamedType baseType, bool deep )
            => this._derivedTypes.GetDerivedTypes( baseType.GetSymbol(), deep ).Select( t => this.Factory.GetNamedType( t ) );

        public IEnumerable<INamedType> GetDerivedTypes( Type baseType, bool deep )
            => this.GetDerivedTypes( (INamedType) this.Factory.GetTypeByReflectionType( baseType ), deep );

        public int Revision { get; }

        // TODO: throw an exception when the caller tries to get aspects that have not been initialized yet.

        public override DeclarationOrigin Origin => DeclarationOrigin.Source;

        IDeclaration? IDeclaration.ContainingDeclaration => null;

        DeclarationKind IDeclaration.DeclarationKind => DeclarationKind.Compilation;

        public bool Equals( IDeclaration other ) => ReferenceEquals( this, other );

        ICompilation ICompilationElement.Compilation => this;

        public IEnumerable<IAttribute> GetAllAttributesOfType( INamedType type )
            => this._allMemberAttributesByTypeName[AttributeHelper.GetShortName( type.Name )]
                .Select(
                    a =>
                    {
                        a.TryGetTarget( this, out var target );

                        return target;
                    } )
                .WhereNotNull()
                .Where( a => a.Type.Equals( type ) );

        internal ImmutableArray<IObservableTransformation> GetObservableTransformationsOnElement( IDeclaration declaration )
            => this._transformations[declaration.ToTypedRef()];

        internal IEnumerable<(IDeclaration DeclaringDeclaration, ImmutableArray<IObservableTransformation> Transformations)> GetAllObservableTransformations(
            bool designTimeOnly )
        {
            foreach ( var group in this._transformations )
            {
                var filteredGroup = designTimeOnly
                    ? group.Where( t => t.IsDesignTime ).ToImmutableArray()
                    : group.ToImmutableArray();

                if ( !filteredGroup.IsEmpty )
                {
                    yield return (group.Key.GetTarget( this ), filteredGroup);
                }
            }
        }

        internal int GetDepth( IDeclaration declaration )
        {
            var reference = declaration.ToTypedRef();

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

                case INamespace { IsGlobalNamespace: true }:
                    // We want the global namespace to be processed after all assembly references
                    return 2;

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
            var reference = namedType.ToTypedRef<IDeclaration>();

            if ( this._depthsCache.TryGetValue( reference, out var depth ) )
            {
                return depth;
            }

            depth = this.GetDepth( namedType.Namespace );

            if ( namedType.BaseType is { IsExternal: false } baseType )
            {
                depth = Math.Max( depth, this.GetDepth( baseType ) );
            }

            foreach ( var interfaceImplementation in namedType.ImplementedInterfaces )
            {
                if ( !interfaceImplementation.IsExternal )
                {
                    depth = Math.Max( depth, this.GetDepth( interfaceImplementation ) );
                }
            }

            depth++;

            this._depthsCache = this._depthsCache.SetItem( reference, depth );

            return depth;
        }

        internal override Ref<IDeclaration> ToRef() => Ref.Compilation( this.RoslynCompilation ).As<IDeclaration>();

        public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => Enumerable.Empty<IDeclaration>();

        string IDisplayable.ToDisplayString( CodeDisplayFormat? format, CodeDisplayContext? context ) => this.RoslynCompilation.AssemblyName ?? "";

        [Memo]
        public override IAssembly DeclaringAssembly => this.Factory.GetAssembly( this.RoslynCompilation.Assembly );

        DeclarationOrigin IDeclaration.Origin => DeclarationOrigin.Source;

        public override ISymbol Symbol => this.RoslynCompilation.Assembly;

        IAttributeList IDeclaration.Attributes => throw new NotSupportedException();

        public string? Name => this.RoslynCompilation.AssemblyName;

        public override string ToString() => $"{this.RoslynCompilation.AssemblyName}";

        internal ICompilationHelpers Helpers { get; } = new CompilationHelpers();

        ICompilationHelpers ICompilationInternal.Helpers => this.Helpers;

        public override IDeclaration OriginalDefinition => this;

        internal GenericMap EmptyGenericMap { get; }

        public bool ContainsType( INamedTypeSymbol type )
        {
            if ( this.PartialCompilation.IsPartial && !this.PartialCompilation.Types.Contains( type ) )
            {
                return false;
            }

            return this.SymbolClassifier.GetTemplatingScope( type ) != TemplatingScope.CompileTimeOnly;
        }

        bool IAssembly.IsExternal => false;

        [Memo]
        public IAssemblyIdentity Identity => new AssemblyIdentityModel( this.RoslynCompilation.Assembly.Identity );

        public override Location? DiagnosticLocation => null;

        public override bool CanBeInherited => false;
    }
}