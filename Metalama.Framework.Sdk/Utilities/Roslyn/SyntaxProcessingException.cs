// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

/// <summary>
/// An <see cref="Exception"/> bound to a specific syntax <see cref="Location"/>.
/// </summary>
internal sealed class SyntaxProcessingException : Exception
{
    internal SyntaxProcessingException( Exception innerException, SyntaxNode? node ) : base( GetMessage( node, innerException ), innerException )
    {
        this.SyntaxNode = node;
    }

    public SyntaxNode? SyntaxNode { get; }

    public static bool ShouldWrapException( Exception exception, SyntaxNode? node )
        => exception is not (SyntaxProcessingException or OperationCanceledException or TaskCanceledException)
           && node?.GetLocation().SourceTree?.FilePath != null;

    private static string GetMessage( SyntaxNode? node, Exception innerException )
    {
        string message;
        
        if ( node != null )
        {
            // Get the node text. We need to remove CR and LF otherwise it is not well parsed by MSBuild.
            var nodeText = node/*.NormalizeWhitespace()*/.ToString().Replace( "\r\n", " " ).Replace( "\n", " " ).Replace( "\n", " " );

            if ( nodeText.Length > 40 )
            {
                nodeText = nodeText.Substring( 0, 37 ) + "...";
            }

            // Get the node path.
            var nodePath = " TESTTESTTEST";

            // for ( var n = node; n != null; n = n.Parent )
            // {
            //     if ( nodePath != "" )
            //     {
            //         nodePath = "/" + nodePath;
            //     }
            //
            //     var identifier = n.GetType().GetProperty( "Identifier" )?.GetValue( n )?.ToString();
            //
            //     if ( identifier != null )
            //     {
            //         nodePath = $"{n.Kind()}[{identifier}]" + nodePath;
            //     }
            //     else
            //     {
            //         nodePath = $"{n.Kind()}" + nodePath;
            //     }
            // }

            var location = node.GetLocation();

            message =
                $"{innerException.GetType().Name} while processing the {node.Kind()} with code `{nodeText}` at '{nodePath}' in '{location.SourceTree?.FilePath}' ({FormatLinePosition( location.GetMappedLineSpan().StartLinePosition )}-{FormatLinePosition( location.GetMappedLineSpan().EndLinePosition )}): {innerException.Message}";
        }
        else
        {
            // We should never get here because the caller should call ShouldWrapException and not create an exception of our type if the method returns false.  
            message = innerException.Message;
        }

        var directory = Environment.ExpandEnvironmentVariables( @"%TEMP%\Metalama\CrashReports\SyntaxProcessingExceptions" );
        Directory.CreateDirectory( directory );
        var path = Path.Combine( directory, $"{Guid.NewGuid()}.txt" );
        File.WriteAllText( path, message + Environment.NewLine + Environment.NewLine + innerException );

        throw new InvestigationException( $"SyntaxProcessingException: {message}. The exception has been saved to '{path}'.", innerException );
        
        //return message;
    }

    private static string FormatLinePosition( in LinePosition position ) => $"{position.Line + 1},{position.Character + 1}";
}

public class InvestigationException( string message, Exception innerException ) : Exception( message, innerException ) { }