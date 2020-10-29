using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Caravela.Framework.Impl.Templating
{
    public class TemplateDriver
    {
        MethodInfo templateMethod;

        public TemplateDriver( MethodInfo templateMethodInfo )
        {
            this.templateMethod = templateMethodInfo;
        }

        public SyntaxNode ExpandDeclaration( object templateInstance, ITemplateExpansionContext context )
        {
            var output = (SyntaxNode) this.templateMethod.Invoke( templateInstance, null );
            return new FlattenBlocksRewriter().Visit( output );
        }
    }
}
