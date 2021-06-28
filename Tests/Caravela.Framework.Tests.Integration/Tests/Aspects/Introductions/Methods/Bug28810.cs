using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

#pragma warning disable CS0169, CS8618

namespace Caravela.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.Bug28810
{
    class DeepCloneAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            var typedMethod = builder.AdviceFactory.IntroduceMethod(builder.TargetDeclaration, nameof(CloneImpl));
            typedMethod.Name = "Clone";
            typedMethod.ReturnType = builder.TargetDeclaration;

            builder.AdviceFactory.ImplementInterface(builder.TargetDeclaration, typeof(ICloneable), conflictBehavior: ConflictBehavior.Ignore);
        }

        [Template]
        public virtual dynamic? CloneImpl()
        {
            var baseMethod = meta.NamedType.Methods.OfExactSignature("Clone", 0, Array.Empty<IType>());

            return null;
        }

        [InterfaceMember( IsExplicit = true)]
        object Clone()
        {
            return meta.This.Clone();
        }
    }
    
    // <target>
    class Targets
    {    
        class NaturallyCloneable : ICloneable
        {
            public object Clone()
            {
                return  new NaturallyCloneable();
            }
        }
    
        [DeepClone]
        class BaseClass
        {
            int a;
            NaturallyCloneable b;
        }

    }
}