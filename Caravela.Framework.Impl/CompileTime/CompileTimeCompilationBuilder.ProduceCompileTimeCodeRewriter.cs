// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CompileTime
{
    internal partial class CompileTimeCompilationBuilder
    {
        /// <summary>
        /// Rewrites a run-time syntax tree into a compile-time syntax tree. Calls <see cref="TemplateCompiler"/> on templates,
        /// and removes run-time-only sub trees.
        /// </summary>
        private sealed class ProduceCompileTimeCodeRewriter : Rewriter
        {
            private static readonly SyntaxAnnotation _hasCompileTimeCodeAnnotation = new( "hasCompileTimeCode" );
            private readonly Compilation _compileTimeCompilation;
            private readonly IDiagnosticAdder _diagnosticAdder;

            public bool Success { get; private set; } = true;

            public bool FoundCompileTimeCode { get; private set; }

            public ProduceCompileTimeCodeRewriter(
                Compilation runTimeCompilation,
                Compilation compileTimeCompilation,
                IDiagnosticAdder diagnosticAdder )
                : base( runTimeCompilation )
            {
                this._compileTimeCompilation = compileTimeCompilation;
                this._diagnosticAdder = diagnosticAdder;
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
                        TemplateCompiler.TryCompile(
                            TemplateNameHelper.GetCompiledTemplateName( methodSymbol.Name ),
                            this._compileTimeCompilation,
                            node,
                            this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ),
                            this._diagnosticAdder,
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
                var propertySymbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node );

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

                            if ( getAccessor != null )
                            {
                                success = success &&
                                    TemplateCompiler.TryCompile(
                                        TemplateNameHelper.GetCompiledPropertyGetTemplateName( propertySymbol.Name ),
                                        this._compileTimeCompilation,
                                        getAccessor,
                                        this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ),
                                        this._diagnosticAdder,
                                        out _,
                                        out transformedGetDeclaration );
                            }

                            if ( setAccessor != null )
                            {
                                success = success &&
                                       TemplateCompiler.TryCompile(
                                        TemplateNameHelper.GetCompiledPropertySetTemplateName( propertySymbol.Name ),
                                        this._compileTimeCompilation,
                                        setAccessor,
                                        this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ),
                                        this._diagnosticAdder,
                                        out _,
                                        out transformedSetDeclaration );
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
                    yield return (MethodDeclarationSyntax) this.Visit( node ).AssertNotNull();
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

            private static MethodDeclarationSyntax RewriteAccessorToMethod(
                AccessorDeclarationSyntax node,
                string templateMethodName,
                TypeSyntax methodGroupReturnType,
                SeparatedSyntaxList<ParameterSyntax>? methodGroupParameters )
            {
                if ( UsesValueKeyword( node.Keyword ) )
                {
                    return
                        MethodDeclaration(
                                PredefinedType( Token( SyntaxKind.VoidKeyword ) ),
                                templateMethodName )
                            .WithParameterList(
                                methodGroupParameters != null
                                    ? ParameterList(
                                        methodGroupParameters.Value.Add(
                                            Parameter( List<AttributeListSyntax>(), TokenList(), methodGroupReturnType, Identifier( "value" ), null ) ) )
                                    : ParameterList(
                                        SingletonSeparatedList(
                                            Parameter( List<AttributeListSyntax>(), TokenList(), methodGroupReturnType, Identifier( "value" ), null ) ) ) )
                            .WithExpressionBody( node.ExpressionBody )
                            .WithBody( node.Body )
                            .WithSemicolonToken( node.SemicolonToken )
                            .NormalizeWhitespace();
                }
                else
                {
                    return
                        MethodDeclaration(
                                methodGroupReturnType,
                                templateMethodName )
                            .WithParameterList( methodGroupParameters != null ? ParameterList( methodGroupParameters.Value ) : ParameterList() )
                            .WithExpressionBody( node.ExpressionBody )
                            .WithBody( node.Body )
                            .WithSemicolonToken( node.SemicolonToken )
                            .NormalizeWhitespace();
                }

                static bool UsesValueKeyword( SyntaxToken keyword )
                {
                    return keyword.Kind() != SyntaxKind.GetAccessorDeclaration;
                }
            }

            // TODO: top-level statements?
        }
    }
}