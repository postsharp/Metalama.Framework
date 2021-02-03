using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.Advices
{
    abstract class Advice : IAdvice
    {
        public IAspect Aspect { get; }
        public ICodeElement TargetDeclaration { get; }


        protected Advice( IAspect aspect, ICodeElement targetDeclaration )
        {
            this.Aspect = aspect;
            this.TargetDeclaration = targetDeclaration;
        }
    }
}
