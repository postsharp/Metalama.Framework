using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating
{
    // TODO: This is a temporary implementation of ITemplateExpansionContext.
    internal class TemplateExpansionContext : ITemplateExpansionContext
    {
        private readonly IMethod _targetMethod;

        public TemplateExpansionContext( 
            object templateInstance,
            IMethod targetMethod, 
            ICompilation compilation,
            IProceedImpl proceedImpl,
            ITemplateExpansionLexicalScope lexicalScope,
            DiagnosticSink diagnosticSink )
        {
            this.TemplateInstance = templateInstance;
            this._targetMethod = targetMethod;
            this.Compilation = compilation;
            this.ProceedImplementation = proceedImpl;
            this.CurrentLexicalScope = lexicalScope;
            this.DiagnosticSink = diagnosticSink;

            Invariant.Assert( diagnosticSink.DefaultLocation != null, "diagnosticSink.DefaultLocation cannot be null" );
            Invariant.Assert( 
                diagnosticSink.DefaultLocation!.Equals( targetMethod.DiagnosticLocation ), 
                "the default location of the DiagnosticSink must be equal to targetMethod");
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

            if ( this._targetMethod.ReturnType.Is( typeof(void) ) )
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

        public IDisposable OpenNestedScope()
        {
            var nestedScope = this.CurrentLexicalScope.OpenNestedScope();
            var cookie = new LexicalScopeCookie(this, this.CurrentLexicalScope, nestedScope);
            this.CurrentLexicalScope = nestedScope;
            return cookie;
        }

        public DiagnosticSink DiagnosticSink { get; }

        private class LexicalScopeCookie : IDisposable
        {
            private readonly TemplateExpansionContext _context;
            private readonly ITemplateExpansionLexicalScope _previousScope;
            private readonly ITemplateExpansionLexicalScope _newScope;

            public LexicalScopeCookie(TemplateExpansionContext context, ITemplateExpansionLexicalScope previousScope, ITemplateExpansionLexicalScope newScope )
            {
                this._context = context;
                this._previousScope = previousScope;
                this._newScope = newScope;
            }

            public void Dispose()
            {
                this._context.CurrentLexicalScope = this._previousScope;
            }
        }
    }
}