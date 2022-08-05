// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.CodeModel;

namespace Metalama.Framework.DesignTime.CodeFixes;

public sealed class CodeActionExecutionContext
{
    internal IServiceProvider ServiceProvider { get; }

    internal CompilationModel Compilation { get; }

    internal ILogger Logger { get; }

    internal ProjectKey ProjectKey { get; }

    internal CodeActionExecutionContext( IServiceProvider serviceProvider, CompilationModel compilation, ILogger logger, ProjectKey projectKey )
    {
        this.ServiceProvider = serviceProvider;
        this.Compilation = compilation;
        this.Logger = logger;
        this.ProjectKey = projectKey;
    }
}