// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
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
        private sealed class Visitor : SafeSyntaxWalker, IDiagnosticAdder
        {
            private readonly ISymbolClassifier _classifier;
            private readonly HashSet<ISymbol> _alreadyReportedDiagnostics = new( SymbolEqualityComparer.Default );
            private readonly bool _reportCompileTimeTreeOutdatedError;
            private readonly bool _isDesignTime;
            private readonly ProjectServiceProvider _serviceProvider;
            private readonly SemanticModel _semanticModel;
            private readonly CompilationContext _compilationContext;
            private readonly Action<Diagnostic> _reportDiagnostic;
            private readonly CancellationToken _cancellationToken;
            private readonly bool _hasCompileTimeCodeFast;
            private TemplateCompiler? _templateCompiler;

            private ISymbol? _currentDeclaration;
            private TemplatingScope? _currentScope;
            private TemplatingScope? _currentTypeScope;
            private TemplateInfo? _currentTemplateInfo;

            public bool HasError { get; private set; }

            public Visitor(
                ProjectServiceProvider serviceProvider,
                SemanticModel semanticModel,
                CompilationContext compilationContext,
                Action<Diagnostic> reportDiagnostic,
                bool reportCompileTimeTreeOutdatedError,
                bool isDesignTime,
                CancellationToken cancellationToken )
            {
                this._serviceProvider = serviceProvider;
                this._semanticModel = semanticModel;
                this._compilationContext = compilationContext;
                this._reportDiagnostic = reportDiagnostic;
                this._classifier = compilationContext.SymbolClassifier;
                this._reportCompileTimeTreeOutdatedError = reportCompileTimeTreeOutdatedError;
                this._isDesignTime = isDesignTime;
                this._cancellationToken = cancellationToken;
                this._hasCompileTimeCodeFast = CompileTimeCodeFastDetector.HasCompileTimeCode( semanticModel.SyntaxTree.GetRoot() );
            }

            private bool IsInTemplate => this._currentTemplateInfo is { AttributeType: not TemplateAttributeType.None };

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

                if ( node is null or IdentifierNameSyntax { IsVar: true } )
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
                            ContainingSymbol.Name: nameof(meta),
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
                using var context = this.WithDeclaration( node );

                this.VerifyTypeDeclaration( node, context );
                base.VisitClassDeclaration( node );
            }

            public override void VisitStructDeclaration( StructDeclarationSyntax node )
            {
                using var context = this.WithDeclaration( node );

                this.VerifyTypeDeclaration( node, context );
                base.VisitStructDeclaration( node );
            }

            public override void VisitRecordDeclaration( RecordDeclarationSyntax node )
            {
                using var context = this.WithDeclaration( node );

                this.VerifyTypeDeclaration( node, context );

                base.VisitRecordDeclaration( node );
            }

            public override void VisitInterfaceDeclaration( InterfaceDeclarationSyntax node )
            {
                using var context = this.WithDeclaration( node );

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

                this.VerifyModifiers( node.Modifiers );

                // Verify that the base class and implemented interfaces are scope-compatible.
                // If the scope is conflict, an error message is written elsewhere.

                if ( node.BaseList != null && this._currentScope != TemplatingScope.Conflict )
                {
                    foreach ( var baseTypeNode in node.BaseList.Types )
                    {
                        var baseType = (INamedTypeSymbol?) ModelExtensions.GetSymbolInfo( this._semanticModel, baseTypeNode.Type ).Symbol;

                        if ( baseType == null )
                        {
                            continue;
                        }

                        var baseTypeScope = this._classifier.GetTemplatingScope( baseType );

                        if ( baseTypeScope is TemplatingScope.Conflict )
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
                                (TemplatingScope.RunTimeOnly, TemplatingScope.DynamicTypeConstruction) => true,
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

                // Check that an aspect is not a nested type
                if ( node.Parent is BaseTypeDeclarationSyntax )
                {
                    var declaredTypeSymbol = (INamedTypeSymbol) context.DeclaredSymbol!;
                    var iAspectSymbol = this._compilationContext.ReflectionMapper.GetTypeSymbol( typeof( IAspect ) );

                    if ( declaredTypeSymbol.Is( iAspectSymbol ) )
                    {
                        this.Report(
                            TemplatingDiagnosticDescriptors.AspectNestedTypeForbidden.CreateRoslynDiagnostic(
                                node.Identifier.GetLocation(),
                                declaredTypeSymbol ) );
                    }
                }
            }

            private void VerifyModifiers( SyntaxTokenList modifiers )
            {
                // Forbid unsafe compile-time code.
                var unsafeKeyword = modifiers.FirstOrDefault( m => m.IsKind( SyntaxKind.UnsafeKeyword ) );

                if ( unsafeKeyword.IsKind( SyntaxKind.UnsafeKeyword ) )
                {
                    if ( this._currentTemplateInfo is { IsNone: false } )
                    {
                        this.Report(
                            TemplatingDiagnosticDescriptors.UnsafeCodeForbiddenInTemplate.CreateRoslynDiagnostic(
                                unsafeKeyword.GetLocation(),
                                this._currentDeclaration! ) );
                    }
                    else if ( this._currentScope != TemplatingScope.RunTimeOnly )
                    {
                        this.Report(
                            TemplatingDiagnosticDescriptors.UnsafeCodeForbiddenInCompileTimeCode.CreateRoslynDiagnostic(
                                unsafeKeyword.GetLocation(),
                                (this._currentDeclaration!, this._currentScope!.Value.ToDisplayString()) ) );
                    }
                }

                // Forbid partial templates.
                var partialKeyword = modifiers.FirstOrDefault( m => m.IsKind( SyntaxKind.PartialKeyword ) );

                if ( partialKeyword.IsKind( SyntaxKind.PartialKeyword ) )
                {
                    if ( this._currentTemplateInfo is { IsNone: false } )
                    {
                        this.Report(
                            TemplatingDiagnosticDescriptors.PartialTemplateMethodsForbidden.CreateRoslynDiagnostic(
                                partialKeyword.GetLocation(),
                                this._currentDeclaration! ) );
                    }
                }
            }

            public override void VisitMethodDeclaration( MethodDeclarationSyntax node )
                => this.VisitBaseMethodOrAccessor(
                    node,
                    node.Modifiers,
                    syntax => base.VisitMethodDeclaration( syntax ) );

            public override void VisitAccessorDeclaration( AccessorDeclarationSyntax node )
                => this.VisitBaseMethodOrAccessor(
                    node,
                    node.Modifiers,
                    syntax => base.VisitAccessorDeclaration( syntax ) );

            private void VisitBaseMethodOrAccessor<T>( T node, SyntaxTokenList modifiers, Action<T> visitBase )
                where T : SyntaxNode
            {
                using ( this.WithDeclaration( node ) )
                {
                    this.VerifyModifiers( modifiers );

                    if ( this.IsInTemplate )
                    {
                        if ( this._isDesignTime )
                        {
                            this._templateCompiler ??= new TemplateCompiler( this._serviceProvider, this._compilationContext );
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
                using ( this.WithDeclaration( node ) )
                {
                    this.VerifyModifiers( node.Modifiers );
                    base.VisitPropertyDeclaration( node );
                }
            }

            public override void VisitConstructorDeclaration( ConstructorDeclarationSyntax node )
            {
                using ( this.WithDeclaration( node ) )
                {
                    this.VerifyModifiers( node.Modifiers );
                    base.VisitConstructorDeclaration( node );
                }
            }

            public override void VisitOperatorDeclaration( OperatorDeclarationSyntax node )
            {
                using ( this.WithDeclaration( node ) )
                {
                    this.VerifyModifiers( node.Modifiers );
                    base.VisitOperatorDeclaration( node );
                }
            }

            public override void VisitEventDeclaration( EventDeclarationSyntax node )
            {
                using ( this.WithDeclaration( node ) )
                {
                    this.VerifyModifiers( node.Modifiers );
                    base.VisitEventDeclaration( node );
                }
            }

            public override void VisitEventFieldDeclaration( EventFieldDeclarationSyntax node )
            {
                foreach ( var f in node.Declaration.Variables )
                {
                    using ( this.WithDeclaration( f ) )
                    {
                        this.VerifyModifiers( node.Modifiers );
                        this.Visit( node.Declaration.Type );
                        this.VisitVariableDeclarator( f );
                    }
                }
            }

            public override void VisitFieldDeclaration( FieldDeclarationSyntax node )
            {
                foreach ( var f in node.Declaration.Variables )
                {
                    using ( this.WithDeclaration( f ) )
                    {
                        this.VerifyModifiers( node.Modifiers );
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

            private Context WithDeclaration( SyntaxNode node )
            {
                // Reset deduplication.
                this._alreadyReportedDiagnostics.Clear();

                // Get the scope.
                var declaredSymbol = this._semanticModel.GetDeclaredSymbol( node );

                if ( declaredSymbol == null )
                {
                    return default;
                }

                var scope = this._classifier.GetTemplatingScope( declaredSymbol );

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

                // Get the template info.
                var templateInfo = this._currentTemplateInfo;

                if ( templateInfo == null || templateInfo.IsNone )
                {
                    templateInfo = this._classifier.GetTemplateInfo( declaredSymbol );
                }

                // Report error on conflict scope.
                if ( scope == TemplatingScope.Conflict )
                {
                    this._classifier.ReportScopeError( node, declaredSymbol, this );
                }

                // Report error when we have compile-time code but no namespace import for fast detection.
                if ( scope != TemplatingScope.RunTimeOnly && !this._hasCompileTimeCodeFast
                                                          && !SystemTypeDetector.IsSystemType(
                                                              declaredSymbol as INamedTypeSymbol ?? declaredSymbol.ContainingType ) )
                {
                    var attributeName = scope == TemplatingScope.RunTimeOrCompileTime ? nameof(RunTimeOrCompileTimeAttribute) : nameof(CompileTimeAttribute);

                    this.Report(
                        TemplatingDiagnosticDescriptors.CompileTimeCodeNeedsNamespaceImport.CreateRoslynDiagnostic(
                            declaredSymbol.GetDiagnosticLocation(),
                            (declaredSymbol, CompileTimeCodeFastDetector.Namespace, attributeName) ) );
                }

                // Check that 'dynamic' is used only in a template or in run-time-only code.
                if ( scope is TemplatingScope.Dynamic or TemplatingScope.DynamicTypeConstruction && typeScope != TemplatingScope.RunTimeOnly
                                                                                                 && templateInfo.IsNone )
                {
                    this.Report(
                        TemplatingDiagnosticDescriptors.OnlyNamedTemplatesCanHaveDynamicSignature.CreateRoslynDiagnostic(
                            declaredSymbol.GetDiagnosticLocation(),
                            (declaredSymbol, declaredSymbol.ContainingType, typeScope!.Value) ) );
                }

                // Check that run-time members are contained in run-time types.
                if ( scope == TemplatingScope.RunTimeOnly && typeScope != TemplatingScope.RunTimeOnly && templateInfo.IsNone )
                {
                    // If we have an illegal run-time scope, we don't perform the scope transition, so we get error messages on the node contents.
                    return default;
                }

                // Assign the new context.
                var context = new Context( this, declaredSymbol );
                this._currentScope = scope;
                this._currentTypeScope = typeScope;
                this._currentDeclaration = declaredSymbol;
                this._currentTemplateInfo = templateInfo;

                return context;
            }

            private readonly struct Context : IDisposable
            {
                private readonly Visitor? _parent;
                private readonly TemplatingScope? _previousTypeScope;
                private readonly TemplatingScope? _previousScope;
                private readonly TemplateInfo? _previousTemplateInfo;
                private readonly ISymbol? _previousDeclaration;

                public Context( Visitor parent, ISymbol? declaredSymbol )
                {
                    this._parent = parent;
                    this._previousTypeScope = parent._currentTypeScope;
                    this._previousScope = parent._currentScope;
                    this._previousTemplateInfo = parent._currentTemplateInfo;
                    this._previousDeclaration = parent._currentDeclaration;
                    this.DeclaredSymbol = declaredSymbol;
                }

                public ISymbol? DeclaredSymbol { get; }

                public void Dispose()
                {
                    if ( this._parent != null )
                    {
                        this._parent._currentScope = this._previousScope;
                        this._parent._currentTemplateInfo = this._previousTemplateInfo;
                        this._parent._currentDeclaration = this._previousDeclaration;
                        this._parent._currentTypeScope = this._previousTypeScope;
                    }
                }
            }
        }
    }
}