// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeFixes;

public class CodeActionInvocationContext
{
    public bool HierarchicalItemsSupported { get; } = HostProcess.Current.Product != HostProduct.Rider;

    public ICodeActionExecutionService Service { get; }

    public Document Document { get; }

    public ILogger Logger { get; }

    public CodeActionInvocationContext( ICodeActionExecutionService service, Document document, ILogger logger )
    {
        this.Service = service;
        this.Document = document;
        this.Logger = logger;
    }
}