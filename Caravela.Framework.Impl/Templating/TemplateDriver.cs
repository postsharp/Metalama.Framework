using System.Reflection;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating
{
    internal class TemplateDriver
    {
        private readonly MethodInfo _templateMethod;

        public TemplateDriver( MethodInfo templateMethodInfo ) => this._templateMethod = templateMethodInfo;

        public BlockSyntax ExpandDeclaration( object templateInstance, IMethod targetMethod, ICompilation compilation )
        {
            return this.ExpandDeclaration(
                templateInstance,
                new ProceedImpl( (BaseMethodDeclarationSyntax) targetMethod.GetSyntaxNode()! ),
                new TemplateContextImpl( targetMethod, targetMethod.DeclaringType!, compilation ) );
        }

        internal BlockSyntax ExpandDeclaration( object templateInstance, IProceedImpl proceedImpl, ITemplateContext templateContext )
        {
            TemplateContext.ProceedImpl = proceedImpl;
            TemplateContext.target = templateContext;
            TemplateContext.ExpansionContext = new TemplateDriverExpansionContext( this, templateContext );

            SyntaxNode output;
            try
            {
                output = (SyntaxNode) this._templateMethod.Invoke( templateInstance, null );
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture( ex.InnerException ).Throw();
                throw new Exception( "this line is unreachable, but is necessary to make the compiler happy" );
            }
            var result = (BlockSyntax) new FlattenBlocksRewriter().Visit( output );

            TemplateContext.ProceedImpl = null;
            TemplateContext.target = null;
            TemplateContext.ExpansionContext = null;

            return result;
        }

        // TODO temporary implementation of ITemplateExpansionContext before we support template nesting.
        private class TemplateDriverExpansionContext : ITemplateExpansionContext
        {
            private readonly TemplateDriver _templateDriver;
            private readonly ITemplateContext _templateContext;

            public TemplateDriverExpansionContext( TemplateDriver templateDriver, ITemplateContext templateContext )
            {
                this._templateContext = templateContext;
                this._templateDriver = templateDriver;
            }

            public StatementSyntax CreateReturnStatement( ExpressionSyntax? returnExpression )
            {
                if ( this._templateContext.Method.ReturnType.Is( typeof( void ) ) || returnExpression == null )
                {
                    return ReturnStatement();
                }

                var returnExpressionKind = returnExpression.Kind();
                if ( returnExpressionKind == SyntaxKind.DefaultLiteralExpression || returnExpressionKind == SyntaxKind.NullLiteralExpression )
                {
                    return ReturnStatement( returnExpression );
                }

                // TODO: validate the returnExpression according to the method's return type.
                // TODO: how to report diagnostics from the template invocation?
                // throw new CaravelaException(
                //    TemplatingDiagnosticDescriptors.ReturnTypeDoesNotMatch,
                //    this._templateDriver._templateMethod.Name, this._templateContext.Method.Name );
                return ReturnStatement( CastExpression( ParseTypeName( this._templateContext.Method.ReturnType.ToDisplayString() ), returnExpression ) );
            }
        }
    }
}
