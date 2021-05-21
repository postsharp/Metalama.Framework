// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.DesignTime.Diagnostics;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace Caravela.Framework.Impl.Templating
{
    internal partial class TemplatingCodeValidator
    {
        /// <summary>
        /// Performs the analysis that are not performed by the pipeline: essentially validates that run-time code does not
        /// reference compile-time-only code, and run the template compiler.
        /// </summary>
        private class Visitor : CSharpSyntaxWalker, IDiagnosticAdder
        {
            private readonly ISymbolClassifier _classifier;
            private readonly HashSet<ISymbol> _alreadyReportedDiagnostics = new( SymbolEqualityComparer.Default );
            private readonly bool _isCompileTimeTreeOutdated;
            private readonly bool _isDesignTime;
            private readonly SemanticModel _semanticModel;
            private readonly Action<Diagnostic> _reportDiagnostic;
            private readonly CancellationToken _cancellationToken;
            private readonly IServiceProvider _serviceProvider;
            private readonly bool _hasCompileTimeCodeFast;

            private SymbolDeclarationScope? _currentDeclarationScope;
            private ISymbol? _currentDeclaration;
            
            public bool HasError { get; private set; }

            public Visitor(
                SemanticModel semanticModel,
                Action<Diagnostic> reportDiagnostic,
                DesignTimeAspectPipeline pipeline,
                CancellationToken cancellationToken ) : this(
                semanticModel,
                reportDiagnostic,
                pipeline.ServiceProvider,
                pipeline.IsCompileTimeSyntaxTreeOutdated( semanticModel.SyntaxTree.FilePath ),
                true,
                cancellationToken ) { }

            public Visitor(
                SemanticModel semanticModel,
                Action<Diagnostic> reportDiagnostic,
                IServiceProvider serviceProvider,
                bool isCompileTimeTreeOutdated,
                bool isDesignTime,
                CancellationToken cancellationToken )
            {
                this._semanticModel = semanticModel;
                this._reportDiagnostic = reportDiagnostic;
                this._serviceProvider = serviceProvider;
                this._classifier = this._serviceProvider.GetService<SymbolClassificationService>().GetClassifier( semanticModel.Compilation );

                this._isCompileTimeTreeOutdated = isCompileTimeTreeOutdated;
                this._isDesignTime = isDesignTime;
                this._cancellationToken = cancellationToken;
                this._hasCompileTimeCodeFast = CompileTimeCodeDetector.HasCompileTimeCode( semanticModel.SyntaxTree.GetRoot() );
            }

            public override void Visit( SyntaxNode? node )
            {
                if ( node == null )
                {
                    return;
                }

                this._cancellationToken.ThrowIfCancellationRequested();

                // We want children to be processed before parents, so that errors are reported on parent (declaring) symbols.
                // This allows to reduce redundant messages.
                base.Visit( node );

                // If the scope is null (e.g. in a using statement) or compile-time-only, we should not analyze.
                // Otherwise, we cannot reference a compile-time-only declaration, except in a typeof() or nameof() expression
                // because these are transformed by the CompileTimeCompilationBuilder.

                if ( this._currentDeclarationScope is SymbolDeclarationScope.RunTimeOnly )
                {
                    var symbolInfo = this._semanticModel.GetSymbolInfo( node );

                    var referencedSymbol = symbolInfo.Symbol;

                    if ( referencedSymbol != null && 
                         this._classifier.GetSymbolDeclarationScope( referencedSymbol ) == SymbolDeclarationScope.CompileTimeOnly && 
                         !node.AncestorsAndSelf().Any( n => n is TypeOfExpressionSyntax || (n is InvocationExpressionSyntax invocation && invocation.IsNameOf())) )
                    {
                        if ( this._alreadyReportedDiagnostics.Add( referencedSymbol ) &&
                             !(referencedSymbol.ContainingSymbol != null && this._alreadyReportedDiagnostics.Contains( referencedSymbol.ContainingSymbol )) )
                        {
                            this.Report(
                                TemplatingDiagnosticDescriptors.CannotReferenceCompileTimeOnly.CreateDiagnostic(
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
                    this.Report(
                        TemplatingDiagnosticDescriptors.CompileTimeTypeNeedsRebuild.CreateDiagnostic(
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
                    if ( this._isDesignTime )
                    {
                        TemplateCompiler templateCompiler = new( this._serviceProvider );
                        _ = templateCompiler.TryAnnotate( node, this._semanticModel, this, this._cancellationToken, out _ );
                    }
                    else
                    {
                        // The template compiler will be called by the main pipeline.
                    }
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

            public void Report( Diagnostic diagnostic )
            {
                if ( diagnostic.Severity == DiagnosticSeverity.Error )
                {
                    this.HasError = true;
                }
                
                this._reportDiagnostic( diagnostic );
            }

            private ScopeCookie WithScope( SyntaxNode node )
            {
                // Reset deduplication.
                this._alreadyReportedDiagnostics.Clear();

                var declaredSymbol = this._semanticModel.GetDeclaredSymbol( node );

                if ( declaredSymbol != null )
                {
                    var scope = this._classifier.GetSymbolDeclarationScope( declaredSymbol );

                    if ( scope != SymbolDeclarationScope.RunTimeOnly && !this._hasCompileTimeCodeFast )
                    {
                        this.Report(
                            TemplatingDiagnosticDescriptors.CompileTimeCodeNeedsNamespaceImport.CreateDiagnostic(
                                declaredSymbol.GetDiagnosticLocation(),
                                (declaredSymbol, CompileTimeCodeDetector.Namespaces.ToArray()) ) );
                    }

                    var context = new ScopeCookie( this, scope, declaredSymbol );
                    this._currentDeclarationScope = scope;
                    this._currentDeclaration = declaredSymbol;

                    return context;
                }

                return default;
            }

            private readonly struct ScopeCookie : IDisposable
            {
                private readonly Visitor? _parent;
                private readonly SymbolDeclarationScope? _previousScope;
                private readonly ISymbol? _previousDeclaration;

                public ScopeCookie( Visitor parent, SymbolDeclarationScope scope, ISymbol? symbol )
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
}