// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.CodeModel;

namespace Metalama.Framework.DesignTime.CodeFixes;

public sealed class CodeActionExecutionContext
{
    internal IServiceProvider ServiceProvider { get; }

    internal CompilationModel Compilation { get; }

    internal ILogger Logger { get; }

    internal ProjectKey ProjectKey { get; }

    internal bool IsComputingPreview { get; }

    internal CodeActionExecutionContext(
        IServiceProvider serviceProvider,
        CompilationModel compilation,
        ILogger logger,
        ProjectKey projectKey,
        bool isComputingPreview )
    {
        this.ServiceProvider = serviceProvider;
        this.Compilation = compilation;
        this.Logger = logger;
        this.ProjectKey = projectKey;
        this.IsComputingPreview = isComputingPreview;
    }
}