using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Transformations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.Advices
{
    internal class IntroduceMethodAdvice : IIntroductionAdvice
    {
        public INamedType TargetDeclaration { get; }
        public IntroducedMethod Transformation { get; }

        public IntroduceMethodAdvice(INamedType targetDeclaration, IntroducedMethod transformation )
        {
            this.TargetDeclaration = targetDeclaration;
            this.Transformation = transformation;
        }
    }
}
