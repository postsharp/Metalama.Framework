// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
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
        private class Visitor : SafeSyntaxWalker, IDiagnosticAdder
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

            private bool IsInTemplate => this._currentDeclarationTemplateType is not (null or TemplateAttributeType.None);

            protected override void VisitCore( SyntaxNode? node )
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

                if ( node == null || node is IdentifierNameSyntax { IsVar: true } )
                {
                    // We skip 'var' because the semantic model sometimes resolve it to dynamic for no reason,
                    // and there is little value in spending more effort coping with this case.
                    return;
                }

                this._cancellationToken.ThrowIfCancellationRequested();

                // We want children to be processed before parents, so that errors are reported on parent (declaring) symbols.
                // This allows to reduce redundant messages.
                base.VisitCore( node );

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

                    if ( referencedScope.GetExpressionExecutionScope() == TemplatingScope.CompileTimeOnly )
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
                                        (this._currentDeclaration!, referencedSymbol, this._currentScope.Value) ) );
                            }
                        }
                    }
                    else if ( referencedScope.GetExpressionExecutionScope() == TemplatingScope.RunTimeOnly )
                    {
                        if ( this._currentScope.Value.GetExpressionExecutionScope() != TemplatingScope.RunTimeOnly && !this.IsInTemplate
                            && !IsTypeOfOrNameOf() )
                        {
                            if ( AvoidDuplicates( referencedSymbol ) )
                            {
                                this.Report(
                                    TemplatingDiagnosticDescriptors.CannotReferenceRunTimeOnly.CreateRoslynDiagnostic(
                                        node.GetLocation(),
                                        (this._currentDeclaration!, referencedSymbol, this._currentScope.Value) ) );
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
                using var context = this.WithContext( node );

                this.VerifyTypeDeclaration( node, context );
                base.VisitClassDeclaration( node );
            }

            public override void VisitStructDeclaration( StructDeclarationSyntax node )
            {
                using var context = this.WithContext( node );

                this.VerifyTypeDeclaration( node, context );
                base.VisitStructDeclaration( node );
            }

            public override void VisitRecordDeclaration( RecordDeclarationSyntax node )
            {
                using var context = this.WithContext( node );

                this.VerifyTypeDeclaration( node, context );

                base.VisitRecordDeclaration( node );
            }

            public override void VisitInterfaceDeclaration( InterfaceDeclarationSyntax node )
            {
                using var context = this.WithContext( node );

                this.VerifyTypeDeclaration( node, context );

                base.VisitInterfaceDeclaration( node );
            }

            private void VerifyTypeDeclaration( BaseTypeDeclarationSyntax node, in Context context )
            {
                // Report an error on aspect classes when the pipeline is paused.
                if ( this._currentScope == TemplatingScope.RunTimeOrCompileTime && this._reportCompileTimeTreeOutdatedError )
                {
                    this.Report(
                        TemplatingDiagnosticDescriptors.CompileTimeTypeNeedsRebuild.CreateRoslynDiagnostic(
                            node.Identifier.GetLocation(),
                            context.DeclaredSymbol! ) );
                }

                // Verify that the base class and implemented interfaces are scope-compatible.
                // If the scope is conflict, an error message is written elsewhere.

                if ( node.BaseList != null && this._currentScope != TemplatingScope.Conflict )
                {
                    foreach ( var baseTypeNode in node.BaseList.Types )
                    {
                        var baseType = (INamedTypeSymbol?) this._semanticModel.GetSymbolInfo( baseTypeNode.Type ).Symbol;

                        if ( baseType == null )
                        {
                            continue;
                        }

                        var baseTypeScope = this._classifier.GetTemplatingScope( baseType );

                        if ( baseTypeScope is TemplatingScope.Conflict or TemplatingScope.Invalid )
                        {
                            this._classifier.ReportScopeError( baseTypeNode, baseType, this );
                        }
                        else
                        {
                            var isAcceptableScope = (this._currentScope, scope: baseTypeScope) switch
                            {
                                (TemplatingScope.CompileTimeOnly, TemplatingScope.CompileTimeOnly) => true,
                                (TemplatingScope.CompileTimeOnly, TemplatingScope.RunTimeOrCompileTime) => true,
                                (TemplatingScope.RunTimeOnly, TemplatingScope.RunTimeOnly) => true,
                                (TemplatingScope.RunTimeOnly, TemplatingScope.RunTimeOrCompileTime) => true,
                                (TemplatingScope.RunTimeOrCompileTime, _) => true,
                                _ => false
                            };

                            if ( !isAcceptableScope )
                            {
                                this.Report(
                                    TemplatingDiagnosticDescriptors.BaseTypeScopeConflict.CreateRoslynDiagnostic(
                                        baseTypeNode.Type.GetLocation(),
                                        ((INamedTypeSymbol) context.DeclaredSymbol!, this._currentScope!.Value.ToDisplayString(), baseType,
                                         baseTypeScope.ToDisplayString()) ) );
                            }
                        }
                    }
                }
            }

            public override void VisitMethodDeclaration( MethodDeclarationSyntax node ) => this.VisitBaseMethodOrAccessor( node, base.VisitMethodDeclaration );

            public override void VisitAccessorDeclaration( AccessorDeclarationSyntax node )
                => this.VisitBaseMethodOrAccessor( node, base.VisitAccessorDeclaration );

            private void VisitBaseMethodOrAccessor<T>( T node, Action<T> visitBase )
                where T : SyntaxNode
            {
                using ( this.WithContext( node ) )
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
                using ( this.WithContext( node ) )
                {
                    base.VisitPropertyDeclaration( node );
                }
            }

            public override void VisitConstructorDeclaration( ConstructorDeclarationSyntax node )
            {
                using ( this.WithContext( node ) )
                {
                    base.VisitConstructorDeclaration( node );
                }
            }

            public override void VisitOperatorDeclaration( OperatorDeclarationSyntax node )
            {
                using ( this.WithContext( node ) )
                {
                    base.VisitOperatorDeclaration( node );
                }
            }

            public override void VisitEventDeclaration( EventDeclarationSyntax node )
            {
                using ( this.WithContext( node ) )
                {
                    base.VisitEventDeclaration( node );
                }
            }

            public override void VisitEventFieldDeclaration( EventFieldDeclarationSyntax node )
            {
                foreach ( var f in node.Declaration.Variables )
                {
                    using ( this.WithContext( f ) )
                    {
                        this.Visit( node.Declaration.Type );
                        this.VisitVariableDeclarator( f );
                    }
                }
            }

            public override void VisitFieldDeclaration( FieldDeclarationSyntax node )
            {
                foreach ( var f in node.Declaration.Variables )
                {
                    using ( this.WithContext( f ) )
                    {
                        this.Visit( node.Declaration.Type );
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

            private Context WithContext( SyntaxNode node )
            {
                // Reset deduplication.
                this._alreadyReportedDiagnostics.Clear();

                var declaredSymbol = this._semanticModel.GetDeclaredSymbol( node );

                if ( declaredSymbol == null )
                {
                    return default;
                }

                var scope = this._classifier.GetTemplatingScope( declaredSymbol );

                // Report error on invalid scope.
                switch ( scope )
                {
                    case TemplatingScope.Invalid:
                        this.Report(
                            TemplatingDiagnosticDescriptors.InvalidScope.CreateRoslynDiagnostic(
                                declaredSymbol.GetDiagnosticLocation(),
                                declaredSymbol ) );

                        break;

                    case TemplatingScope.Conflict:
                        this._classifier.ReportScopeError( node, declaredSymbol, this );

                        break;

                    default:
                        {
                            if ( scope != TemplatingScope.RunTimeOnly && !this._hasCompileTimeCodeFast
                                                                      && !SystemTypeDetector.IsSystemType(
                                                                          declaredSymbol as INamedTypeSymbol ?? declaredSymbol.ContainingType ) )
                            {
                                this.Report(
                                    TemplatingDiagnosticDescriptors.CompileTimeCodeNeedsNamespaceImport.CreateRoslynDiagnostic(
                                        declaredSymbol.GetDiagnosticLocation(),
                                        (declaredSymbol, CompileTimeCodeFastDetector.Namespace) ) );
                            }

                            break;
                        }
                }

                // Get the type scope.
                TemplatingScope? typeScope;

                if ( declaredSymbol is INamedTypeSymbol namedType )
                {
                    typeScope = this._classifier.GetTemplatingScope( namedType );
                }
                else
                {
                    typeScope = this._currentTypeScope;
                }

                // Get the template type.
                var templateType = this._currentDeclarationTemplateType;

                if ( !this.IsInTemplate )
                {
                    templateType = this._classifier.GetTemplateInfo( declaredSymbol ).AttributeType;
                }

                // Check that 'dynamic' is used only in a template or in run-time-only code.
                var isTemplate = templateType is not (null or TemplateAttributeType.None);

                if ( scope == TemplatingScope.Dynamic && typeScope != TemplatingScope.RunTimeOnly && isTemplate )
                {
                    this.Report(
                        TemplatingDiagnosticDescriptors.OnlyNamedTemplatesCanHaveDynamicSignature.CreateRoslynDiagnostic(
                            declaredSymbol.GetDiagnosticLocation(),
                            declaredSymbol ) );
                }

                // Check that run-time members are contained in run-time types.
                if ( scope == TemplatingScope.RunTimeOnly && typeScope != TemplatingScope.RunTimeOnly && !isTemplate )
                {
                    // If we have an illegal run-time scope, we don't perform the scope transition, so we get error messages on the node contents.
                    return default;
                }

                // Assign the new context.
                var context = new Context( this, declaredSymbol );
                this._currentScope = scope;
                this._currentTypeScope = typeScope;
                this._currentDeclaration = declaredSymbol;
                this._currentDeclarationTemplateType = templateType;

                return context;
            }

            private readonly struct Context : IDisposable
            {
                private readonly Visitor? _parent;
                private readonly TemplatingScope? _previousTypeScope;
                private readonly TemplatingScope? _previousScope;
                private readonly TemplateAttributeType? _previousDeclarationTemplateType;
                private readonly ISymbol? _previousDeclaration;

                public Context( Visitor parent, ISymbol? declaredSymbol )
                {
                    this._parent = parent;
                    this._previousTypeScope = parent._currentTypeScope;
                    this._previousScope = parent._currentScope;
                    this._previousDeclarationTemplateType = parent._currentDeclarationTemplateType;
                    this._previousDeclaration = parent._currentDeclaration;
                    this.DeclaredSymbol = declaredSymbol;
                }

                public ISymbol? DeclaredSymbol { get; }

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