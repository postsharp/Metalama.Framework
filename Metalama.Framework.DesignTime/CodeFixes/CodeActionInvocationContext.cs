// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Utilities;
using Metalama.Framework.DesignTime.Rpc;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.CodeFixes;

/// <summary>
/// Context information passed to the <see cref="CodeActionBaseModel.ToCodeActions"/> method.  
/// </summary>
public sealed class CodeActionInvocationContext
{
    /// <summary>
    /// Gets a value indicating whether the current IDE supports hierarchical code items.
    /// </summary>
    internal static bool HierarchicalItemsSupported => ProcessUtilities.ProcessKind != ProcessKind.Rider;

    internal ICodeActionExecutionService Service { get; }

    internal Document Document { get; }

    internal SyntaxNode SyntaxNode { get; }

    internal ILogger Logger { get; }

    internal ProjectKey ProjectKey { get; }

    internal CodeActionInvocationContext( ICodeActionExecutionService service, Document document, SyntaxNode syntaxNode, ILogger logger, ProjectKey projectKey )
    {
        this.Service = service;
        this.Document = document;
        this.SyntaxNode = syntaxNode;
        this.Logger = logger;
        this.ProjectKey = projectKey;
    }
}