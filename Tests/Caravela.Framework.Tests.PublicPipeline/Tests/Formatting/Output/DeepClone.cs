using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Code.Syntax;
using Caravela.Framework.Diagnostics;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable CS0067
#pragma warning disable CS0169

namespace Caravela.Framework.Tests.Integration.Tests.Formatting.Output
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
            IExpression baseCall;
          
            if (!meta.Target.Method.IsOverride)
            {
                meta.DefineExpression( meta.Base.MemberwiseClone(), out baseCall);
            }
            else
            {
                meta.DefineExpression( meta.Target.Method.Invokers.Base.Invoke(meta.This), out baseCall);
            }
            
            var clone = meta.Cast(meta.Target.Type, baseCall);

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
