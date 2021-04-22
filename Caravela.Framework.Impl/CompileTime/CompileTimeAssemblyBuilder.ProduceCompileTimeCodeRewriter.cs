// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CompileTime
{
    internal partial class CompileTimeAssemblyBuilder
    {
        public static readonly SyntaxAnnotation HasCompileTimeCodeAnnotation = new( "hasCompileTimeCode" );

        private sealed class ProduceCompileTimeCodeRewriter : Rewriter
        {
            private readonly Compilation _compileTimeCompilation;
            private readonly List<Diagnostic> _diagnostics = new();

            public bool Success { get; private set; } = true;

            public IReadOnlyList<Diagnostic> Diagnostics => this._diagnostics;

            public bool FoundCompileTimeCode { get; private set; }

            public ProduceCompileTimeCodeRewriter(
                ISymbolClassifier symbolClassifier,
                Compilation runTimeCompilation,
                Compilation compileTimeCompilation )
                : base( symbolClassifier, runTimeCompilation )
            {
                this._compileTimeCompilation = compileTimeCompilation;
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

                        return (T) node.WithMembers( List( members ) ).WithAdditionalAnnotations( HasCompileTimeCodeAnnotation );
                }
            }

            private new IEnumerable<MethodDeclarationSyntax> VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                var methodSymbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node );

                if ( methodSymbol != null && this.SymbolClassifier.IsTemplate( methodSymbol ) )
                {
                    var success =
                        TemplateCompiler.TryCompile(
                            this._compileTimeCompilation,
                            node,
                            this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ),
                            this._diagnostics,
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
                    SyntaxNode? transformedGetNode = null;
                    SyntaxNode? transformedSetNode = null;

                    // Compile accessors into templates.
                    if ( !propertySymbol.IsAbstract )
                    {
                        if ( node.AccessorList != null )
                        {
                            var getAccessor = node.AccessorList.Accessors.SingleOrDefault( a => a.Kind() == SyntaxKind.GetAccessorDeclaration );
                            var setAccessor = node.AccessorList.Accessors.SingleOrDefault( a => a.Kind() == SyntaxKind.SetAccessorDeclaration || a.Kind() == SyntaxKind.InitAccessorDeclaration );

                            var propertyIdentifier = node switch
                            {
                                PropertyDeclarationSyntax property => property.Identifier.ValueText,
                                IndexerDeclarationSyntax indexer => "Indexer",
                                _ => throw new AssertionFailedException()
                            };

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
                                        this._compileTimeCompilation,
                                        RewriteAccessorToMethod( getAccessor, propertyIdentifier, node.Type, propertyParameters ),
                                        this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ),
                                        this._diagnostics,
                                        out _,
                                        out transformedGetNode );
                            }

                            if ( setAccessor != null )
                            {
                                success = success &&
                                    TemplateCompiler.TryCompile(
                                        this._compileTimeCompilation,
                                        RewriteAccessorToMethod( setAccessor, propertyIdentifier, node.Type, propertyParameters ),
                                        this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ),
                                        this._diagnostics,
                                        out _,
                                        out transformedSetNode );
                            }
                        }
                    }

                    if ( success )
                    {
                        yield return WithThrowNotSupportedExceptionBody( node, "Template code cannot be directly executed." );

                        if ( transformedGetNode != null )
                        {
                            yield return (MethodDeclarationSyntax) transformedGetNode.AssertNotNull();
                        }

                        if ( transformedGetNode != null )
                        {
                            yield return (MethodDeclarationSyntax) transformedGetNode.AssertNotNull();
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

                if ( transformedNode.Members.Any( m => m.HasAnnotation( HasCompileTimeCodeAnnotation ) ) )
                {
                    return transformedNode.WithAdditionalAnnotations( HasCompileTimeCodeAnnotation );
                }
                else
                {
                    return null;
                }
            }

            public override SyntaxNode? VisitCompilationUnit( CompilationUnitSyntax node )
            {
                var transformedNode = (CompilationUnitSyntax) base.VisitCompilationUnit( node )!;

                if ( transformedNode.Members.Any( m => m.HasAnnotation( HasCompileTimeCodeAnnotation ) ) )
                {
                    return transformedNode.WithAdditionalAnnotations( HasCompileTimeCodeAnnotation );
                }
                else
                {
                    return null;
                }
            }

            private static MethodDeclarationSyntax RewriteAccessorToMethod( AccessorDeclarationSyntax node, string methodGroupName, TypeSyntax methodGroupReturnType, SeparatedSyntaxList<ParameterSyntax>? methodGroupParameters )
            {
                if ( UsesValueKeyword( node.Keyword ) )
                {
                    return
                        MethodDeclaration(
                            PredefinedType( Token( SyntaxKind.VoidKeyword ) ),
                            $"{node.Keyword.ValueText}_{methodGroupName}" )
                        .WithParameterList(
                            methodGroupParameters != null
                            ? ParameterList( methodGroupParameters.Value.Add(
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
                            $"{node.Keyword.ValueText}_{methodGroupName}" )
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