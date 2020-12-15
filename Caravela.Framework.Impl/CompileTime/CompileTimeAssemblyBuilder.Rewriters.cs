using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.CompileTime
{
    partial class CompileTimeAssemblyBuilder
    {
        abstract class Rewriter : CSharpSyntaxRewriter
        {
            private readonly ISymbolClassifier _symbolClassifier;
            protected readonly Compilation _compilation;

            protected Rewriter( ISymbolClassifier symbolClassifier, Compilation compilation )
            {
                this._symbolClassifier = symbolClassifier;
                this._compilation = compilation;
            }

            protected SymbolDeclarationScope GetSymbolDeclarationScope( MemberDeclarationSyntax node )
            {
                var symbol = this._compilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node )!;
                return this._symbolClassifier.GetSymbolDeclarationScope( symbol );
            }
        }

        sealed class ProduceCompileTimeCodeRewriter : Rewriter
        {
            private readonly TemplateCompiler _templateCompiler;

            public bool FoundCompileTimeCode { get; private set; }

            public ProduceCompileTimeCodeRewriter( ISymbolClassifier symbolClassifier, TemplateCompiler templateCompiler, Compilation compilation )
                : base( symbolClassifier, compilation )
            {
                this._templateCompiler = templateCompiler;
            }

            private bool _addTemplateUsings;
            private static readonly SyntaxList<UsingDirectiveSyntax> _templateUsings = SyntaxFactory.ParseCompilationUnit(@"
using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Caravela.Framework.Impl.Templating.TemplateHelper;
").Usings;

            public override SyntaxNode VisitCompilationUnit(CompilationUnitSyntax node)
            {
                node = (CompilationUnitSyntax)base.VisitCompilationUnit(node)!;

                // TODO: handle namespaces properly
                if (this._addTemplateUsings)
                {
                    // add all template usings, unless such using is already in the list
                    var usingsToAdd = _templateUsings.Where(tu => !node.Usings.Any(u => u.IsEquivalentTo(tu)));

                    node = node.AddUsings(usingsToAdd.ToArray());
                }

#if DEBUG
                node = node.NormalizeWhitespace();
#endif

                return node;
            }

            // TODO: assembly and module-level attributes?
            public override SyntaxNode? VisitAttributeList(AttributeListSyntax node) => node.Parent is CompilationUnitSyntax ? null : node;

            public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node) => this.VisitTypeDeclaration(node);
            public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node) => this.VisitTypeDeclaration(node);
            public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node) => this.VisitTypeDeclaration(node);
            public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax node) => this.VisitTypeDeclaration(node);

            private T? VisitTypeDeclaration<T>(T node) where T : TypeDeclarationSyntax
            {
                switch (this.GetSymbolDeclarationScope(node))
                {
                    case SymbolDeclarationScope.Default or SymbolDeclarationScope.RunTimeOnly:
                        return null;

                    case SymbolDeclarationScope.CompileTimeOnly:
                        this.FoundCompileTimeCode = true;

                        var members = new List<MemberDeclarationSyntax>();

                        foreach (var member in node.Members)
                        {
                            switch (member)
                            {
                                case MethodDeclarationSyntax method:
                                    members.AddRange(this.VisitMethodDeclaration(method));
                                    break;
                                case TypeDeclarationSyntax nestedType:
                                    members.Add((MemberDeclarationSyntax)this.Visit(nestedType));
                                    break;
                                default:
                                    members.Add(member);
                                    break;
                            }
                        }

                        return (T)node.WithMembers(SyntaxFactory.List(members));

                    default:
                        throw new NotImplementedException();
                }
            }

            private new IEnumerable<MethodDeclarationSyntax> VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                if (this.GetSymbolDeclarationScope(node) == SymbolDeclarationScope.Template)
                {
                    var diagnostics = new List<Diagnostic>();
                    bool success =
                        this._templateCompiler.TryCompile(node, this._compilation.GetSemanticModel(node.SyntaxTree), diagnostics, out _, out var transformedNode);

                    Debug.Assert(success || diagnostics.Any(d => d.Severity >= DiagnosticSeverity.Error));

                    if (success)
                    {
                        // reporting warnings is currently not supported here
                        Debug.Assert(diagnostics.Count == 0);

                        this._addTemplateUsings = true;
                    }
                    else
                        throw new DiagnosticsException(GeneralDiagnosticDescriptors.ErrorProcessingTemplates, diagnostics.ToImmutableArray());

                    yield return node;
                    yield return (MethodDeclarationSyntax)transformedNode!;
                }
                else
                {
                    yield return (MethodDeclarationSyntax)base.VisitMethodDeclaration(node)!;
                }
            }

            // TODO: top-level statements?
        }

        // PERF: explicitly skip over all other nodes?
        internal class RemoveInvalidUsingsRewriter : CSharpSyntaxRewriter
        {
            private readonly Compilation _compilation;

            public RemoveInvalidUsingsRewriter(Compilation compilation) => this._compilation = compilation;

            public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax node)
            {
                var symbolInfo = this._compilation.GetSemanticModel(node.SyntaxTree).GetSymbolInfo(node.Name);

                if (symbolInfo.Symbol == null)
                    return null;

                return node;
            }
        }

        private class PrepareRunTimeAssemblyRewriter : Rewriter
        {
            public PrepareRunTimeAssemblyRewriter( ISymbolClassifier symbolClassifier, Compilation compilation ) : base( symbolClassifier, compilation ) { }

            public override SyntaxNode? VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                if ( this.GetSymbolDeclarationScope( node ) is SymbolDeclarationScope.CompileTimeOnly or SymbolDeclarationScope.Template )
                {
                    string message = "Compile-time only code cannot be called at run-time.";

                    // throw new System.NotSupportedException("message")
                    var body = ThrowExpression( ObjectCreationExpression( ParseTypeName( "System.NotSupportedException" ) )
                        .AddArgumentListArguments( Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( message ) ) ) ) );

                    return node.WithBody( null ).WithExpressionBody( ArrowExpressionClause( body ) );
                }

                return node;
            }
        }

    }
}
