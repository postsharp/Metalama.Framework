using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0169, CS8618

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.Bug28810
{
    internal class DeepCloneAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            var typedMethod = builder.Advice.IntroduceMethod(
                builder.Target,
                nameof(CloneImpl),
                buildMethod: m =>
                {
                    m.Name = "Clone";
                    m.ReturnType = builder.Target;
                } );

            builder.Advice.ImplementInterface( builder.Target, typeof(ICloneable), whenExists: OverrideStrategy.Ignore );
        }

        [Template]
        public virtual dynamic? CloneImpl()
        {
            // This method does not do anything.
            var baseMethod = meta.Target.Type.Methods.OfExactSignature( "Clone", Array.Empty<IType>() );

            return null;
        }

        [InterfaceMember( IsExplicit = true )]
        private object Clone()
        {
            // This should call final version of introduced Clone method.
            return meta.This.Clone();
        }
    }

    // <target>
    internal class Targets
    {
        private class NaturallyCloneable : ICloneable
        {
            public object Clone()
            {
                return new NaturallyCloneable();
            }
        }

        [DeepClone]
        private class BaseClass
        {
            private int a;
            private NaturallyCloneable? b;
        }
    }
}