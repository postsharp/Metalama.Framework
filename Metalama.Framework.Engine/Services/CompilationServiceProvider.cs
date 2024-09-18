﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Framework.Services;

namespace Metalama.Framework.Engine.Services;

public interface ICompilationService;

public abstract class CompilationServiceProvider<T> : IProjectService
    where T : ICompilationService
{
    public ProjectServiceProvider ServiceProvider { get; }

    private readonly WeakCache<CompilationContext, T> _cache = new();

    protected CompilationServiceProvider( in ProjectServiceProvider serviceProvider )
    {
        this.ServiceProvider = serviceProvider;
    }

    public T Get( CompilationContext compilationContext )
        => this._cache.GetOrAdd(
            compilationContext,
            c => this.Create( compilationContext ) );

    protected abstract T Create( CompilationContext compilationContext );
}