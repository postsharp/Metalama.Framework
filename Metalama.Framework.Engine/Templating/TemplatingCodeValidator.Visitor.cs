// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Templating
{
    public partial class TemplatingCodeValidator
    {
        /// <summary>
        /// Performs the analysis that are not performed by the pipeline: essentially validates that run-time code does not
        /// reference compile-time-only code, and run the template compiler.
        /// </summary>
        private class Visitor : CSharpSyntaxWalker, IDiagnosticAdder
        {
            private readonly ISymbolClassifier _classifier;
            private readonly HashSet<ISymbol> _alreadyReportedDiagnostics = new( SymbolEqualityComparer.Default );
            private readonly bool _reportCompileTimeTreeOutdatedError;
            private readonly bool _isDesignTime;
            private readonly SemanticModel _semanticModel;
            private readonly Action<Diagnostic> _reportDiagnostic;
            private readonly CancellationToken _cancellationToken;
            private readonly IServiceProvider _serviceProvider;
            private readonly bool _hasCompileTimeCodeFast;
            private TemplateCompiler? _templateCompiler;

            private ISymbol? _currentDeclaration;
            private TemplatingScope? _currentScope;
            private TemplatingScope? _currentTypeScope;
            private TemplateAttributeType? _currentDeclarationTemplateType;

            public bool HasError { get; private set; }

            public Visitor(
                SemanticModel semanticModel,
                Action<Diagnostic> reportDiagnostic,
                IServiceProvider serviceProvider,
                bool reportCompileTimeTreeOutdatedError,
                bool isDesignTime,
                CancellationToken cancellationToken )
            {
                this._semanticModel = semanticModel;
                this._reportDiagnostic = reportDiagnostic;
                this._serviceProvider = serviceProvider;
                this._classifier = this._serviceProvider.GetRequiredService<SymbolClassificationService>().GetClassifier( semanticModel.Compilation );

                this._reportCompileTimeTreeOutdatedError = reportCompileTimeTreeOutdatedError;
                this._isDesignTime = isDesignTime;
                this._cancellationToken = cancellationToken;
                this._hasCompileTimeCodeFast = CompileTimeCodeFastDetector.HasCompileTimeCode( semanticModel.SyntaxTree.GetRoot() );
            }

            private bool IsInTemplate
                => this._currentDeclarationTemplateType.HasValue && this._currentDeclarationTemplateType.Value != TemplateAttributeType.None;

            public override void Visit( SyntaxNode? node )
            {
                bool IsTypeOfOrNameOf()
                {
                    return node
                        .AncestorsAndSelf()
                        .Any( n => n is TypeOfExpressionSyntax || (n is InvocationExpressionSyntax invocation && invocation.IsNameOf()) );
                }

                bool AvoidDuplicates( ISymbol symbol )
                {
                    return this._alreadyReportedDiagnostics.Add( symbol ) &&
                           !(symbol.ContainingSymbol != null
                             && this._alreadyReportedDiagnostics.Contains( symbol.ContainingSymbol ));
                }

                if ( node == null )
                {
                    return;
                }

                this._cancellationToken.ThrowIfCancellationRequested();

                // We want children to be processed before parents, so that errors are reported on parent (declaring) symbols.
                // This allows to reduce redundant messages.
                base.Visit( node );

                // If the scope is null (e.g. in a using statement), we should not analyze.
                if ( !this._currentScope.HasValue )
                {
                    return;
                }

                // Otherwise, we have to check references.

                var referencedSymbol = this._semanticModel.GetSymbolInfo( node ).Symbol;

                if ( referencedSymbol is { } and not ITypeParameterSymbol )
                {
                    var referencedScope = this._classifier.GetTemplatingScope( referencedSymbol );

                    if ( referencedScope == TemplatingScope.CompileTimeOnly )
                    {
                        // ReSharper disable once MissingIndent
                        var isProceed = referencedSymbol is
                        {
                            ContainingSymbol: { Name: nameof(meta) },
                            Name: nameof(meta.Proceed) or nameof(meta.ProceedAsync) or nameof(meta.ProceedEnumerable) or nameof(meta.ProceedEnumerator)
                        };

                        if ( isProceed && !this.IsInTemplate )
                        {
                            // Cannot reference 'meta.Proceed' out of a template.
                            if ( AvoidDuplicates( referencedSymbol ) )
                            {
                                this.Report(
                                    TemplatingDiagnosticDescriptors.CannotUseProceedOutOfTemplate.CreateRoslynDiagnostic(
                                        node.GetLocation(),
                                        this._currentDeclaration! ) );
                            }
                        }
                        else if ( !(this._currentScope.Value.MustExecuteAtCompileTime() || this.IsInTemplate) && !IsTypeOfOrNameOf() )
                        {
                            // We cannot reference a compile-time-only declaration, except in a typeof() or nameof() expression
                            // because these are transformed by the CompileTimeCompilationBuilder.

                            if ( AvoidDuplicates( referencedSymbol ) )
                            {
                                this.Report(
                                    TemplatingDiagnosticDescriptors.CannotReferenceCompileTimeOnly.CreateRoslynDiagnostic(
                                        node.GetLocation(),
                                        (this._currentDeclaration!, referencedSymbol) ) );
                            }
                        }
                    }
                    else if ( referencedScope == TemplatingScope.RunTimeOnly )
                    {
                        if ( this._currentScope.Value == TemplatingScope.CompileTimeOnly && !this.IsInTemplate && !IsTypeOfOrNameOf() )
                        {
                            if ( AvoidDuplicates( referencedSymbol ) )
                            {
                                this.Report(
                                    TemplatingDiagnosticDescriptors.CannotReferenceRunTimeOnly.CreateRoslynDiagnostic(
                                        node.GetLocation(),
                                        (this._currentDeclaration!, referencedSymbol) ) );
                            }
                        }
                    }
                }
            }

            public override void VisitAttribute( AttributeSyntax node )
            {
                // Do not validate custom attributes.
            }

            public override void VisitAttributeList( AttributeListSyntax node )
            {
                // Do not validate custom attributes.
            }

            public override void VisitBaseList( BaseListSyntax node )
            {
                // Do not validate the base list.
            }

            public override void VisitClassDeclaration( ClassDeclarationSyntax node )
            {
                using var scope = this.WithScope( node );

                if ( scope.PreviousScope == TemplatingScope.RunTimeOrCompileTime && this._reportCompileTimeTreeOutdatedError )
                {
                    this.Report(
                        TemplatingDiagnosticDescriptors.CompileTimeTypeNeedsRebuild.CreateRoslynDiagnostic(
                            node.Identifier.GetLocation(),
                            scope.PreviousDeclaration! ) );
                }

                base.VisitClassDeclaration( node );
            }

            public override void VisitMethodDeclaration( MethodDeclarationSyntax node ) => this.VisitBaseMethodOrAccessor( node, base.VisitMethodDeclaration );

            public override void VisitAccessorDeclaration( AccessorDeclarationSyntax node )
                => this.VisitBaseMethodOrAccessor( node, base.VisitAccessorDeclaration );

            private void VisitBaseMethodOrAccessor<T>( T node, Action<T> visitBase )
                where T : SyntaxNode
            {
                using ( this.WithScope( node ) )
                {
                    if ( this.IsInTemplate )
                    {
                        if ( this._isDesignTime )
                        {
                            this._templateCompiler ??= new TemplateCompiler( this._serviceProvider, this._semanticModel.Compilation );
                            _ = this._templateCompiler.TryAnnotate( node, this._semanticModel, this, this._cancellationToken, out _, out _ );
                        }
                        else
                        {
                            // The template compiler will be called by the main pipeline.
                        }
                    }
                    else
                    {
                        visitBase( node );
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

            public override void VisitEventDeclaration( EventDeclarationSyntax node )
            {
                using ( this.WithScope( node ) )
                {
                    base.VisitEventDeclaration( node );
                }
            }

            public override void VisitEventFieldDeclaration( EventFieldDeclarationSyntax node )
            {
                foreach ( var f in node.Declaration.Variables )
                {
                    using ( this.WithScope( f ) )
                    {
                        this.VisitVariableDeclarator( f );
                    }
                }
            }

            public override void VisitFieldDeclaration( FieldDeclarationSyntax node )
            {
                foreach ( var f in node.Declaration.Variables )
                {
                    using ( this.WithScope( f ) )
                    {
                        this.VisitVariableDeclarator( f );
                    }
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
                    var scope = this._classifier.GetTemplatingScope( declaredSymbol );

                    switch ( scope )
                    {
                        case TemplatingScope.Invalid:
                            this.Report(
                                TemplatingDiagnosticDescriptors.InvalidScope.CreateRoslynDiagnostic(
                                    declaredSymbol.GetDiagnosticLocation(),
                                    declaredSymbol ) );

                            break;

                        case TemplatingScope.Conflict:
                            this.Report(
                                TemplatingDiagnosticDescriptors.ScopeConflictInSignature.CreateRoslynDiagnostic(
                                    declaredSymbol.GetDiagnosticLocation(),
                                    declaredSymbol ) );

                            break;

                        default:
                            {
                                if ( scope != TemplatingScope.RunTimeOnly && !this._hasCompileTimeCodeFast )
                                {
                                    this.Report(
                                        TemplatingDiagnosticDescriptors.CompileTimeCodeNeedsNamespaceImport.CreateRoslynDiagnostic(
                                            declaredSymbol.GetDiagnosticLocation(),
                                            (declaredSymbol, CompileTimeCodeFastDetector.Namespace) ) );
                                }

                                break;
                            }
                    }

                    var typeScope = this._currentTypeScope;

                    if ( !typeScope.HasValue && declaredSymbol is ITypeSymbol )
                    {
                        typeScope = scope;
                    }

                    var context = new ScopeCookie( this, scope, typeScope, declaredSymbol );
                    this._currentScope = scope;

                    if ( !this.IsInTemplate )
                    {
                        this._currentDeclarationTemplateType = this._classifier.GetTemplateInfo( declaredSymbol ).AttributeType;
                    }

                    if ( scope == TemplatingScope.Dynamic && this._currentDeclarationTemplateType.GetValueOrDefault() != TemplateAttributeType.Template )
                    {
                        this.Report(
                            TemplatingDiagnosticDescriptors.OnlyNamedTemplatesCanHaveDynamicSignature.CreateRoslynDiagnostic(
                                declaredSymbol.GetDiagnosticLocation(),
                                declaredSymbol ) );
                    }

                    this._currentDeclaration = declaredSymbol;

                    return context;
                }

                return default;
            }

            private readonly struct ScopeCookie : IDisposable
            {
                private readonly Visitor? _parent;
                private readonly TemplatingScope? _previousTypeScope;
                private readonly TemplatingScope? _previousScope;
                private readonly TemplateAttributeType? _previousDeclarationTemplateType;
                private readonly ISymbol? _previousDeclaration;

                public ScopeCookie( Visitor parent, TemplatingScope previousScope, TemplatingScope? previousTypeScope, ISymbol? previousDeclaration )
                {
                    this._parent = parent;
                    this._previousTypeScope = previousTypeScope;
                    this._previousScope = parent._currentScope;
                    this._previousDeclarationTemplateType = parent._currentDeclarationTemplateType;
                    this._previousDeclaration = parent._currentDeclaration;
                    this.PreviousScope = previousScope;
                    this.PreviousDeclaration = previousDeclaration;
                }

                public TemplatingScope PreviousScope { get; }

                public ISymbol? PreviousDeclaration { get; }

                public void Dispose()
                {
                    if ( this._parent != null )
                    {
                        this._parent._currentScope = this._previousScope;
                        this._parent._currentDeclarationTemplateType = this._previousDeclarationTemplateType;
                        this._parent._currentDeclaration = this._previousDeclaration;
                        this._parent._currentTypeScope = this._previousTypeScope;
                    }
                }
            }
        }
    }
}