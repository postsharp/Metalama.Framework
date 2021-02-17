using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.TestFramework.Templating
{
    internal class TestTemplateExpansionContext : ITemplateExpansionContext
    {
        private readonly IMethod _targetMethod;

        public TestTemplateExpansionContext( Assembly assembly, SourceCompilation compilation )
        {
            this.Compilation = compilation;

            var aspectType = assembly.GetTypes().Single( t => t.Name.Equals( "Aspect", StringComparison.Ordinal ) );
            this.TemplateInstance = Activator.CreateInstance( aspectType )!;

            var targetType = assembly.GetTypes().Single( t => t.Name.Equals( "TargetCode", StringComparison.Ordinal ) );
            var targetCaravelaType = compilation.GetTypeByReflectionName( targetType.FullName! )!;
            this._targetMethod = targetCaravelaType.Methods.GetValue().Single( m => m.Name == "Method" );

            var roslynCompilation = compilation.RoslynCompilation;
            var roslynType = roslynCompilation.GetTypes().Single( t => t.Name.Equals( "TargetCode", StringComparison.Ordinal ) );
            var roslynMethod = (BaseMethodDeclarationSyntax) roslynType.GetMembers()
                .Single( m => m.Name == "Method" )
                .DeclaringSyntaxReferences
                .Select( r => (CSharpSyntaxNode) r.GetSyntax() )
                .Single();

            this.ProceedImplementation = new TestProceedImpl( roslynMethod );
            this.CurrentLexicalScope = new TemplateTestLexicalScope( this, (IMethodInternal) this._targetMethod );
        }

        public ICodeElement TargetDeclaration => this._targetMethod;

        public object TemplateInstance { get; }

        public IProceedImpl ProceedImplementation { get; }

        public ICompilation Compilation { get; }

        public ITemplateExpansionLexicalScope CurrentLexicalScope { get; private set; }

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

        private class TemplateTestLexicalScope : ITemplateExpansionLexicalScope
        {
            private readonly Dictionary<string, string> _templateToTargetIdentifiersMap = new Dictionary<string, string>();
            private readonly HashSet<string> _definedIdentifiers = new HashSet<string>();
            private readonly TemplateTestLexicalScope? _parentScope;
            private readonly List<TemplateTestLexicalScope> _nestedScopes = new List<TemplateTestLexicalScope>();
            private readonly TestTemplateExpansionContext _expansionContext;

            public TemplateTestLexicalScope( TestTemplateExpansionContext expansionContext, IMethodInternal methodInternal )
            {
                this._expansionContext = expansionContext;
                this._parentScope = null;

                foreach ( var symbolName in methodInternal.LookupSymbols().Select( s => s.Name ) )
                {
                    this._definedIdentifiers.Add( symbolName );
                }
            }

            private TemplateTestLexicalScope( TemplateTestLexicalScope parentScope )
            {
                this._expansionContext = parentScope._expansionContext;
                this._parentScope = parentScope;
            }

            public void Dispose()
            {
                if ( this._parentScope != null )
                {
                    this._expansionContext.CurrentLexicalScope = this._parentScope;
                }
            }

            public string DefineIdentifier( string name )
            {
                var targetName = name;
                var i = 0;
                while ( this.IsDefined( targetName ) )
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
                var nestedScope = new TemplateTestLexicalScope( this );
                this._nestedScopes.Add( nestedScope );
                this._expansionContext.CurrentLexicalScope = nestedScope;
                return nestedScope;
            }

            private bool IsDefined( string name )
            {
                return this._definedIdentifiers.Contains( name ) || this.IsDefinedInParent( name ) || this.IsDefinedInNested( name );
            }

            private bool IsDefinedInParent( string name )
            {
                if ( this._parentScope == null )
                {
                    return false;
                }

                return this._parentScope._definedIdentifiers.Contains( name ) || this._parentScope.IsDefinedInParent( name );
            }

            private bool IsDefinedInNested( string name )
            {
                foreach ( var nestedScope in this._nestedScopes )
                {
                    if ( nestedScope._definedIdentifiers.Contains( name ) || nestedScope.IsDefinedInNested( name ) )
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
