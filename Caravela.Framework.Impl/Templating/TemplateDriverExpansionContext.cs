using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating
{
    // TODO: This is a temporary implementation of ITemplateExpansionContext.
    internal class TemplateDriverExpansionContext : ITemplateExpansionContext
    {
        private readonly IMethod _targetMethod;
        //private readonly TemplateDriver _templateDriver;
        //private readonly ITemplateContext _templateContext;

        public TemplateDriverExpansionContext( object templateInstance, IMethod targetMethod, ICompilation compilation )
        {
            this.TemplateInstance = templateInstance;
            this._targetMethod = targetMethod;
            this.Compilation = compilation;
            //this._templateContext = templateContext;
            //this._templateDriver = templateDriver;
            this.ProceedImplementation = new ProceedImpl( (BaseMethodDeclarationSyntax) targetMethod.GetSyntaxNode() );
            this.CurrentLexicalScope = new TemplateDriverLexicalScope( this, (IMethodInternal) targetMethod );
        }

        public ICodeElement TargetDeclaration => this._targetMethod;
        public object TemplateInstance { get; }
        public IProceedImpl ProceedImplementation { get; }
        public ICompilation Compilation { get; }
        public ITemplateExpansionLexicalScope CurrentLexicalScope { get; private set; }

        public StatementSyntax CreateReturnStatement( ExpressionSyntax? returnExpression )
        {
            if ( returnExpression == null ) return ReturnStatement();
            if ( this._targetMethod.ReturnType.Is( typeof( void ) ) ) return ReturnStatement();

            var returnExpressionKind = returnExpression.Kind();
            if ( returnExpressionKind == SyntaxKind.DefaultLiteralExpression || returnExpressionKind == SyntaxKind.NullLiteralExpression )
            {
                return ReturnStatement( returnExpression );
            }

            // TODO: validate the returnExpression according to the method's return type.
            // TODO: how to report diagnostics from the template invocation?
            //throw new CaravelaException(
            //    TemplatingDiagnosticDescriptors.ReturnTypeDoesNotMatch,
            //    this._templateDriver._templateMethod.Name, this._templateContext.Method.Name );
            return ReturnStatement( CastExpression( ParseTypeName( this._targetMethod.ReturnType.ToDisplayString() ), returnExpression ) );
        }


        class TemplateDriverLexicalScope : ITemplateExpansionLexicalScope
        {
            private readonly Dictionary<string, string> _templateToTargetIdentifiersMap = new Dictionary<string, string>();
            private readonly HashSet<string> _definedIdentifiers;
            private readonly ITemplateExpansionLexicalScope? _parentScope;
            private readonly TemplateDriverExpansionContext _expansionContext;

            public TemplateDriverLexicalScope( TemplateDriverExpansionContext expansionContext, IMethodInternal methodInternal )
                : this( expansionContext )
            {
                foreach ( var symbolName in methodInternal.LookupSymbols().Select( s => s.Name ) )
                {
                    this._definedIdentifiers.Add( symbolName );
                }
            }

            private TemplateDriverLexicalScope( TemplateDriverExpansionContext expansionContext )
            {
                this._expansionContext = expansionContext;
                this._definedIdentifiers = new HashSet<string>();
                this._parentScope = expansionContext.CurrentLexicalScope;
            }

            public void Dispose()
            {
                if ( this._parentScope != null )
                {
                    this._expansionContext.CurrentLexicalScope = this._parentScope;
                }
            }

            public SyntaxToken DefineIdentifier( string name )
            {
                string targetName = name;
                int i = 0;
                while ( this._definedIdentifiers.Contains( targetName ) )
                {
                    i++;
                    targetName = $"{name}_{i}";
                }

                this._definedIdentifiers.Add( targetName );
                this._templateToTargetIdentifiersMap[name] = targetName;

                return Identifier( targetName );
            }

            public IdentifierNameSyntax CreateIdentifierName( string name )
            {
                string targetName;
                if ( !this._templateToTargetIdentifiersMap.TryGetValue( name, out targetName ) )
                {
                    targetName = name;
                }

                return IdentifierName( targetName );
            }

            public ITemplateExpansionLexicalScope OpenNestedScope()
            {
                var nestedScope = new TemplateDriverLexicalScope( this._expansionContext );
                this._expansionContext.CurrentLexicalScope = nestedScope;
                return nestedScope;
            }
        }
    }
}
