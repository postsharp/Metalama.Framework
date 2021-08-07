using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable CS0067
#pragma warning disable CS0169

namespace Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty
{
    class DeepCloneAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var typedMethod = builder.AdviceFactory.IntroduceMethod(
                builder.Target,
                nameof(CloneImpl),
                whenExists: OverrideStrategy.Override);

            typedMethod.Name = "Clone";
            typedMethod.ReturnType = builder.Target;

            builder.AdviceFactory.ImplementInterface(
                builder.Target,
                typeof(ICloneable),
                whenExists: OverrideStrategy.Ignore);
        }

        [Template(IsVirtual = true)]
        public virtual dynamic CloneImpl()
        {
            // Define a local variable of the same type as the target type.
            var clone = meta.Target.Type.DefaultValue();

            if (meta.Target.Method.Invokers.Base == null)
            {
                // Invoke base.MemberwiseClone().
                clone = meta.Cast(meta.Target.Type, meta.Base.MemberwiseClone());
            }
            else
            {
                // Invoke the base method.
                clone = meta.Cast(meta.Target.Type, meta.Target.Method.Invokers.Base.Invoke(meta.This));
            }

            // Select clonable fields.
            var clonableFields =
                meta.Target.Type.FieldsAndProperties.Where(
                    f => f.IsAutoPropertyOrField &&
                    (f.Type.Is(typeof(ICloneable)) ||
                    (f.Type is INamedType fieldNamedType && fieldNamedType.Aspects<DeepCloneAttribute>().Any())));

            foreach (var field in clonableFields)
            {
                field.Invokers.Final.SetValue(
                    clone,
                    meta.Cast(field.Type, ((ICloneable)field.Invokers.Final.GetValue(meta.This)).Clone()));
            }

            return clone;
        }

        [InterfaceMember(IsExplicit = true)]
        object Clone()
        {
            return meta.This.Clone();
        }
    }

    
    class ManuallyCloneable : ICloneable
    {
        public object Clone()
        {
            return new ManuallyCloneable();
        }
    }

    // <target>
    class Targets
    {
        [DeepClone]
        class AutomaticallyCloneable
        {
            int a;

            ManuallyCloneable? b;

            AutomaticallyCloneable? c;
        }

        [DeepClone]
        class Derived : AutomaticallyCloneable
        {
            private string d;
        }
    }
}
