// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.DesignTime
{
    public partial class DesignTimeAnalyzer
    {
        private class Visitor : CSharpSyntaxWalker, IDiagnosticAdder
        {
            private readonly SemanticModelAnalysisContext _context;
            private readonly ISymbolClassifier _classifier;
            private readonly HashSet<ISymbol> _alreadyReportedDiagnostics = new( SymbolEqualityComparer.Default );
            private SymbolDeclarationScope? _currentDeclarationScope;
            private ISymbol? _currentDeclaration;

            public Visitor( SemanticModelAnalysisContext context )
            {
                this._context = context;
                this._classifier = SymbolClassifier.GetInstance( context.SemanticModel.Compilation );
            }

            public override void Visit( SyntaxNode? node )
            {
                if ( node == null )
                {
                    return;
                }

                // We want children to be processed before parents, so that errors are reported on parent (declaring) symbols.
                // This allows to reduce redundant messages.
                base.Visit( node );

                // If the scope is null (e.g. in a using statement) or compile-time-only, we should not analyze.
                // Otherwise, we cannot reference a compile-time-only declaration.

                if ( this._currentDeclarationScope.HasValue && this._currentDeclarationScope != SymbolDeclarationScope.CompileTimeOnly )
                {
                    var symbolInfo = this._context.SemanticModel.GetSymbolInfo( node );

                    var referencedSymbol = symbolInfo.Symbol;

                    if ( referencedSymbol != null && this._classifier.GetSymbolDeclarationScope( referencedSymbol ) == SymbolDeclarationScope.CompileTimeOnly )
                    {
                        if ( this._alreadyReportedDiagnostics.Add( referencedSymbol ) &&
                             !(referencedSymbol.ContainingSymbol != null && this._alreadyReportedDiagnostics.Contains( referencedSymbol.ContainingSymbol )) )
                        {
                            this._context.ReportDiagnostic(
                                DesignTimeDiagnosticDescriptors.CannotReferenceCompileTimeOnly.CreateDiagnostic(
                                    node.GetLocation(),
                                    (this._currentDeclaration!, referencedSymbol) ) );
                        }
                    }
                }
            }

            public override void VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                using ( this.WithScope( node ) )
                {
                    base.VisitClassDeclaration( node );
                }
            }

            public override void VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                var methodSymbol = this._context.SemanticModel.GetDeclaredSymbol( node );

                if ( methodSymbol != null && this._classifier.IsTemplate( methodSymbol ) )
                {
                    _ = TemplateCompiler.TryAnnotate( node, this._context.SemanticModel, true, this, out _ );
                }
                else
                {
                    using ( this.WithScope( node ) )
                    {
                        base.VisitMethodDeclaration( node );
                    }
                }
            }

            public override void VisitPropertyDeclaration( PropertyDeclarationSyntax node )
            {
                using ( this.WithScope( node ) )
                {
                    base.VisitPropertyDeclaration( node );
                }
            }

            public override void VisitFieldDeclaration( FieldDeclarationSyntax node )
            {
                using ( this.WithScope( node ) )
                {
                    base.VisitFieldDeclaration( node );
                }
            }

            public override void VisitConstructorDeclaration( ConstructorDeclarationSyntax node )
            {
                using ( this.WithScope( node ) )
                {
                    base.VisitConstructorDeclaration( node );
                }
            }

            public override void VisitOperatorDeclaration( OperatorDeclarationSyntax node )
            {
                using ( this.WithScope( node ) )
                {
                    base.VisitOperatorDeclaration( node );
                }
            }

            public override void VisitIncompleteMember( IncompleteMemberSyntax node )
            {
                // Skip
            }

            void IDiagnosticAdder.ReportDiagnostic( Diagnostic diagnostic )
            {
                if ( DesignTimeDiagnosticIds.Contains( diagnostic.Id ) )
                {
                    this._context.ReportDiagnostic( diagnostic );
                }
            }

            private ScopeCookie WithScope( SyntaxNode node )
            {
                // Reset deduplication.
                this._alreadyReportedDiagnostics.Clear();

                var declaredSymbol = this._context.SemanticModel.GetDeclaredSymbol( node );

                if ( declaredSymbol != null )
                {
                    var scope = this._classifier.GetSymbolDeclarationScope( declaredSymbol );

                    if ( scope != SymbolDeclarationScope.Both )
                    {
                        var context = new ScopeCookie( this );
                        this._currentDeclarationScope = scope;
                        this._currentDeclaration = declaredSymbol;

                        return context;
                    }
                }

                return default;
            }

            private readonly struct ScopeCookie : IDisposable
            {
                private readonly Visitor? _parent;
                private readonly SymbolDeclarationScope? _previousScope;
                private readonly ISymbol? _previousDeclaration;

                public ScopeCookie( Visitor parent )
                {
                    this._parent = parent;
                    this._previousScope = parent._currentDeclarationScope;
                    this._previousDeclaration = parent._currentDeclaration;
                }

                public void Dispose()
                {
                    if ( this._parent != null )
                    {
                        this._parent._currentDeclarationScope = this._previousScope;
                        this._parent._currentDeclaration = this._previousDeclaration;
                    }
                }
            }
        }
    }
}