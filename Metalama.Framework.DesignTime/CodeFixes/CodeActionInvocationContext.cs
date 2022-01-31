// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.CodeFixes;

/// <summary>
/// Context information passed to the <see cref="CodeActionBaseModel.ToCodeActions"/> method.  
/// </summary>
public class CodeActionInvocationContext
{
    /// <summary>
    /// Gets a value indicating whether the current IDE supports hierarchical code items.
    /// </summary>
    internal  bool HierarchicalItemsSupported { get; } = HostProcess.Current.Product != HostProduct.Rider;

    internal  ICodeActionExecutionService Service { get; }

    internal  Document Document { get; }

    internal  ILogger Logger { get; }

    internal  string ProjectId { get; }

    internal CodeActionInvocationContext( ICodeActionExecutionService service, Document document, ILogger logger, string projectId )
    {
        this.Service = service;
        this.Document = document;
        this.Logger = logger;
        this.ProjectId = projectId;
    }
}