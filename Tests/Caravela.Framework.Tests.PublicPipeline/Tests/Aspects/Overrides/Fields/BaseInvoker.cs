using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

#pragma warning disable CS0169

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Overrides.Fields.BaseInvoker
{
    class Aspect : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        { 
            get => meta.Target.FieldOrProperty.Value;
            set => meta.Target.FieldOrProperty.Value = value;
        }
    }

    class TargetCode
    {

      
        [Aspect]
        int field;
        
    }
}