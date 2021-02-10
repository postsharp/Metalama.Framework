using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating
{
    internal partial class TemplateDriver
    {
        private readonly MethodInfo _templateMethod;

        public TemplateDriver( MethodInfo templateMethodInfo ) => this._templateMethod = templateMethodInfo;

        [Obsolete( "Call a method with ITemplateExpansionContext parameter instead." )]
        public BlockSyntax ExpandDeclaration( object templateInstance, IMethod targetMethod, ICompilation compilation )
        {
            return this.ExpandDeclaration( new TemplateDriverExpansionContext( templateInstance, targetMethod, compilation ) );
        }

        public BlockSyntax ExpandDeclaration( ITemplateExpansionContext templateExpansionContext )
        {
            // TODO: support target declaration other than a method.
            if ( templateExpansionContext.TargetDeclaration is not IMethod )
                throw new NotImplementedException();

            var targetMethod = (IMethod) templateExpansionContext.TargetDeclaration;

            TemplateContext.target = new TemplateContextImpl( targetMethod, targetMethod.DeclaringType!, templateExpansionContext.Compilation );
            TemplateContext.ProceedImpl = templateExpansionContext.ProceedImplementation;
            TemplateSyntaxFactory.ExpansionContext = templateExpansionContext;

            var output = (SyntaxNode) this._templateMethod.Invoke( templateExpansionContext.TemplateInstance, null );
            var result = (BlockSyntax) new FlattenBlocksRewriter().Visit( output );

            TemplateContext.target = null;
            TemplateContext.ProceedImpl = null;
            TemplateSyntaxFactory.ExpansionContext = null;

            return result;
        }
    }
}
