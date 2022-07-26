// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

/// <summary>
/// An <see cref="Exception"/> bound to a specific syntax <see cref="Location"/>.
/// </summary>
public sealed class SyntaxProcessingException : Exception
{
    public SyntaxProcessingException( SyntaxNode node, Exception innerException ) : base( GetMessage( node, innerException ), innerException )
    {
        this.Location = node.GetLocation();
    }

    public Location Location { get; }

    private static string GetMessage( SyntaxNode node, Exception innerException )
    {
        var location = node.GetLocation();

        return
            $"{innerException.GetType().Namespace} while processing a {node.Kind()} at '{location.SourceTree?.FilePath}', line {location.GetMappedLineSpan().StartLinePosition.Line + 1}: {innerException.Message}";
    }
}