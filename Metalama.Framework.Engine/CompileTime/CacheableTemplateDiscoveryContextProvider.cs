// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.CompileTime;

/// <summary>
/// This class provides a <see cref="ITemplateReflectionContext"/> that can be cached because the <see cref="Compilation"/>
/// stores only references to <see cref="PortableExecutableReference"/>, and nothing that holds a <see cref="SyntaxTree"/>.
/// </summary>
internal sealed class CacheableTemplateDiscoveryContextProvider
{
    private readonly Compilation _compilation;
    private readonly Lazy<CacheableContext> _lazyImpl;
    private readonly GlobalServiceProvider _serviceProvider;
    private readonly AttributeDeserializer _attributeDeserializer;
    private readonly ReferenceAssemblyLocator _referenceAssemblyLocator;
    private readonly CompilationContextFactory _compilationContextFactory;
    private bool _isFrozen;
    private bool _mustEnlargeVisibility;

    public CacheableTemplateDiscoveryContextProvider( Compilation compilation, ProjectServiceProvider projectServiceProvider )
    {
        this._compilation = compilation;
        this._serviceProvider = projectServiceProvider.Global;
        this._compilationContextFactory = projectServiceProvider.GetRequiredService<CompilationContextFactory>();

        // We can take the main AttributeDeserializer because all attributes are either public or visible to the main compilation. 
        this._attributeDeserializer = projectServiceProvider.GetRequiredService<CompilationContextFactory>()
            .GetInstance( compilation )
            .AttributeDeserializer;

        this._referenceAssemblyLocator = projectServiceProvider.GetReferenceAssemblyLocator();
        this._lazyImpl = new Lazy<CacheableContext>( this.CreateContext );
    }

    public void OnPortableExecutableReferenceDiscovered()
    {
        if ( this._isFrozen )
        {
            throw new AssertionFailedException();
        }

        this._mustEnlargeVisibility = true;
    }

    private CacheableContext CreateContext()
    {
        this._isFrozen = true;

        var compilation = this._mustEnlargeVisibility
            ? CSharpCompilation.Create(
                nameof(CacheableTemplateDiscoveryContextProvider),
                references: this._compilation.References.OfType<PortableExecutableReference>(),
                options: (CSharpCompilationOptions?) this._compilation.Options.WithMetadataImportOptions( MetadataImportOptions.All ) )
            : this._compilation;

        return new CacheableContext( compilation, this );
    }

    public ITemplateReflectionContext GetTemplateDiscoveryContext() => this._lazyImpl.Value;

    private sealed class CacheableContext : ITemplateReflectionContext
    {
        private readonly CacheableTemplateDiscoveryContextProvider _parent;
        private readonly Lazy<CompilationModel> _compilationModel;

        public CacheableContext( Compilation compilation, CacheableTemplateDiscoveryContextProvider parent )
        {
            this.Compilation = compilation;
            this._parent = parent;

            this._compilationModel = new Lazy<CompilationModel>(
                () => CompilationModel.CreateInitialInstance(
                    new Project( this._parent._compilationContextFactory ),
                    this.Compilation,
                    new CompilationModelOptions( ShowExternalPrivateMembers: true ) ) );
        }

        [Memo]
        public ISymbolClassifier SymbolClassifier
            => new SymbolClassifier(
                this._parent._serviceProvider,
                this.Compilation,
                this._parent._attributeDeserializer,
                this._parent._referenceAssemblyLocator );

        public Compilation Compilation { get; }

        public AttributeDeserializer AttributeDeserializer => this._parent._attributeDeserializer;

        public CompilationModel GetCompilationModel( ICompilation sourceCompilation ) => this._compilationModel.Value;
    }

    private sealed class Project : IProject
    {
        public Project( CompilationContextFactory compilationContextFactory )
        {
            this.ServiceProvider = ServiceProvider<IProjectService>.Empty.WithService( compilationContextFactory );
        }

        public string Name => "<null>";

        public string? Path => null;

        public ImmutableArray<IAssemblyIdentity> AssemblyReferences => ImmutableArray<IAssemblyIdentity>.Empty;

        public ImmutableHashSet<string> PreprocessorSymbols => ImmutableHashSet<string>.Empty;

        public string? Configuration => null;

        public string? TargetFramework => null;

        public bool TryGetProperty( string name, [NotNullWhen( true )] out string? value )
        {
            value = null;

            return false;
        }

        public T Extension<T>()
            where T : ProjectExtension, new()
            => new();

        public IServiceProvider<IProjectService> ServiceProvider { get; }
    }

    public void Freeze()
    {
        this._isFrozen = true;
    }
}