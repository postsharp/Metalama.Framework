﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
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
    private readonly ProjectServiceProvider _serviceProvider;
    private bool _mustEnlargeVisibility;

    public CacheableTemplateDiscoveryContextProvider( Compilation compilation, ProjectServiceProvider serviceProvider )
    {
        this._compilation = compilation;
        this._serviceProvider = serviceProvider;

        this._lazyImpl = new Lazy<CacheableContext>( this.CreateContext );
    }

    public void OnPortableExecutableReferenceDiscovered()
    {
        this._mustEnlargeVisibility = true;
    }

    private CacheableContext CreateContext()
    {
        var compilation = this._mustEnlargeVisibility
            ? CSharpCompilation.Create(
                nameof(CacheableTemplateDiscoveryContextProvider),
                references: this._compilation.References.OfType<PortableExecutableReference>(),
                options: (CSharpCompilationOptions?) this._compilation.Options.WithMetadataImportOptions( MetadataImportOptions.All ) )
            : this._compilation;

        return new CacheableContext( compilation, this, this._mustEnlargeVisibility );
    }

    public ITemplateReflectionContext GetTemplateDiscoveryContext() => this._lazyImpl.Value;

    private sealed class CacheableContext : ITemplateReflectionContext
    {
        private readonly CacheableTemplateDiscoveryContextProvider _parent;
        private readonly Lazy<CompilationModel> _compilationModel;

        public CacheableContext( Compilation compilation, CacheableTemplateDiscoveryContextProvider parent, bool isCacheable )
        {
            this._parent = parent;
            this.IsCacheable = isCacheable;
            this.Compilation = compilation;

            this._compilationModel = new Lazy<CompilationModel>(
                () => CompilationModel.CreateInitialInstance(
                    new ProjectModel( compilation, parent._serviceProvider ),
                    this.Compilation,
                    new CompilationModelOptions( ShowExternalPrivateMembers: true ) ) );
        }

        public Compilation Compilation { get; }

        public CompilationModel GetCompilationModel( ICompilation sourceCompilation ) => this._compilationModel.Value;

        public bool IsCacheable { get; }

        public override string ToString() => $"CacheableContext EnlargedVisibility={this._parent._mustEnlargeVisibility}";
    }
}