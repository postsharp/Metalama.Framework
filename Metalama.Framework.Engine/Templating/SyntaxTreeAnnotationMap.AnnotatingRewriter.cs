// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
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
        private sealed class AnnotatingRewriter : SafeSyntaxRewriter
        {
            private readonly SemanticModel? _semanticModel;
            private readonly SyntaxTreeAnnotationMap _map;
            private readonly bool _isTemplate;
            private readonly IDiagnosticAdder _diagnosticAdder;

            private HashSet<SyntaxNode>? _nodesWithErrorReports;

            public AnnotatingRewriter(
                SemanticModel? semanticModel,
                SyntaxTreeAnnotationMap map,
                bool isTemplate,
                IDiagnosticAdder diagnosticAdder )
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

            protected override SyntaxNode? VisitCore( SyntaxNode? node )
            {
                if ( node == null )
                {
                    return null;
                }

                var originalNode = node;
                var transformedNode = base.VisitCore( node )!;

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
                    var symbol = symbolInfo.Symbol;

                    void IndexSymbol( ISymbol s )
                    {
                        if ( !this._map._symbolToAnnotationMap.TryGetValue( s, out var annotation ) )
                        {
                            annotation = new SyntaxAnnotation( _symbolAnnotationKind );
                            this._map._symbolToAnnotationMap[s] = annotation;
                            this._map._annotationToSymbolMap[annotation] = s;
                        }

                        annotatedNode = annotatedNode.WithAdditionalAnnotations( annotation );
                    }

                    if ( symbol != null )
                    {
                        // Report invalid symbols.
                        if ( symbol is IErrorTypeSymbol )
                        {
                            // Do not report this error because it is reported by Roslyn anyway.
                        }
                        else if ( this.IsPartiallyError( symbol ) )
                        {
                            if ( node is IdentifierNameSyntax { IsVar: true } )
                            {
                                // We don't report an error on the 'var' keyword because it is reported on the right side
                                // of the assignment anyway.
                            }
                            else
                            {
                                ReportError( TemplatingDiagnosticDescriptors.PartiallyUnresolvedSymbolInTemplate, symbol );
                            }
                        }

                        IndexSymbol( symbol );
                    }
                    else if ( symbolInfo is { CandidateReason: CandidateReason.LateBound, CandidateSymbols.IsDefaultOrEmpty: false } )
                    {
                        // With dynamic code, when need to index all potential symbols.
                        foreach ( var s in symbolInfo.CandidateSymbols )
                        {
                            IndexSymbol( s );
                        }
                    }

                    // Cache semanticModel.GetDeclaredSymbol
                    if ( declaredSymbol != null )
                    {
                        if ( !this._map._declaredSymbolToAnnotationMap.TryGetValue( declaredSymbol, out var annotation ) )
                        {
                            annotation = new SyntaxAnnotation( _declaredSymbolAnnotationKind );
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
                            annotation = SymbolAnnotationMapper.GetOrCreateAnnotation(
                                SymbolAnnotationMapper.ExpressionTypeAnnotationKind,
                                typeInfo.Type );

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
                    if ( !originalNode.DescendantNodes().Any( n => this._nodesWithErrorReports.Contains( n ) ) )
                    {
                        this._diagnosticAdder.Report( diagnostic.CreateRoslynDiagnostic( originalNode.GetLocation(), arg ) );

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
                    INamedTypeSymbol { IsUnboundGenericType: false } namedType => namedType.TypeArguments.Any( this.IsPartiallyError ),
                    IMethodSymbol method => this.IsPartiallyError( method.ReturnType ) || method.Parameters.Any( p => this.IsPartiallyError( p.Type ) ),
                    IPropertySymbol property => this.IsPartiallyError( property.Type ) || property.Parameters.Any( p => this.IsPartiallyError( p.Type ) ),
                    IEventSymbol @event => this.IsPartiallyError( @event.Type ),
                    IFieldSymbol field => this.IsPartiallyError( field.Type ),
                    _ => false
                };
        }
    }
}