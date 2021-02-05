using Caravela.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Transformations
{
    internal abstract class Transformation
    {
    }

    internal abstract class OverriddenElement : Transformation
    {
        public abstract ICodeElement OverriddenDeclaration { get; }
    }

    internal class OverriddenMethod : OverriddenElement
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
