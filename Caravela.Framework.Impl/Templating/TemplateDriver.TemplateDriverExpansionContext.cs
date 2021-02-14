using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating
{
    internal partial class TemplateDriver
    {
        // TODO: This is a temporary implementation of ITemplateExpansionContext.
        private class TemplateDriverExpansionContext : ITemplateExpansionContext
        {
            private readonly IMethod _targetMethod;

            public TemplateDriverExpansionContext( object templateInstance, IMethod targetMethod, ICompilation compilation )
            {
                this.TemplateInstance = templateInstance;
                this._targetMethod = targetMethod;
                this.Compilation = compilation;
                this.ProceedImplementation = new ProceedInvokeMethod( targetMethod );
                this.CurrentLexicalScope = new TemplateDriverLexicalScope( this, (IMethodInternal) targetMethod );
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

                // TODO: validate the returnExpression according to the method's return type.
                return ReturnStatement( CastExpression( ParseTypeName( this._targetMethod.ReturnType.ToDisplayString() ), returnExpression ) );
            }

            private class TemplateDriverLexicalScope : ITemplateExpansionLexicalScope
            {
                private readonly Dictionary<string, string> _templateToTargetIdentifiersMap = new Dictionary<string, string>();
                private readonly HashSet<string> _definedIdentifiers = new HashSet<string>();
                private readonly TemplateDriverLexicalScope? _parentScope;
                private readonly List<TemplateDriverLexicalScope> _nestedScopes = new List<TemplateDriverLexicalScope>();
                private readonly TemplateDriverExpansionContext _expansionContext;

                public TemplateDriverLexicalScope( TemplateDriverExpansionContext expansionContext, IMethodInternal methodInternal )
                {
                    this._expansionContext = expansionContext;
                    this._parentScope = null;

                    foreach ( var symbolName in methodInternal.LookupSymbols().Select( s => s.Name ) )
                    {
                        this._definedIdentifiers.Add( symbolName );
                    }
                }

                private TemplateDriverLexicalScope( TemplateDriverLexicalScope parentScope )
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
                    var nestedScope = new TemplateDriverLexicalScope( this );
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
}
