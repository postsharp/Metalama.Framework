// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Caravela.Framework.Impl.Formatting
{
    internal class XmlDocumentationReader
    {
        private static readonly Regex _cleanupRegex = new Regex( "`[0-9]+" );
        private readonly Dictionary<string, XElement> _members;
        private static readonly Regex _spaceRegex = new( "\\s+", RegexOptions.Compiled );
        private static XmlDocumentationReader? _instance;

        public static XmlDocumentationReader Instance => _instance ??= new XmlDocumentationReader();

        private XmlDocumentationReader()
        {
            var documentationPath = Path.ChangeExtension( typeof(IAspect).Assembly.Location, ".xml" );

            if ( File.Exists( documentationPath ) )
            {
                var documentationDocument = XDocument.Load( documentationPath );

                this._members = documentationDocument
                    !.Root
                    !.Element( "members" )
                    !.Elements( "member" )
                    .ToDictionary( m => m.Attribute( "name" )!.Value, m => m );
            }
            else
            {
                this._members = new Dictionary<string, XElement>();
            }
        }

        public string? GetFormattedDocumentation( ISymbol symbol, Compilation compilation, string prefix = "" )
        {
            switch ( symbol )
            {
                case IMethodSymbol { IsOverride: true } methodSymbol:
                    return this.GetFormattedDocumentation( methodSymbol.OverriddenMethod!, compilation, "Overriding the " );
                
                case IMethodSymbol { ExplicitInterfaceImplementations: { Length: > 0 } } methodSymbol:
                    // TODO: Implicit implementations are not trivial.
                    return this.GetFormattedDocumentation( methodSymbol.ExplicitInterfaceImplementations.First(), compilation, "Implementing the " );
                    
                case IMethodSymbol methodSymbol:
                    symbol = methodSymbol.OriginalDefinition;
                    
                    break;

                case INamedTypeSymbol namedTypeSymbol:
                    symbol = namedTypeSymbol.ConstructedFrom;

                    break;
            }

            var documentationId = symbol.GetDocumentationCommentId();

            if ( documentationId == null || !this._members.TryGetValue( documentationId, out var documentation ) )
            {
                // If the constructor is not documented, return the documentation of the type.
                if ( symbol is IMethodSymbol { MethodKind: MethodKind.Constructor } methodSymbol )
                {
                    return this.GetFormattedDocumentation( methodSymbol.ContainingType, compilation );
                }
                
                return null;
            }

            var summary = documentation.Element( "summary" );

            if ( summary == null )
            {
                return null;
            }

            StringBuilder stringBuilder = new();

            foreach ( var node in summary.Nodes() )
            {
                switch ( node )
                {
                    case XText text:
                        stringBuilder.Append( text.Value );

                        break;

                    case XElement see when see.Name == "see":

                        var cref = see.Attribute( "cref" )!.Value;
                        var referencedSymbol = DocumentationCommentId.GetFirstSymbolForReferenceId( cref, compilation );

                        if ( referencedSymbol != null )
                        {
                            stringBuilder.Append( referencedSymbol.ToDisplayString( SymbolDisplayFormat.CSharpShortErrorMessageFormat ) );
                        }
                        else
                        {
                            // Somehow DocumentationCommentId.GetFirstSymbolForReferenceId does only work for types.
                            var indexOfParenthesis = cref.IndexOf( '(' );
                            var crefWithoutParameters = indexOfParenthesis > 0 ? cref.Substring( 0, indexOfParenthesis ) : cref;
                            var parts = crefWithoutParameters.Split( '.', ':' );

                            switch ( parts[0] )
                            {
                                case "T":
                                    stringBuilder.Append( CleanUpName( parts[parts.Length - 1] ) );

                                    break;

                                default:
                                    stringBuilder.Append( CleanUpName( parts[parts.Length - 2] ) );
                                    stringBuilder.Append( '.' );
                                    stringBuilder.Append( CleanUpName( parts[parts.Length - 1] ) );

                                    break;
                            }
                        }

                        break;

                    case XElement paramRef when paramRef.Name == "paramRef":

                        stringBuilder.Append( paramRef.Attribute( "name" ) );

                        break;

                    default:
                        stringBuilder.Append( node );

                        break;
                }
            }

            var normalized = _spaceRegex.Replace( stringBuilder.ToString(), " " ).Trim();

            var declarationKind = symbol switch
            {
                INamedTypeSymbol namedType => namedType.TypeKind switch
                {
                    TypeKind.FunctionPointer => "function pointer",
                    TypeKind.TypeParameter => "type parameter",
                    _ => namedType.TypeKind.ToString().ToLowerInvariant()
                },
                _ => symbol.GetDeclarationKind().ToDisplayString()
            };

            var prolog = $"{symbol.ToDisplayString( SymbolDisplayFormat.CSharpShortErrorMessageFormat )} {declarationKind}\n\n";

            return prefix + prolog + normalized;
        }

        private static string CleanUpName( string name ) => _cleanupRegex.Replace( name, "" );
    }
}