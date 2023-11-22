using Metalama.Framework.Aspects;

#pragma warning disable CS0169

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Overrides.Fields.BaseInvoker
{
    class Aspect : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        { 
            get => meta.Target.FieldOrProperty.Value;
            set => meta.Target.FieldOrProperty.Value = value;
        }
    }

    // <target>
    class TargetCode
    {
        [Aspect]
        int field;        
    }
}