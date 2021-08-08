using Caravela.Framework.Aspects;
using Caravela.Framework.Code.Syntax;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Serialization
{
    internal class DynamicSyntaxSerializer : ObjectSerializer<ISyntaxBuilder>
    {
        public DynamicSyntaxSerializer( SyntaxSerializationService service ) : base( service ) { }

        public override ExpressionSyntax Serialize( ISyntaxBuilder obj, ICompilationElementFactory syntaxFactory )
            => ((IDynamicExpression) obj.ToSyntax()).CreateExpression();

        public override Type? OutputType => null;
    }
}