// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.CodeModel.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Metrics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Project;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

namespace Metalama.Framework.Engine.CodeModel
{
    public sealed partial class CompilationModel : SymbolBasedDeclaration, ICompilationInternal
    {
        static CompilationModel()
        {
            MetalamaEngineModuleInitializer.EnsureInitialized();
        }

        public static CompilationModel CreateInitialInstance(
            ProjectModel project,
            PartialCompilation compilation,
            AspectRepository? aspectRepository = null,
            string? debugLabel = null )
            => new( project, compilation, aspectRepository, CompilationModelOptions.Default, debugLabel );

        public static CompilationModel CreateInitialInstance(
            ProjectModel project,
            Compilation compilation,
            ImmutableArray<ManagedResource> resources = default,
            AspectRepository? aspectRepository = null,
            string? debugLabel = null )
            => new( project, PartialCompilation.CreateComplete( compilation, resources ), aspectRepository, CompilationModelOptions.Default, debugLabel );

        internal static CompilationModel CreateInitialInstance(
            ProjectModel project,
            Compilation compilation,
            CompilationModelOptions options,
            string? debugLabel )
            => new( project, PartialCompilation.CreateComplete( compilation ), null, options: options, debugLabel: debugLabel );

        // This collection index all attributes on types and members, but not attributes on the assembly and the module.
        private readonly ImmutableDictionaryOfArray<string, AttributeRef> _allMemberAttributesByTypeName;

        internal AspectRepository AspectRepository { get; }

        private readonly DerivedTypeIndex _derivedTypes;

        private ImmutableDictionary<Ref<IDeclaration>, Ref<IDeclaration>> _redirections =
            ImmutableDictionary.Create<Ref<IDeclaration>, Ref<IDeclaration>>();

        private ImmutableDictionary<Ref<IDeclaration>, int> _depthsCache = ImmutableDictionary.Create<Ref<IDeclaration>, int>();

        public DeclarationFactory Factory { get; }

        IAspectRepository ICompilationInternal.AspectRepository => this.AspectRepository;

        internal ProjectModel Project { get; }

        IProject ICompilation.Project => this.Project;

        internal CompilationContext CompilationContext { get; }

        public PartialCompilation PartialCompilation { get; }

        internal MetricManager MetricManager { get; }

        internal CompilationModelOptions Options { get; }

        private readonly string? _debugLabel;

        private CompilationModel(
            ProjectModel project,
            PartialCompilation partialCompilation,
            AspectRepository? aspectRepository,
            CompilationModelOptions? options,
            string? debugLabel )
        {
            this.PartialCompilation = partialCompilation;
            this.Project = project;
            this._debugLabel = debugLabel;

            this.CompilationContext = CompilationContextFactory.GetInstance( partialCompilation.Compilation );

            this._staticConstructors =
                ImmutableDictionary<INamedTypeSymbol, IConstructorBuilder>.Empty.WithComparers( this.CompilationContext.SymbolComparer );

            this._finalizers = ImmutableDictionary<INamedTypeSymbol, IMethodBuilder>.Empty.WithComparers( this.CompilationContext.SymbolComparer );

            this._derivedTypes = partialCompilation.DerivedTypes;
            this.AspectRepository = aspectRepository ?? new IncrementalAspectRepository();

            // If the MetricManager is not provided, we create an instance. This allows to test metrics independently from the pipeline.
            this.MetricManager = project.ServiceProvider.GetService<MetricManager>()
                                 ?? new MetricManager( (ServiceProvider<IProjectService>) project.ServiceProvider );

            this.EmptyGenericMap = new GenericMap( partialCompilation.Compilation );
            this.Helpers = new CompilationHelpers();
            this.Options = options ?? CompilationModelOptions.Default;

            // Initialize dictionaries of modified members.
            void InitializeDictionary<T>( out ImmutableDictionary<INamedTypeSymbol, T> dictionary )
                => dictionary = ImmutableDictionary.Create<INamedTypeSymbol, T>()
                    .WithComparers( this.CompilationContext.SymbolComparer );

            InitializeDictionary( out this._fields );
            InitializeDictionary( out this._methods );
            InitializeDictionary( out this._constructors );
            InitializeDictionary( out this._events );
            InitializeDictionary( out this._properties );
            InitializeDictionary( out this._indexers );
            InitializeDictionary( out this._allInterfaceImplementations );
            InitializeDictionary( out this._interfaceImplementations );

            this._parameters = ImmutableDictionary.Create<Ref<IHasParameters>, ParameterUpdatableCollection>()
                .WithComparers( RefEqualityComparer<IHasParameters>.Default );

            this._attributes =
                ImmutableDictionary<Ref<IDeclaration>, AttributeUpdatableCollection>.Empty.WithComparers( RefEqualityComparer<IDeclaration>.Default );

            this.Factory = new DeclarationFactory( this );

            // Discover custom attributes.
            AttributeDiscoveryVisitor attributeDiscoveryVisitor = new( this.CompilationContext );

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
        private CompilationModel(
            CompilationModel prototype,
            IReadOnlyCollection<ITransformation>? observableTransformations,
            IEnumerable<AspectInstance>? aspectInstances,
            string? debugLabel ) : this(
            prototype,
            true,
            debugLabel )
        {
            if ( observableTransformations != null )
            {
                foreach ( var transformation in observableTransformations )
                {
                    this.AddTransformation( transformation );
                }

                this.IsMutable = false;

                // TODO: Performance. The next line essentially instantiates the complete code model. We should look at attributes without doing that. 
                var allNewDeclarations =
                    observableTransformations
                        .OfType<IIntroduceDeclarationTransformation>()
                        .SelectMany( t => t.DeclarationBuilder.GetContainedDeclarations() );

                // TODO: Performance. The next line essentially instantiates the complete code model. We should look at attributes without doing that. 
                var allAttributes =
                    allNewDeclarations.SelectMany( c => c.Attributes )
                        .Cast<AttributeBuilder>()
                        .Concat( observableTransformations.OfType<IntroduceAttributeTransformation>().Select( x => x.AttributeBuilder ) )
                        .Select( a => new AttributeRef( a ) );

                this._derivedTypes = prototype._derivedTypes.WithIntroducedInterfaces( observableTransformations.OfType<IIntroduceInterfaceTransformation>() );

                // TODO: this cache may need to be smartly invalidated when we have interface introductions.
                this._allMemberAttributesByTypeName = prototype._allMemberAttributesByTypeName.AddRange( allAttributes, a => a.AttributeTypeName! );
            }

            if ( aspectInstances != null )
            {
                this.AspectRepository = this.AspectRepository.WithAspectInstances( aspectInstances );
            }
        }

        private CompilationModel( CompilationModel prototype, bool mutable, string? debugLabel, CompilationModelOptions? options = null )
        {
            this.IsMutable = mutable;
            this.Project = prototype.Project;
            this.Helpers = prototype.Helpers;
            this.Options = options ?? prototype.Options;
            this._debugLabel = debugLabel;

            this._derivedTypes = prototype._derivedTypes;
            this.PartialCompilation = prototype.PartialCompilation;
            this.CompilationContext = prototype.CompilationContext;
            this._methods = prototype._methods;
            this._constructors = prototype._constructors;
            this._fields = prototype._fields;
            this._properties = prototype._properties;
            this._indexers = prototype._indexers;
            this._events = prototype._events;
            this._interfaceImplementations = prototype._interfaceImplementations;
            this._allInterfaceImplementations = prototype._allInterfaceImplementations;
            this._staticConstructors = prototype._staticConstructors;
            this._finalizers = prototype._finalizers;
            this._parameters = prototype._parameters;
            this._attributes = prototype._attributes;

            this.Factory = new DeclarationFactory( this );
            this._depthsCache = prototype._depthsCache;
            this._redirections = prototype._redirections;
            this._allMemberAttributesByTypeName = prototype._allMemberAttributesByTypeName;
            this.AspectRepository = prototype.AspectRepository;
            this.MetricManager = prototype.MetricManager;
            this.EmptyGenericMap = prototype.EmptyGenericMap;
        }

        private CompilationModel( CompilationModel prototype, AspectRepository aspectRepository, string? debugLabel ) : this( prototype, false, debugLabel )
        {
            this.AspectRepository = aspectRepository;
        }

        internal CompilationModel WithTransformationsAndAspectInstances(
            IReadOnlyCollection<ITransformation>? introducedDeclarations,
            IEnumerable<AspectInstance>? aspectInstances,
            string? debugLabel )
        {
            if ( introducedDeclarations?.Count == 0 && aspectInstances == null )
            {
                return this;
            }

            return new CompilationModel( this, introducedDeclarations, aspectInstances, debugLabel );
        }

        internal CompilationModel WithAspectRepository( AspectRepository aspectRepository, string? debugLabel )
            => this.AspectRepository == aspectRepository ? this : new CompilationModel( this, aspectRepository, debugLabel );

        [Memo]
        public INamedTypeCollection Types
            => new NamedTypeCollection(
                this,
                new CompilationTypeUpdatableCollection( this, this.RoslynCompilation.SourceModule.GlobalNamespace, false ) );

        public INamedTypeCollection AllTypes
            => new NamedTypeCollection(
                this,
                new CompilationTypeUpdatableCollection( this, this.RoslynCompilation.SourceModule.GlobalNamespace, true ) );

        [Memo]
        public override IAttributeCollection Attributes
            => new AttributeCollection(
                this,
                this.GetAttributeCollection( Ref.Compilation( this.CompilationContext ).As<IDeclaration>() ) );

        public override DeclarationKind DeclarationKind => DeclarationKind.Compilation;

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.RoslynCompilation.AssemblyName ?? "<Anonymous>";

#pragma warning disable CS0809
        [Obsolete( "This method call is redundant." )]
        public override CompilationModel Compilation => this;
#pragma warning restore CS0809

        public Compilation RoslynCompilation => this.PartialCompilation.Compilation;

        IDeclarationFactory ICompilationInternal.Factory => this.Factory;

        public IReadOnlyList<IManagedResource> ManagedResources => throw new NotImplementedException();

        public ICompilationComparers Comparers => this.CompilationContext.Comparers;

        public INamespace GlobalNamespace => this.Factory.GetNamespace( this.RoslynCompilation.SourceModule.GlobalNamespace );

        public IEnumerable<INamedType> GetDerivedTypes( INamedType baseType, DerivedTypesOptions options = default )
        {
            OnUnsupportedDependency( $"{nameof(ICompilation)}.{nameof(this.GetDerivedTypes)}" );

            return this._derivedTypes.GetDerivedTypesInCurrentCompilation( baseType.GetSymbol(), options ).Select( t => this.Factory.GetNamedType( t ) );
        }

        public IEnumerable<INamedType> GetDerivedTypes( Type baseType, DerivedTypesOptions options = default )
            => this.GetDerivedTypes( (INamedType) this.Factory.GetTypeByReflectionType( baseType ), options );

        // TODO: throw an exception when the caller tries to get aspects that have not been initialized yet.

        public override IDeclarationOrigin Origin => DeclarationOrigin.Source;

        public override IDeclaration? ContainingDeclaration => null;

        DeclarationKind IDeclaration.DeclarationKind => DeclarationKind.Compilation;

        public override bool Equals( IDeclaration? other ) => ReferenceEquals( this, other );

        ICompilation ICompilationElement.Compilation => this;

        internal IEnumerable<IAttribute> GetAllAttributesOfType( INamedType type )
            => this._allMemberAttributesByTypeName[AttributeHelper.GetShortName( type.Name )]
                .Select(
                    a =>
                    {
                        a.TryGetTarget( this, out var target );

                        return target;
                    } )
                .WhereNotNull()
                .Where( a => a.Type.Equals( (IType) type ) );

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

                case INamespace { DeclaringAssembly.IsExternal: true } ns:
                    throw new InvalidOperationException( $"Cannot compute the depth of '{ns.FullName}' because it is an external namespace." );

                case INamespace { IsGlobalNamespace: true }:
                    // We want the global namespace to be processed after all assembly references.
                    return 2;

                case INamespace ns:
                    {
                        // Then, we just count the number of components in the namespace, and we add the depth of the global namespace.
                        var depth = 3;

                        foreach ( var c in ns.FullName )
                        {
                            if ( c == '.' )
                            {
                                depth++;
                            }
                        }

                        return depth;
                    }

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
            if ( namedType.DeclaringAssembly.IsExternal )
            {
                return 0;
            }

            var reference = namedType.ToTypedRef<IDeclaration>();

            if ( this._depthsCache.TryGetValue( reference, out var depth ) )
            {
                return depth;
            }

            depth = this.GetDepth( namedType.Namespace );

            if ( namedType.BaseType is { DeclaringAssembly.IsExternal: false } baseType )
            {
                depth = Math.Max( depth, this.GetDepth( baseType ) );
            }

            foreach ( var interfaceImplementation in namedType.ImplementedInterfaces )
            {
                if ( !interfaceImplementation.DeclaringAssembly.IsExternal )
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
                if ( this._redirections.TryGetValue( reference, out var target ) )
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

        internal override Ref<IDeclaration> ToRef() => Ref.Compilation( this.CompilationContext ).As<IDeclaration>();

        public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

        public override IEnumerable<IDeclaration> GetDerivedDeclarations( DerivedTypesOptions options = default ) => Enumerable.Empty<IDeclaration>();

        string IDisplayable.ToDisplayString( CodeDisplayFormat? format, CodeDisplayContext? context ) => this.RoslynCompilation.AssemblyName ?? "";

        [Memo]
        public override IAssembly DeclaringAssembly => this.Factory.GetAssembly( this.RoslynCompilation.Assembly );

        public override ISymbol Symbol => this.RoslynCompilation.Assembly;

        internal string? Name => this.RoslynCompilation.AssemblyName;

        public override string ToString()
        {
            if ( this._debugLabel == null )
            {
                return this.RoslynCompilation.AssemblyName ?? "<anonymous>";
            }
            else
            {
                return $"{this.RoslynCompilation.AssemblyName} ({this._debugLabel})";
            }
        }

        private CompilationHelpers Helpers { get; }

        ICompilationHelpers ICompilationInternal.Helpers => this.Helpers;

        public override IDeclaration OriginalDefinition => this;

        internal GenericMap EmptyGenericMap { get; }

        bool IAssembly.IsExternal => false;

        [Memo]
        public IAssemblyIdentity Identity => new AssemblyIdentityModel( this.RoslynCompilation.Assembly.Identity );

        public override Location? DiagnosticLocation => null;

        public override bool CanBeInherited => false;

        public override SyntaxTree? PrimarySyntaxTree => null;

        public bool IsPartial => this.PartialCompilation.IsPartial;

        internal CompilationModel CreateMutableClone( string? debugLabel = null ) => new( this, true, debugLabel, this.Options );

        internal CompilationModel CreateImmutableClone( string? debugLabel = null ) => new( this, false, debugLabel, this.Options );

        public bool AreInternalsVisibleFrom( IAssembly assembly )
            => this.RoslynCompilation.Assembly.AreInternalsVisibleToImpl( (IAssemblySymbol) assembly.GetSymbol().AssertNotNull() );

        [Memo]
        public IAssemblyCollection ReferencedAssemblies => new ReferencedAssemblyCollection( this, this.RoslynCompilation.SourceModule );
    }
}