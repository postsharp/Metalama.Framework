// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Templating;

public static partial class SyntaxFactoryDebugHelper
{
    /// <summary>
    /// Generates a string that contains C# code that instantiates the given node
    /// using SyntaxFactory. Used for debugging.
    /// </summary>
    public static string ToSyntaxFactoryDebug( this SyntaxNode node, Compilation compilation, IServiceProvider serviceProvider )
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