using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Advices
{
    // TODO: switch to templating and move back to Caravela.Framework
    public class OverrideMethodAdvice : IAdvice<IMethod>
    {
        public IMethod TargetDeclaration { get; }
        public Func<BlockSyntax, BlockSyntax> Transformation { get; }

        public OverrideMethodAdvice( IMethod targetDeclaration, Func<BlockSyntax, BlockSyntax> transformation )
        {
            this.TargetDeclaration = targetDeclaration;
            this.Transformation = transformation;
        }
    }
}
