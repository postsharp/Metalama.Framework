using Caravela.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Transformations
{
    abstract class Transformation
    {
    }

    abstract class OverriddenElement : Transformation
    {
        public abstract ICodeElement OverriddenDeclaration { get; }
    }

    class OverriddenMethod : OverriddenElement
    {
        public override ICodeElement OverriddenDeclaration { get; }
        public BlockSyntax MethodBody { get; }

        public OverriddenMethod( IMethod overriddenDeclaration, BlockSyntax methodBody )
        {
            this.OverriddenDeclaration = overriddenDeclaration;
            this.MethodBody = methodBody;
        }
    }
}
