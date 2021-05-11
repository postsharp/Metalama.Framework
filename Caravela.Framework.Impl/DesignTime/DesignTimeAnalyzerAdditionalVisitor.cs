// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// Performs the analysis that are not performed by the pipeline: essentially validates that run-time code does not
    /// reference compile-time-only code, and run the template compiler.
    /// </summary>
    internal class DesignTimeAnalyzerAdditionalVisitor : CSharpSyntaxWalker, IDiagnosticAdder
    {
        // TODO: These analysis should probably be moved elsewhere and performed in the pipeline.
        // There seems to be no reason any more to run the template analysis here instead of in the pipeline.

        private readonly ISymbolClassifier _classifier;
        private readonly HashSet<ISymbol> _alreadyReportedDiagnostics = new( SymbolEqualityComparer.Default );
        private readonly bool _isCompileTimeTreeOutdated;
        private readonly SemanticModel _semanticModel;
        private readonly Action<Diagnostic> _reportDiagnostic;
        private SymbolDeclarationScope? _currentDeclarationScope;
        private ISymbol? _currentDeclaration;

        public DesignTimeAnalyzerAdditionalVisitor(
            SemanticModelAnalysisContext context, IBuildOptions buildOptions ) : this( context.SemanticModel, context.ReportDiagnostic,  DesignTimeAspectPipelineCache
                                                               .Instance
                                                               .GetOrCreatePipeline( buildOptions ))
        {
            
        }
        public DesignTimeAnalyzerAdditionalVisitor( SemanticModel semanticModel, Action<Diagnostic> reportDiagnostic, DesignTimeAspectPipeline pipeline  )
        {
            this._semanticModel = semanticModel;
            this._reportDiagnostic = reportDiagnostic;
            this._classifier = SymbolClassifier.GetInstance( semanticModel.Compilation );


            this._isCompileTimeTreeOutdated = pipeline.IsCompileTimeSyntaxTreeOutdated( semanticModel.SyntaxTree.FilePath );
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
                var symbolInfo = this._semanticModel.GetSymbolInfo( node );

                var referencedSymbol = symbolInfo.Symbol;

                if ( referencedSymbol != null && this._classifier.GetSymbolDeclarationScope( referencedSymbol ) == SymbolDeclarationScope.CompileTimeOnly )
                {
                    if ( this._alreadyReportedDiagnostics.Add( referencedSymbol ) &&
                         !(referencedSymbol.ContainingSymbol != null && this._alreadyReportedDiagnostics.Contains( referencedSymbol.ContainingSymbol )) )
                    {
                        this._reportDiagnostic(
                            DesignTimeDiagnosticDescriptors.CannotReferenceCompileTimeOnly.CreateDiagnostic(
                                node.GetLocation(),
                                (this._currentDeclaration!, referencedSymbol) ) );
                    }
                }
            }
        }

        public override void VisitClassDeclaration( ClassDeclarationSyntax node )
        {
            using var scope = this.WithScope( node );

            if ( (scope.Scope == SymbolDeclarationScope.Both || scope.Scope == SymbolDeclarationScope.Both) &&
                 this._isCompileTimeTreeOutdated )
            {
                this._reportDiagnostic(
                    DesignTimeDiagnosticDescriptors.CompileTimeTypeNeedsRebuild.CreateDiagnostic(
                        node.Identifier.GetLocation(),
                        scope.Symbol! ) );
            }

            base.VisitClassDeclaration( node );

        }

        public override void VisitMethodDeclaration( MethodDeclarationSyntax node )
        {
            var methodSymbol = this._semanticModel.GetDeclaredSymbol( node );

            if ( methodSymbol != null && this._classifier.IsTemplate( methodSymbol ) )
            {
                TemplateCompiler templateCompiler = new( ServiceProvider.Empty );
                _ = templateCompiler.TryAnnotate( node, this._semanticModel, this, out _ );
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

        void IDiagnosticAdder.Report( Diagnostic diagnostic )
        {
            if ( DesignTimeAnalyzer.DesignTimeDiagnosticIds.Contains( diagnostic.Id ) )
            {
                this._reportDiagnostic( diagnostic );
            }
        }

        private ScopeCookie WithScope( SyntaxNode node )
        {
            // Reset deduplication.
            this._alreadyReportedDiagnostics.Clear();

            var declaredSymbol = this._semanticModel.GetDeclaredSymbol( node );

            if ( declaredSymbol != null )
            {
                var scope = this._classifier.GetSymbolDeclarationScope( declaredSymbol );
                var context = new ScopeCookie( this, scope, declaredSymbol );
                this._currentDeclarationScope = scope;
                this._currentDeclaration = declaredSymbol;

                return context;
            
            }

            return default;
        }

        private readonly struct ScopeCookie : IDisposable
        {
            private readonly DesignTimeAnalyzerAdditionalVisitor? _parent;
            private readonly SymbolDeclarationScope? _previousScope;
            private readonly ISymbol? _previousDeclaration;

            public ScopeCookie( DesignTimeAnalyzerAdditionalVisitor parent, SymbolDeclarationScope scope, ISymbol? symbol )
            {
                this._parent = parent;
                this._previousScope = parent._currentDeclarationScope;
                this._previousDeclaration = parent._currentDeclaration;
                this.Scope = scope;
                this.Symbol = symbol;
            }

            public SymbolDeclarationScope Scope { get; }

            public ISymbol? Symbol { get; }

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