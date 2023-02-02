// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CompileTime;

internal class ClassifyingCompilationContextFactory : IProjectService, IDisposable
{
    private readonly ProjectServiceProvider _serviceProvider;
    private readonly WeakCache<Compilation, ClassifyingCompilationContext> _instances = new();
    private readonly CompilationContextFactory _compilationContextFactory;

    public ClassifyingCompilationContextFactory( ProjectServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
        this._compilationContextFactory = serviceProvider.GetRequiredService<CompilationContextFactory>();
    }

    public ClassifyingCompilationContext GetInstance( Compilation compilation ) => this._instances.GetOrAdd( compilation, this.GetInstanceCore );

    private ClassifyingCompilationContext GetInstanceCore( Compilation compilation )
        => new( this._serviceProvider, this._compilationContextFactory.GetInstance( compilation ) );

    public void Dispose()
    {
        this._instances.Dispose();
    }
}