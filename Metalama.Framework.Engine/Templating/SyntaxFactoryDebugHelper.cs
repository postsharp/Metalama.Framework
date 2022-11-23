// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Templating;

public static partial class SyntaxFactoryDebugHelper
{
    /// <summary>
    /// Generates a string that contains C# code that instantiates the given node
    /// using SyntaxFactory. Used for debugging.
    /// </summary>
    public static string ToSyntaxFactoryDebug( this SyntaxNode node, Compilation compilation, ProjectServiceProvider serviceProvider )
    {
        MetaSyntaxRewriter rewriter = new( serviceProvider, compilation, RoslynApiVersion.Current );

        try
        {
            var normalized = NormalizeRewriter.Instance.Visit( node );
            var transformedNode = rewriter.Visit( normalized )!;

            return transformedNode.ToFullString();
        }
        catch ( Exception ex )
        {
            return ex.ToString();
        }
    }
}