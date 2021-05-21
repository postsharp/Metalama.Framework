// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CompileTime
{
    internal partial class CompileTimeCompilationBuilder
    {
        /// <summary>
        /// Rewrites a run-time syntax tree into a compile-time syntax tree. Calls <see cref="TemplateCompiler"/> on templates,
        /// and removes run-time-only sub trees.
        /// </summary>
        private sealed class ProduceCompileTimeCodeCompileTimeBaseRewriter : CompileTimeBaseRewriter
        {
            private static readonly SyntaxAnnotation _hasCompileTimeCodeAnnotation = new( "hasCompileTimeCode" );
            private readonly Compilation _compileTimeCompilation;
            private readonly IDiagnosticAdder _diagnosticAdder;
            private readonly TemplateCompiler _templateCompiler;
            private readonly CancellationToken _cancellationToken;

            public bool Success { get; private set; } = true;

            public bool FoundCompileTimeCode { get; private set; }

            public ProduceCompileTimeCodeCompileTimeBaseRewriter(
                Compilation runTimeCompilation,
                Compilation compileTimeCompilation,
                IDiagnosticAdder diagnosticAdder,
                TemplateCompiler templateCompiler,
                IServiceProvider serviceProvider,
                CancellationToken cancellationToken )
                : base( runTimeCompilation, serviceProvider )
            {
                this._compileTimeCompilation = compileTimeCompilation;
                this._diagnosticAdder = diagnosticAdder;
                this._templateCompiler = templateCompiler;
                this._cancellationToken = cancellationToken;
            }

            // TODO: assembly and module-level attributes?
            public override SyntaxNode? VisitAttributeList( AttributeListSyntax node ) => node.Parent is CompilationUnitSyntax ? null : node;

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

            public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

            public override SyntaxNode? VisitInterfaceDeclaration( InterfaceDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

            public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node ) => this.VisitTypeDeclaration( node );

            private T? VisitTypeDeclaration<T>( T node )
                where T : TypeDeclarationSyntax
            {
                this._cancellationToken.ThrowIfCancellationRequested();

                switch ( this.GetSymbolDeclarationScope( node ) )
                {
                    case SymbolDeclarationScope.RunTimeOnly:
                        return null;

                    default:
                        this.FoundCompileTimeCode = true;

                        var members = new List<MemberDeclarationSyntax>();

                        foreach ( var member in node.Members )
                        {
                            switch ( member )
                            {
                                case MethodDeclarationSyntax method:
                                    members.AddRange( this.VisitMethodDeclaration( method ).AssertNoneNull() );

                                    break;

                                case IndexerDeclarationSyntax indexer:
                                    members.AddRange( this.VisitBasePropertyDeclaration( indexer ).AssertNoneNull() );

                                    break;

                                case PropertyDeclarationSyntax property:
                                    members.AddRange( this.VisitBasePropertyDeclaration( property ).AssertNoneNull() );

                                    break;

                                case TypeDeclarationSyntax nestedType:
                                    members.Add( (MemberDeclarationSyntax) this.Visit( nestedType ).AssertNotNull() );

                                    break;

                                default:
                                    members.Add( member );

                                    break;
                            }
                        }

                        return (T) node.WithMembers( List( members ) ).WithAdditionalAnnotations( _hasCompileTimeCodeAnnotation );
                }
            }

            private new IEnumerable<MethodDeclarationSyntax> VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                var methodSymbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node );

                if ( methodSymbol != null && this.SymbolClassifier.IsTemplate( methodSymbol ) )
                {
                    var success =
                        this._templateCompiler.TryCompile(
                            TemplateNameHelper.GetCompiledTemplateName( methodSymbol ),
                            this._compileTimeCompilation,
                            node,
                            this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ),
                            this._diagnosticAdder,
                            this._cancellationToken,
                            out _,
                            out var transformedNode );

                    if ( success )
                    {
                        yield return WithThrowNotSupportedExceptionBody( node, "Template code cannot be directly executed." );
                        yield return (MethodDeclarationSyntax) transformedNode.AssertNotNull();
                    }
                    else
                    {
                        this.Success = false;
                    }
                }
                else
                {
                    yield return (MethodDeclarationSyntax) base.VisitMethodDeclaration( node ).AssertNotNull();
                }
            }

            private IEnumerable<MemberDeclarationSyntax> VisitBasePropertyDeclaration( BasePropertyDeclarationSyntax node )
            {
                var propertySymbol = (IPropertySymbol) this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node ).AssertNotNull();

                if ( propertySymbol != null && this.SymbolClassifier.IsTemplate( propertySymbol ) )
                {
                    var success = true;
                    SyntaxNode? transformedGetDeclaration = null;
                    SyntaxNode? transformedSetDeclaration = null;

                    // Compile accessors into templates.
                    if ( !propertySymbol.IsAbstract )
                    {
                        if ( node.AccessorList != null )
                        {
                            var getAccessor = node.AccessorList.Accessors.SingleOrDefault( a => a.Kind() == SyntaxKind.GetAccessorDeclaration );

                            var setAccessor = node.AccessorList.Accessors.SingleOrDefault(
                                a => a.Kind() == SyntaxKind.SetAccessorDeclaration || a.Kind() == SyntaxKind.InitAccessorDeclaration );

                            var propertyParameters = node switch
                            {
                                PropertyDeclarationSyntax property => (SeparatedSyntaxList<ParameterSyntax>?) null,
                                IndexerDeclarationSyntax indexer => indexer.ParameterList.Parameters,
                                _ => throw new AssertionFailedException()
                            };

                            // Auto properties don't have bodies and so we don't need templates.

                            if ( getAccessor != null && (getAccessor.Body != null || getAccessor.ExpressionBody != null) )
                            {
                                success = success &&
                                          this._templateCompiler.TryCompile(
                                              TemplateNameHelper.GetCompiledTemplateName( propertySymbol.GetMethod.AssertNotNull() ),
                                              this._compileTimeCompilation,
                                              getAccessor,
                                              this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ),
                                              this._diagnosticAdder,
                                              out _,
                                              out transformedGetDeclaration );
                            }

                            if ( setAccessor != null && (setAccessor.Body != null || setAccessor.ExpressionBody != null) )
                            {
                                success = success &&
                                          this._templateCompiler.TryCompile(
                                              TemplateNameHelper.GetCompiledTemplateName( propertySymbol.SetMethod.AssertNotNull() ),
                                              this._compileTimeCompilation,
                                              setAccessor,
                                              this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ),
                                              this._diagnosticAdder,
                                              out _,
                                              out transformedSetDeclaration );
                            }

                            // Expression bodied property.
                            if ( node is PropertyDeclarationSyntax { ExpressionBody: not null } propertyNode )
                            {
                                // TODO: Does this preserve trivias in expression body?
                                success = success &&
                                          this._templateCompiler.TryCompile(
                                              TemplateNameHelper.GetCompiledTemplateName( propertySymbol.SetMethod.AssertNotNull() ),
                                              this._compileTimeCompilation,
                                              AccessorDeclaration(
                                                  SyntaxKind.GetAccessorDeclaration,
                                                  List<AttributeListSyntax>(),
                                                  TokenList(),
                                                  propertyNode.ExpressionBody! ),
                                              this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ),
                                              this._diagnosticAdder,
                                              out _,
                                              out transformedGetDeclaration );
                            }
                        }
                    }

                    if ( success )
                    {
                        yield return WithThrowNotSupportedExceptionBody( node, "Template code cannot be directly executed." );

                        if ( transformedGetDeclaration != null )
                        {
                            yield return (MemberDeclarationSyntax) transformedGetDeclaration;
                        }

                        if ( transformedSetDeclaration != null )
                        {
                            yield return (MemberDeclarationSyntax) transformedSetDeclaration;
                        }
                    }
                    else
                    {
                        this.Success = false;
                    }
                }
                else
                {
                    yield return (BasePropertyDeclarationSyntax) this.Visit( node ).AssertNotNull();
                }
            }

            public override SyntaxNode? VisitNamespaceDeclaration( NamespaceDeclarationSyntax node )
            {
                var transformedNode = (NamespaceDeclarationSyntax) base.VisitNamespaceDeclaration( node )!;

                if ( transformedNode.Members.Any( m => m.HasAnnotation( _hasCompileTimeCodeAnnotation ) ) )
                {
                    return transformedNode.WithAdditionalAnnotations( _hasCompileTimeCodeAnnotation );
                }
                else
                {
                    return null;
                }
            }

            public override SyntaxNode? VisitCompilationUnit( CompilationUnitSyntax node )
            {
                var transformedNode = (CompilationUnitSyntax) base.VisitCompilationUnit( node )!;

                if ( transformedNode.Members.Any( m => m.HasAnnotation( _hasCompileTimeCodeAnnotation ) ) )
                {
                    return transformedNode.WithAdditionalAnnotations( _hasCompileTimeCodeAnnotation );
                }
                else
                {
                    return null;
                }
            }

            // TODO: top-level statements?
        }
    }
}