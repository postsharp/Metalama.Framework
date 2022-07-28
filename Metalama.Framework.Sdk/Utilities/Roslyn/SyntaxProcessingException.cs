// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

/// <summary>
/// An <see cref="Exception"/> bound to a specific syntax <see cref="Location"/>.
/// </summary>
public sealed class SyntaxProcessingException : Exception
{
    public SyntaxProcessingException( Exception innerException, SyntaxNode? node ) : base( GetMessage( node, innerException ), innerException )
    {
        this.SyntaxNode = node;
    }

    public Location? Location => this.SyntaxNode?.GetLocation();

    public SyntaxNode? SyntaxNode { get; }

    public static bool ShouldWrapException( Exception exception, SyntaxNode? node )
        => exception is not SyntaxProcessingException && node != null && node.GetLocation().SourceTree?.FilePath != null;

    private static string GetMessage( SyntaxNode? node, Exception innerException )
    {
        if ( node != null )
        {
            // Get the node text.
            var nodeText = node.NormalizeWhitespace().ToString();

            if ( nodeText.Length > 40 )
            {
                nodeText = nodeText.Substring( 0, 37 ) + "...";
            }

            // Get the node path.
            var nodePath = "";

            for ( var n = node; n != null; n = n.Parent )
            {
                if ( nodePath != "" )
                {
                    nodePath = "/" + nodePath;
                }

                var identifier = n.GetType().GetProperty( "Identifier" )?.GetValue( n )?.ToString();

                if ( identifier != null )
                {
                    nodePath = $"{n.Kind()}[{identifier}]" + nodePath;
                }
                else
                {
                    nodePath = $"{n.Kind()}" + nodePath;
                }
            }

            var location = node.GetLocation();

            return
                $"{innerException.GetType().Name} while processing the {node.Kind()} with code `{nodeText}` at '{nodePath}' in '{location.SourceTree?.FilePath}' ({FormatLinePosition( location.GetMappedLineSpan().StartLinePosition )}-{FormatLinePosition( location.GetMappedLineSpan().EndLinePosition )}): {innerException.Message}";
        }
        else
        {
            // We should never get here because the caller should call ShouldWrapException and not create an exception of our type if the method returns false.  
            return innerException.Message;
        }
    }

    private static string FormatLinePosition( in LinePosition position ) => $"{position.Line + 1},{position.Character + 1}";
}