// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using System;

namespace Metalama.Framework.Engine.CodeFixes;

public class CodeActionExecutionContext
{
    public IServiceProvider ServiceProvider { get; }

    public CompilationModel Compilation { get; }

    public ILogger Logger { get; }

    public string ProjectId { get; }

    public CodeActionExecutionContext( IServiceProvider serviceProvider, CompilationModel compilation, ILogger logger, string projectId )
    {
        this.ServiceProvider = serviceProvider;
        this.Compilation = compilation;
        this.Logger = logger;
        this.ProjectId = projectId;
    }
}