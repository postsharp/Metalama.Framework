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
        private sealed class ProduceCompileTimeCodeRewriter : Rewriter
        {
            private static readonly SyntaxList<UsingDirectiveSyntax> _templateUsings = ParseCompilationUnit( @"
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Caravela.Framework.Impl.Templating.TemplateSyntaxFactory;
" ).Usings;

            private readonly TemplateCompiler _templateCompiler;
            private readonly List<Diagnostic> _diagnostics = new List<Diagnostic>();
            private bool _addTemplateUsings;

            public bool Success { get; private set; } = true;

            public IReadOnlyList<Diagnostic> Diagnostics => this._diagnostics;

            public bool FoundCompileTimeCode { get; private set; }

            public ProduceCompileTimeCodeRewriter( ISymbolClassifier symbolClassifier, TemplateCompiler templateCompiler, Compilation compilation )
                : base( symbolClassifier, compilation )
            {
                this._templateCompiler = templateCompiler;
            }

            public override SyntaxNode VisitCompilationUnit( CompilationUnitSyntax node )
            {
                node = (CompilationUnitSyntax) base.VisitCompilationUnit( node )!;

                // TODO: handle namespaces properly
                if ( this._addTemplateUsings )
                {
                    // add all template usings, unless such using is already in the list
                    var usingsToAdd = _templateUsings.Where( tu => !node.Usings.Any( u => u.IsEquivalentTo( tu ) ) );

                    node = node.AddUsings( usingsToAdd.ToArray() );
                }

#if DEBUG
                node = node.NormalizeWhitespace();
#endif

                return node;
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
                    case SymbolDeclarationScope.Default or SymbolDeclarationScope.RunTimeOnly:
                        return null;

                    case SymbolDeclarationScope.CompileTimeOnly:
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

                        return (T) node.WithMembers( List( members ) );

                    default:
                        throw new NotImplementedException();
                }
            }

            private new IEnumerable<MethodDeclarationSyntax> VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                if ( this.GetSymbolDeclarationScope( node ) == SymbolDeclarationScope.Template )
                {
                    var success =
                        this._templateCompiler.TryCompile( node, this.Compilation.GetSemanticModel( node.SyntaxTree ), this._diagnostics, out _, out var transformedNode );

                    if ( success )
                    {
                        this._addTemplateUsings = true;

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

            // TODO: top-level statements?
        }
    }
}
