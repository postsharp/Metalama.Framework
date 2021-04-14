// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CompileTime
{
    internal partial class CompileTimeAssemblyBuilder
    {
        public static readonly SyntaxAnnotation HasCompileTimeCodeAnnotation = new( "hasCompileTimeCode" );

        private sealed class ProduceCompileTimeCodeRewriter : Rewriter
        {

            private readonly TemplateCompiler _templateCompiler;
            private readonly Compilation _compileTimeCompilation;
            private readonly List<Diagnostic> _diagnostics = new();

            public bool Success { get; private set; } = true;

            public IReadOnlyList<Diagnostic> Diagnostics => this._diagnostics;

            public bool FoundCompileTimeCode { get; private set; }

            public ProduceCompileTimeCodeRewriter(
                ISymbolClassifier symbolClassifier,
                TemplateCompiler templateCompiler,
                Compilation runTimeCompilation,
                Compilation compileTimeCompilation )
                : base( symbolClassifier, runTimeCompilation )
            {
                this._templateCompiler = templateCompiler;
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
                        this._templateCompiler.TryCompile( this._compileTimeCompilation, node, this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ), this._diagnostics, out _, out var transformedNode );

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

            // TODO: top-level statements?
        }
    }
}
