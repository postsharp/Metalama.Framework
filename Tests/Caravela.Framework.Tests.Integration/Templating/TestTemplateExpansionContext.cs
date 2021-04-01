// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Tests.Integration.Templating
{
    internal class TestTemplateExpansionContext : ITemplateExpansionContext, IDisposable
    {
        private readonly IMethod _targetMethod;
        private readonly DiagnosticList _diagnostics;
        private TestLexicalScope _currentScope;

        public TestTemplateExpansionContext( Assembly assembly, CompilationModel compilation )
        {
            var roslynCompilation = compilation.RoslynCompilation;

            this.Compilation = compilation;

            var templateType = assembly.GetTypes().Single( t => t.Name.Equals( "Aspect", StringComparison.Ordinal ) );
            this.TemplateInstance = Activator.CreateInstance( templateType )!;

            var targetType = assembly.GetTypes().Single( t => t.Name.Equals( "TargetCode", StringComparison.Ordinal ) );
            var targetCaravelaType = compilation.Factory.GetTypeByReflectionName( targetType.FullName! )!;
            this._targetMethod = targetCaravelaType.Methods.Single( m => m.Name == "Method" );

            this._diagnostics = new DiagnosticList( this._targetMethod.DiagnosticLocation );

            var roslynTargetType = roslynCompilation.Assembly.GetTypes().Single( t => t.Name.Equals( "TargetCode", StringComparison.Ordinal ) );
            var roslynTargetMethod = (BaseMethodDeclarationSyntax) roslynTargetType.GetMembers()
                .Single( m => m.Name == "Method" )
                .DeclaringSyntaxReferences
                .Select( r => (CSharpSyntaxNode) r.GetSyntax() )
                .Single();

            var semanticModel = compilation.RoslynCompilation.GetSemanticModel( compilation.RoslynCompilation.SyntaxTrees[0] );
            var roslynTargetMethodSymbol = semanticModel.GetDeclaredSymbol( roslynTargetMethod );
            if ( roslynTargetMethodSymbol == null )
            {
                throw new InvalidOperationException( "The symbol of the target method was not found." );
            }

            this._targetMethod = compilation.Factory.GetMethod( roslynTargetMethodSymbol );

            this.ProceedImplementation = new TestProceedImpl( roslynTargetMethod );
            this._currentScope = new TestLexicalScope( this, semanticModel, roslynTargetMethod );
        }

        public ICodeElement TargetDeclaration => this._targetMethod;

        public object TemplateInstance { get; }

        public IProceedImpl ProceedImplementation { get; }

        public ICompilation Compilation { get; }

        public ITemplateExpansionLexicalScope CurrentLexicalScope => this._currentScope;

        DiagnosticSink ITemplateExpansionContext.DiagnosticSink => this._diagnostics;

        public StatementSyntax CreateReturnStatement( ExpressionSyntax? returnExpression )
        {
            if ( returnExpression == null )
            {
                return ReturnStatement();
            }

            if ( this._targetMethod.ReturnType.Is( typeof( void ) ) )
            {
                return ReturnStatement();
            }

            var returnExpressionKind = returnExpression.Kind();
            if ( returnExpressionKind == SyntaxKind.DefaultLiteralExpression || returnExpressionKind == SyntaxKind.NullLiteralExpression )
            {
                return ReturnStatement( returnExpression );
            }

            return ReturnStatement( CastExpression( ParseTypeName( this._targetMethod.ReturnType.ToDisplayString() ), returnExpression ) );
        }

        public void Dispose()
        {
        }

        public IDisposable OpenNestedScope()
        {
            return (IDisposable) this._currentScope.OpenNestedScope();
        }

        private class TestLexicalScope : ITemplateExpansionLexicalScope, IDisposable
        {
            private readonly Dictionary<string, string> _templateToTargetIdentifiersMap = new Dictionary<string, string>();
            private readonly HashSet<string> _definedIdentifiers = new HashSet<string>();
            private readonly TestLexicalScope? _parentScope;
            private readonly List<TestLexicalScope> _nestedScopes = new List<TestLexicalScope>();
            private readonly TestTemplateExpansionContext _expansionContext;

            public ITemplateExpansionLexicalScope? Parent => this._parentScope;

            public IReadOnlyCollection<string> DefinedIdentifiers => this._definedIdentifiers;

            public IReadOnlyList<ITemplateExpansionLexicalScope> NestedScopes => this._nestedScopes;

            public TestLexicalScope( TestTemplateExpansionContext expansionContext, SemanticModel semanticModel, BaseMethodDeclarationSyntax targetMethodSyntax )
            {
                this._expansionContext = expansionContext;
                this._parentScope = null;

                var lookupPosition = targetMethodSyntax.Body != null ? targetMethodSyntax.Body.SpanStart : targetMethodSyntax.SpanStart;
                var visibleSymbols = semanticModel.LookupSymbols( lookupPosition );

                foreach ( var symbolName in visibleSymbols.Select( s => s.Name ) )
                {
                    this._definedIdentifiers.Add( symbolName );
                }
            }

            private TestLexicalScope( TestLexicalScope parentScope )
            {
                this._expansionContext = parentScope._expansionContext;
                this._parentScope = parentScope;
            }

            public void Dispose()
            {
                if ( this._parentScope != null )
                {
                    this._expansionContext._currentScope = this._parentScope;
                }
            }

            public string DefineIdentifier( string name )
            {
                var targetName = name;
                var i = 0;
                while ( this.IsDefineable( targetName ) )
                {
                    i++;
                    targetName = $"{name}_{i}";
                }

                this._definedIdentifiers.Add( targetName );
                this._templateToTargetIdentifiersMap[name] = targetName;

                return targetName;
            }

            public string LookupIdentifier( string name )
            {
                if ( this._templateToTargetIdentifiersMap.TryGetValue( name, out var targetName ) )
                {
                    return targetName;
                }

                if ( this._parentScope != null )
                {
                    return this._parentScope.LookupIdentifier( name );
                }

                return name;
            }

            public ITemplateExpansionLexicalScope OpenNestedScope()
            {
                var nestedScope = new TestLexicalScope( this );
                this._nestedScopes.Add( nestedScope );
                this._expansionContext._currentScope = nestedScope;
                return nestedScope;
            }

            public bool IsDefined( string name, bool includeAncestorScopes = true )
            {
                if ( includeAncestorScopes )
                {
                    return this._definedIdentifiers.Contains( name ) || this.IsDefinedInParent( name );
                }
                else
                {
                    return this._definedIdentifiers.Contains( name );
                }
            }

            private bool IsDefinedInParent( string name )
            {
                if ( this._parentScope == null )
                {
                    return false;
                }

                return this._parentScope._definedIdentifiers.Contains( name ) || this._parentScope.IsDefinedInParent( name );
            }
        }
    }
}
