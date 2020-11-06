using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating
{
    public class TemplateDriver
    {
        private readonly MethodInfo _templateMethod;

        public TemplateDriver( MethodInfo templateMethodInfo ) => this._templateMethod = templateMethodInfo;

        // used by AspectWorkbench
        public BlockSyntax ExpandDeclaration( object templateInstance ) => this.ExpandDeclaration( templateInstance, null!, null! );

        internal BlockSyntax ExpandDeclaration( object templateInstance, ProceedImpl proceed, TemplateContextImpl templateContext )
        {
            TemplateContext.ProceedImpl = proceed;
            TemplateContext.target = templateContext;

            var output = (SyntaxNode) this._templateMethod.Invoke( templateInstance, null );
            var result = (BlockSyntax) new FlattenBlocksRewriter().Visit( output );

            TemplateContext.ProceedImpl = null;
            TemplateContext.target = null;

            return result;
        }
    }
}
