﻿using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl.Advices
{
    internal abstract class Advice : IAdvice
    {
        public AspectInstance Aspect { get; }

        IAspect IAdvice.Aspect => this.Aspect.Aspect;

        public ICodeElement TargetDeclaration { get; }

        public AspectPartId AspectPartId { get; }

        public string? PartName { get; set; }

        protected Advice( AspectInstance aspect, ICodeElement targetDeclaration )
        {
            this.Aspect = aspect;
            this.TargetDeclaration = targetDeclaration;
            this.AspectPartId = new AspectPartId( this.Aspect.AspectType, this.PartName );
        }

        public abstract AdviceResult ToResult( ICompilation compilation );
    }
    
    
}
