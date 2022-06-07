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
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
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

        // This collection index all attributes on types and members, but not attributes on the assembly and the module.
        private readonly ImmutableDictionaryOfArray<string, AttributeRef> _allMemberAttributesByTypeName;

        private readonly ImmutableDictionaryOfArray<Ref<IDeclaration>, IAspectInstanceInternal> _aspects;

        private readonly DerivedTypeIndex _derivedTypes;

        private ImmutableDictionary<Ref<IDeclaration>, Ref<IDeclaration>> _redirectionCache =
            ImmutableDictionary.Create<Ref<IDeclaration>, Ref<IDeclaration>>();

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
            this._aspects = ImmutableDictionaryOfArray<Ref<IDeclaration>, IAspectInstanceInternal>.Empty;
            this.SymbolClassifier = project.ServiceProvider.GetRequiredService<SymbolClassificationService>().GetClassifier( this.RoslynCompilation );
            this.MetricManager = project.ServiceProvider.GetService<MetricManager>() ?? new MetricManager( project.ServiceProvider );
            this.EmptyGenericMap = new GenericMap( partialCompilation.Compilation );

            // Initialize dictionaries of modified members.
            static void InitializeDictionary<T>( out ImmutableDictionary<INamedTypeSymbol, T> dictionary )
                => dictionary = ImmutableDictionary.Create<INamedTypeSymbol, T>()
                    .WithComparers( SymbolEqualityComparer.Default );

            InitializeDictionary( out this._fields );
            InitializeDictionary( out this._methods );
            InitializeDictionary( out this._constructors );
            InitializeDictionary( out this._events );
            InitializeDictionary( out this._properties );
            InitializeDictionary( out this._indexers );
            InitializeDictionary( out this._interfaceImplementations );

            this._parameters = ImmutableDictionary.Create<Ref<IHasParameters>, ParameterUpdatableCollection>()
                .WithComparers( DeclarationRefEqualityComparer<Ref<IHasParameters>>.Default );

            this._attributes =
                ImmutableDictionary<Ref<IDeclaration>, AttributeUpdatableCollection>.Empty.WithComparers(
                    DeclarationRefEqualityComparer<Ref<IDeclaration>>.Default );

            this.Factory = new DeclarationFactory( this );

            // Discover custom attributes.
            AttributeDiscoveryVisitor attributeDiscoveryVisitor = new( this.RoslynCompilation );

            foreach ( var tree in partialCompilation.SyntaxTrees )
            {
                attributeDiscoveryVisitor.Visit( tree.Value );
            }

            this._allMemberAttributesByTypeName = attributeDiscoveryVisitor.GetDiscoveredAttributes();
        }

        // The following dictionaries contain the members of types, if they have been modified. If they have not been modified,
        // the collection should be created from symbols.

        /// <summary>
        /// Initializes a new instance of the <see cref="CompilationModel"/> class that is based on a prototype instance but appends transformations.
        /// </summary>
        /// <param name="prototype"></param>
        /// <param name="observableTransformations"></param>
        private CompilationModel( CompilationModel prototype, IReadOnlyList<IObservableTransformation> observableTransformations ) : this( prototype, true )
        {
            foreach ( var transformation in observableTransformations )
            {
                this.AddTransformation( prototype, transformation );
            }

            this.IsMutable = false;

            // TODO: Performance. The next line essentially instantiates the complete code model. We should look at attributes without doing that. 
            var allNewDeclarations =
                observableTransformations
                    .OfType<IDeclaration>()
                    .SelectMany( declaration => declaration.GetContainedDeclarations() );

            // TODO: Performance. The next line essentially instantiates the complete code model. We should look at attributes without doing that. 
            var allAttributes =
                allNewDeclarations.SelectMany( c => c.Attributes )
                    .Cast<AttributeBuilder>()
                    .Concat( observableTransformations.OfType<AttributeBuilder>() )
                    .Select( a => new AttributeRef( a ) );

            this._derivedTypes = prototype._derivedTypes.WithIntroducedInterfaces( observableTransformations.OfType<IIntroduceInterfaceTransformation>() );

            // TODO: this cache may need to be smartly invalidated when we have interface introductions.
            this._allMemberAttributesByTypeName = prototype._allMemberAttributesByTypeName.AddRange( allAttributes, a => a.AttributeTypeName! );
        }

        private CompilationModel( CompilationModel prototype, bool mutable )
        {
            this.IsMutable = mutable;
            this.Project = prototype.Project;
            this.Revision = prototype.Revision + 1;

            this._derivedTypes = prototype._derivedTypes;
            this.PartialCompilation = prototype.PartialCompilation;
            this.ReflectionMapper = prototype.ReflectionMapper;
            this.InvariantComparer = prototype.InvariantComparer;
            this._methods = prototype._methods;
            this._constructors = prototype._constructors;
            this._fields = prototype._fields;
            this._properties = prototype._properties;
            this._indexers = prototype._indexers;
            this._events = prototype._events;
            this._interfaceImplementations = prototype._interfaceImplementations;
            this._staticConstructors = prototype._staticConstructors;
            this._parameters = prototype._parameters;
            this._attributes = prototype._attributes;

            this.Factory = new DeclarationFactory( this );
            this._depthsCache = prototype._depthsCache;
            this._redirectionCache = prototype._redirectionCache;
            this._allMemberAttributesByTypeName = prototype._allMemberAttributesByTypeName;
            this._aspects = prototype._aspects;
            this.SymbolClassifier = prototype.SymbolClassifier;
            this.MetricManager = prototype.MetricManager;
            this.EmptyGenericMap = prototype.EmptyGenericMap;
        }

        private CompilationModel( CompilationModel prototype, IReadOnlyList<IAspectInstanceInternal> aspectInstances ) : this( prototype, false )
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
        public INamedTypeCollection Types
            => new NamedTypeCollection(
                this,
                new CompilationTypeUpdatableCollection( this, this.RoslynCompilation.GlobalNamespace ) );

        [Memo]
        public override IAttributeCollection Attributes
            => new AttributeCollection(
                this,
                this.RoslynCompilation.Assembly
                    .GetAttributes()
                    .Union( this.RoslynCompilation.SourceModule.GetAttributes() )
                    .Where( a => a.AttributeConstructor != null )
                    .Select( a => new AttributeRef( a, Ref.FromSymbol( this.RoslynCompilation.Assembly, this.RoslynCompilation ) ) )
                    .ToList() );

        public override DeclarationKind DeclarationKind => DeclarationKind.Compilation;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.RoslynCompilation.AssemblyName ?? "<Anonymous>";

        public override CompilationModel Compilation => this;

        public Compilation RoslynCompilation => this.PartialCompilation.Compilation;

        IDeclarationFactory ICompilationInternal.Factory => this.Factory;

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

        IDeclaration ICompilation.GetDeclarationFromId( DeclarationSerializableId declarationId )
            => this.Factory.GetDeclarationFromSerializableId( declarationId );

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

        internal bool TryGetRedirectedDeclaration( Ref<IDeclaration> reference, out Ref<IDeclaration> redirected )
        {
            var result = false;

            while ( true )
            {
                if ( this._redirectionCache.TryGetValue( reference, out var target ) )
                {
                    result = true;
                    reference = target;
                }
                else
                {
                    redirected = reference;

                    return result;
                }
            }
        }

        internal override Ref<IDeclaration> ToRef() => Ref.Compilation( this.RoslynCompilation ).As<IDeclaration>();

        public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( bool deep = true ) => Enumerable.Empty<IDeclaration>();

        string IDisplayable.ToDisplayString( CodeDisplayFormat? format, CodeDisplayContext? context ) => this.RoslynCompilation.AssemblyName ?? "";

        [Memo]
        public override IAssembly DeclaringAssembly => this.Factory.GetAssembly( this.RoslynCompilation.Assembly );

        DeclarationOrigin IDeclaration.Origin => DeclarationOrigin.Source;

        public override ISymbol Symbol => this.RoslynCompilation.Assembly;

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

        public override SyntaxTree? PrimarySyntaxTree => null;

        public CompilationModel ToMutable() => new( this, true );
    }
}