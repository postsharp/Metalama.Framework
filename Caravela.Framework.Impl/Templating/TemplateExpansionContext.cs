// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating
{
    // TODO: This is a temporary implementation of TemplateExpansionContext.

    internal class TemplateExpansionContext
    {
        private readonly IMethod _targetMethod;

        public TemplateExpansionLexicalScope LexicalScope { get; }

        public TemplateExpansionContext(
            object templateInstance,
            IMethod targetMethod,
            ICompilation compilation,
            IProceedImpl proceedImpl,
            TemplateExpansionLexicalScope lexicalScope,
            DiagnosticSink diagnosticSink )
        {
            this.TemplateInstance = templateInstance;
            this._targetMethod = targetMethod;
            this.Compilation = compilation;
            this.ProceedImplementation = proceedImpl;
            this.DiagnosticSink = diagnosticSink;
            this.LexicalScope = lexicalScope;
            Invariant.Assert( diagnosticSink.DefaultLocation != null );
            Invariant.Assert(
                diagnosticSink.DefaultLocation!.Equals( targetMethod.DiagnosticLocation ) );
        }

        public ICodeElement TargetDeclaration => this._targetMethod;

        public object TemplateInstance { get; }

        public IProceedImpl ProceedImplementation { get; }

        public ICompilation Compilation { get; }

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

        public DiagnosticSink DiagnosticSink { get; }
    }
}