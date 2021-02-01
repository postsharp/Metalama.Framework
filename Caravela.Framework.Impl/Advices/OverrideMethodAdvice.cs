﻿using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Transformations;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Advices
{
    class OverrideMethodAdvice : IOverrideMethodAdvice, IAdviceImplementation
    {
        public IAspect Aspect { get; } 
        public IMethod TemplateMethod { get; }
        public IMethod TargetDeclaration { get; }

        public OverrideMethodAdvice( IAspect aspect, IMethod templateMethod, IMethod targetDeclaration )
        {
            this.Aspect = aspect;
            this.TemplateMethod = templateMethod;
            this.TargetDeclaration = targetDeclaration;
        }

        public IEnumerable<Transformation> GetTransformations( ICompilation compilation )
        {
            string templateMethodName = this.TemplateMethod.Name + TemplateCompiler.TemplateMethodSuffix;
            var methodBody = new TemplateDriver( this.Aspect.GetType().GetMethod( templateMethodName ) ).ExpandDeclaration( this.Aspect, this.TargetDeclaration, compilation );
            yield return new OverriddenMethod( this.TargetDeclaration, methodBody );
        }
    }
}
