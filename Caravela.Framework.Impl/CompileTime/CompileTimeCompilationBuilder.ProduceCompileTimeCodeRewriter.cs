﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.ReflectionMocks;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using TypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Caravela.Framework.Impl.CompileTime
{
    internal partial class CompileTimeCompilationBuilder
    {
#pragma warning disable CA1001 // Class must be disposable.

        /// <summary>
        /// Rewrites a run-time syntax tree into a compile-time syntax tree. Calls <see cref="TemplateCompiler"/> on templates,
        /// and removes run-time-only sub trees.
        /// </summary>
        private sealed class ProduceCompileTimeCodeRewriter : CompileTimeBaseRewriter
        {
            private static readonly SyntaxAnnotation _hasCompileTimeCodeAnnotation = new( "hasCompileTimeCode" );
            private readonly Compilation _compileTimeCompilation;
            private readonly IDiagnosticAdder _diagnosticAdder;
            private readonly TemplateCompiler _templateCompiler;
            private readonly CancellationToken _cancellationToken;

            private Context _currentContext;

            public bool Success { get; private set; } = true;

            public bool FoundCompileTimeCode { get; private set; }

            public ProduceCompileTimeCodeRewriter(
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
                this._currentContext = new Context( SymbolDeclarationScope.Both, this );
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

                var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node )!;

                var scope = this.SymbolClassifier.GetSymbolDeclarationScope( symbol );

                if ( scope == SymbolDeclarationScope.RunTimeOnly )
                {
                    return null;
                }
                else
                {
                    this.FoundCompileTimeCode = true;

                    // Add type members.

                    var members = new List<MemberDeclarationSyntax>();

                    using ( this.WithScope( scope ) )
                    {
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

                                default:
                                    members.Add( (MemberDeclarationSyntax) this.Visit( member ).AssertNotNull() );

                                    break;
                            }
                        }
                    }

                    // Add non-implemented members of IAspect and IEligible.
                    SyntaxGenerator syntaxGenerator = LanguageServiceFactory.CSharpSyntaxGenerator;
                    var allImplementedInterfaces = symbol.SelectManyRecursive( i => i.Interfaces );

                    foreach ( var implementedInterface in allImplementedInterfaces )
                    {
                        if ( implementedInterface.Name == nameof(IAspect) || implementedInterface.Name == nameof(IEligible<IDeclaration>) )
                        {
                            foreach ( var member in implementedInterface.GetMembers() )
                            {
                                if ( member is not IMethodSymbol method )
                                {
                                    continue;
                                }

                                var memberImplementation = (IMethodSymbol?) symbol.FindImplementationForInterfaceMember( member );

                                if ( memberImplementation == null || memberImplementation.ContainingType.TypeKind == TypeKind.Interface )
                                {
                                    var newMethod = MethodDeclaration(
                                            default,
                                            default,
                                            (TypeSyntax) syntaxGenerator.TypeExpression( method.ReturnType ),
                                            ExplicitInterfaceSpecifier( (NameSyntax) syntaxGenerator.TypeExpression( implementedInterface ) ),
                                            Identifier( method.Name ),
                                            default,
                                            ParameterList(
                                                SeparatedList(
                                                    method.Parameters.Select(
                                                        p => Parameter(
                                                            default,
                                                            default,
                                                            (TypeSyntax) syntaxGenerator.TypeExpression( p.Type ),
                                                            Identifier( p.Name ),
                                                            default ) ) ) ),
                                            default,
                                            Block(),
                                            default,
                                            Token( SyntaxKind.SemicolonToken ) )
                                        .NormalizeWhitespace();

                                    members.Add( newMethod );
                                }
                            }
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

                if ( this.SymbolClassifier.IsTemplate( propertySymbol ) )
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
                                              this._cancellationToken,
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
                                              this._cancellationToken,
                                              out _,
                                              out transformedSetDeclaration );
                            }

                            // Expression bodied property.
                            if ( node is PropertyDeclarationSyntax { ExpressionBody: not null } propertyNode )
                            {
                                // TODO: Does this preserve trivia in expression body?
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
                                              this._cancellationToken,
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

            public override SyntaxNode? VisitInvocationExpression( InvocationExpressionSyntax node )
            {
                if ( this._currentContext.Scope != SymbolDeclarationScope.RunTimeOnly && node.IsNameOf() )
                {
                    var typeSymbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree )
                        .GetSymbolInfo( node.ArgumentList.Arguments[0].Expression )
                        .Symbol;

                    if ( typeSymbol != null && this.SymbolClassifier.GetSymbolDeclarationScope( typeSymbol ) == SymbolDeclarationScope.RunTimeOnly )
                    {
                        return SyntaxFactoryEx.LiteralExpression( typeSymbol.Name );
                    }
                }

                return base.VisitInvocationExpression( node );
            }

            public override SyntaxNode? VisitTypeOfExpression( TypeOfExpressionSyntax node )
            {
                if ( this._currentContext.Scope != SymbolDeclarationScope.RunTimeOnly )
                {
                    var typeSymbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetSymbolInfo( node.Type ).Symbol;

                    if ( typeSymbol != null && this.SymbolClassifier.GetSymbolDeclarationScope( typeSymbol ) == SymbolDeclarationScope.RunTimeOnly )
                    {
                        // We are in a compile-time-only block but we have a typeof to a run-time-only block. 
                        // This is a situation we can handle by rewriting the typeof to a call to CompileTimeType.CreateFromDocumentationId.
                        var compileTimeType =
                            ReflectionMapper.GetInstance( this._compileTimeCompilation ).GetTypeSyntax( typeof(CompileTimeType) );

                        var memberAccess =
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                compileTimeType,
                                IdentifierName( nameof(CompileTimeType.CreateFromDocumentationId) ) );

                        var invocation = InvocationExpression(
                            memberAccess,
                            ArgumentList(
                                SeparatedList(
                                    new[]
                                    {
                                        Argument( SyntaxFactoryEx.LiteralExpression( typeSymbol.GetDocumentationCommentId() ) ),
                                        Argument( SyntaxFactoryEx.LiteralExpression( typeSymbol.ToDisplayString() ) )
                                    } ) ) );

                        return invocation;
                    }
                }

                return base.VisitTypeOfExpression( node );
            }

            private T? AddLocationAnnotation<T>( T? originalNode, T? transformedNode )
                where T : SyntaxNode
                => originalNode == null || transformedNode == null
                    ? null
                    : (T?) this._templateCompiler.LocationAnnotationMap.AddLocationAnnotation( originalNode, transformedNode! );

            // The default implementation of Visit(SyntaxNode) and Visit(SyntaxToken) adds the location annotations.

            public override SyntaxNode? Visit( SyntaxNode? node ) => this.AddLocationAnnotation( node, base.Visit( node ) );

            public override SyntaxToken VisitToken( SyntaxToken token ) => this._templateCompiler.LocationAnnotationMap.AddLocationAnnotation( token );

            private Context WithScope( SymbolDeclarationScope scope )
            {
                this._currentContext = new Context( scope, this );

                return this._currentContext;
            }

            // TODO: top-level statements?

            private class Context : IDisposable
            {
                private readonly ProduceCompileTimeCodeRewriter _parent;
                private readonly Context _oldContext;

                public Context( SymbolDeclarationScope scope, ProduceCompileTimeCodeRewriter parent )
                {
                    this.Scope = scope;
                    this._parent = parent;

                    // This will be null for the root context.
                    this._oldContext = parent._currentContext;
                }

                public SymbolDeclarationScope Scope { get; }

                public void Dispose()
                {
                    this._parent._currentContext = this._oldContext;
                }
            }
        }
    }
}