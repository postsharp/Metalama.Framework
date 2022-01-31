// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.CodeFixes;

public class CodeActionInvocationContext
{
    public bool HierarchicalItemsSupported { get; } = HostProcess.Current.Product != HostProduct.Rider;

    public ICodeActionExecutionService Service { get; }

    public Document Document { get; }

    public ILogger Logger { get; }

    public string ProjectId { get; }

    public CodeActionInvocationContext( ICodeActionExecutionService service, Document document, ILogger logger, string projectId )
    {
        this.Service = service;
        this.Document = document;
        this.Logger = logger;
        this.ProjectId = projectId;
    }
}