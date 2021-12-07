using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

#pragma warning disable CS0169, CS8618

namespace Caravela.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.Bug28810
{
    internal class DeepCloneAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            var typedMethod = builder.Advices.IntroduceMethod( builder.Target, nameof(CloneImpl) );
            typedMethod.Name = "Clone";
            typedMethod.ReturnType = builder.Target;

            builder.Advices.ImplementInterface( builder.Target, typeof(ICloneable), whenExists: OverrideStrategy.Ignore );
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
            private NaturallyCloneable b;
        }
    }
}