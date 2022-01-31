// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Options;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime;

public abstract class ProjectHandler : IDisposable
{
    public IServiceProvider ServiceProvider { get; }

    public IProjectOptions ProjectOptions { get; }

    protected ProjectHandler( IServiceProvider serviceProvider, IProjectOptions projectOptions )
    {
        this.ServiceProvider = serviceProvider;
        this.ProjectOptions = projectOptions;
    }

    public abstract void GenerateSources( Compilation compilation, GeneratorExecutionContext context );

    protected virtual void Dispose( bool disposing ) { }

    public void Dispose() => this.Dispose( true );
}