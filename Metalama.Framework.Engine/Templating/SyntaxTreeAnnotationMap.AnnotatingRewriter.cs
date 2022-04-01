// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Templating
{
    internal sealed partial class SyntaxTreeAnnotationMap
    {
        /// <summary>
        /// A <see cref="CSharpSyntaxRewriter"/> that adds annotations.
        /// </summary>
        private class AnnotatingRewriter : CSharpSyntaxRewriter
        {
            private readonly SemanticModel? _semanticModel;
            private readonly SyntaxTreeAnnotationMap _map;
            private readonly bool _isTemplate;
            private readonly IDiagnosticAdder _diagnosticAdder;
            private HashSet<SyntaxNode>? _nodesWithErrorReports;

            public AnnotatingRewriter( SemanticModel? semanticModel, SyntaxTreeAnnotationMap map, bool isTemplate, IDiagnosticAdder diagnosticAdder )
            {
                this._semanticModel = semanticModel;
                this._map = map;
                this._isTemplate = isTemplate;
                this._diagnosticAdder = diagnosticAdder;
            }

            public bool Success { get; private set; } = true;

            public override SyntaxToken VisitToken( SyntaxToken token )
            {
                return this._map.AddLocationAnnotation( token );
            }

            public override SyntaxNode? Visit( SyntaxNode? node )
            {
                if ( node == null )
                {
                    return null;
                }

                var originalNode = node!;
                var transformedNode = base.Visit( node );

                // Don't run twice.
                if ( transformedNode.HasAnnotations( AnnotationKinds ) )
                {
                    throw new AssertionFailedException();
                }

                // Cache location.
                var annotatedNode = this._map.AddLocationAnnotation( originalNode, transformedNode );

                if ( this._isTemplate )
                {
                    // Get info from the semantic mode.
                    var semanticModel = this._semanticModel.AssertNotNull();

                    var symbolInfo = semanticModel.GetSymbolInfo( originalNode );
                    var typeInfo = semanticModel.GetTypeInfo( originalNode );
                    var declaredSymbol = semanticModel.GetDeclaredSymbol( originalNode );

                    // Cache semanticModel.GetSymbolInfo
                    if ( symbolInfo.Symbol != null )
                    {
                        // Report invalid symbols.
                        if ( symbolInfo.Symbol is IErrorTypeSymbol )
                        {
                            ReportError( TemplatingDiagnosticDescriptors.UnresolvedSymbolInTemplate, symbolInfo.Symbol );
                        }
                        else if ( this.IsPartiallyError( symbolInfo.Symbol ) )
                        {
                            ReportError( TemplatingDiagnosticDescriptors.PartiallyUnresolvedSymbolInTemplate, symbolInfo.Symbol );
                        }

                        if ( !this._map._symbolToAnnotationMap.TryGetValue( symbolInfo.Symbol, out var annotation ) )
                        {
                            annotation = new SyntaxAnnotation( _symbolAnnotationKind, this._map._symbolIdGenerator.GetId( symbolInfo.Symbol ) );
                            this._map._symbolToAnnotationMap[symbolInfo.Symbol] = annotation;
                            this._map._annotationToSymbolMap[annotation] = symbolInfo.Symbol;
                        }

                        annotatedNode = annotatedNode.WithAdditionalAnnotations( annotation );
                    }
                    else 
                    {
                        if ( node.IsKind( SyntaxKind.IdentifierName ) )
                        {
                            var identifier = (IdentifierNameSyntax) node;

                            if ( !identifier.IsVar )
                            {
                                ReportError( TemplatingDiagnosticDescriptors.UnresolvedIdentifierInTemplate, identifier.Identifier.ToString() );
                            }
                        }

                        return originalNode;
                    }

                    // Cache semanticModel.GetDeclaredSymbol
                    if ( declaredSymbol != null )
                    {
                        if ( !this._map._declaredSymbolToAnnotationMap.TryGetValue( declaredSymbol, out var annotation ) )
                        {
                            annotation = new SyntaxAnnotation( _declaredSymbolAnnotationKind, this._map._symbolIdGenerator.GetId( declaredSymbol ) );
                            this._map._declaredSymbolToAnnotationMap[declaredSymbol] = annotation;
                            this._map._annotationToDeclaredSymbolMap[annotation] = declaredSymbol;
                        }

                        annotatedNode = annotatedNode.WithAdditionalAnnotations( annotation );
                    }

                    // Cache semanticModel.GetTypeInfo.
                    if ( typeInfo.Type != null )
                    {
                        if ( !this._map._typeToAnnotationMap.TryGetValue( typeInfo.Type, out var annotation ) )
                        {
                            annotation = new SyntaxAnnotation( _expressionTypeAnnotationKind, this._map._symbolIdGenerator.GetId( typeInfo.Type ) );
                            this._map._typeToAnnotationMap[typeInfo.Type] = annotation;
                            this._map._annotationToTypeMap[annotation] = typeInfo.Type;
                        }

                        annotatedNode = annotatedNode.WithAdditionalAnnotations( annotation );
                    }
                }

                return annotatedNode;

                void ReportError<T>( DiagnosticDefinition<T> diagnostic, T arg ) 
                    where T : notnull
                {
                    this._nodesWithErrorReports ??= new HashSet<SyntaxNode>();

                    // Avoid reporting on the parent if we have reported anything on a child.
                    if ( !originalNode.ChildNodes().Any( n => this._nodesWithErrorReports.Contains( n ) ) )
                    {
                        this._diagnosticAdder.Report(
                            diagnostic.CreateRoslynDiagnostic( originalNode.GetLocation(), arg ) );

                        this.Success = false;
                        this._nodesWithErrorReports.Add( originalNode );
                    }
                }
            }

            // Determines if the type of the symbol of a parameter is unresolved.
            private bool IsPartiallyError( ISymbol symbol )
                => symbol switch
                {
                    IErrorTypeSymbol => true,
                    INamedTypeSymbol namedType => namedType.TypeArguments.Any( this.IsPartiallyError ),
                    IMethodSymbol method => this.IsPartiallyError( method.ReturnType ) || method.Parameters.Any( p => this.IsPartiallyError( p.Type ) ),
                    IPropertySymbol property => this.IsPartiallyError( property.Type ) || property.Parameters.Any( p => this.IsPartiallyError( p.Type ) ),
                    IEventSymbol @event => this.IsPartiallyError( @event.Type ),
                    IFieldSymbol field => this.IsPartiallyError( field.Type ),
                    _ => false
                };
        }
    }
}