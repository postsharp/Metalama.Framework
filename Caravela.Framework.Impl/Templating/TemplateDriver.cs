using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating.MetaModel;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating
{
    public class TemplateDriver
    {
        private readonly MethodInfo _templateMethod;

        public TemplateDriver( MethodInfo templateMethodInfo ) => this._templateMethod = templateMethodInfo;

        public BlockSyntax ExpandDeclaration( object templateInstance, IMethod targetMethod, ICompilation compilation )
        {
            TemplateContext.ProceedImpl = new ProceedImpl( (BaseMethodDeclarationSyntax) targetMethod.GetSyntaxNode() );
            TemplateContext.target = new TemplateContextImpl( targetMethod, targetMethod.DeclaringType!, compilation );

            var output = (SyntaxNode) this._templateMethod.Invoke( templateInstance, null );
            var result = (BlockSyntax) new FlattenBlocksRewriter().Visit( output );

            TemplateContext.ProceedImpl = null;
            TemplateContext.target = null;

            return result;
        }
    }
}
