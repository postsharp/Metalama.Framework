using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating
{
    internal partial class TemplateDriver
    {
        private readonly MethodInfo _templateMethod;

        public TemplateDriver( MethodInfo templateMethodInfo )
        {
            this._templateMethod = templateMethodInfo ?? throw new ArgumentNullException(nameof(templateMethodInfo));
        }

        [Obsolete( "Call a method with ITemplateExpansionContext parameter instead." )]
        public BlockSyntax ExpandDeclaration( object templateInstance, IMethod targetMethod, ICompilation compilation )
        {
            return this.ExpandDeclaration( new TemplateDriverExpansionContext( templateInstance, targetMethod, compilation ) );
        }

        public BlockSyntax ExpandDeclaration( ITemplateExpansionContext templateExpansionContext )
        {
            // TODO: support target declaration other than a method.
            if ( templateExpansionContext.TargetDeclaration is not IMethod )
            {
                throw new NotImplementedException();
            }

            var targetMethod = (IMethod) templateExpansionContext.TargetDeclaration;
            var templateContext = new TemplateContextImpl( targetMethod, targetMethod.DeclaringType!, templateExpansionContext.Compilation );

            TemplateContext.Initialize( templateContext, templateExpansionContext.ProceedImplementation );
            TemplateSyntaxFactory.Initialize( templateExpansionContext );

            SyntaxNode output;
            try
            {
                output = (SyntaxNode) this._templateMethod.Invoke( templateExpansionContext.TemplateInstance, null );
            }
            catch ( TargetInvocationException ex ) when ( ex.InnerException != null )
            {
                ExceptionDispatchInfo.Capture( ex.InnerException ).Throw();
                throw new Exception( "this line is unreachable, but is necessary to make the compiler happy" );
            }

            var result = (BlockSyntax) new FlattenBlocksRewriter().Visit( output );

            TemplateContext.Close();
            TemplateSyntaxFactory.Close();

            return result;
        }
    }
}
