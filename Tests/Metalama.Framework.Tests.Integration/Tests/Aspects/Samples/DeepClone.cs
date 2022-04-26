#if TEST_OPTIONS
// @RequiredConstant(NET5_0_OR_GREATER)
#endif

// In .NET Framework we get: Target runtime doesn't support covariant return types in overrides. Return type must be 'Targets.AutomaticallyCloneable'
// to match overridden member 'Targets.AutomaticallyCloneable.Clone()'`

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067
#pragma warning disable CS0169, CS8618, CS8602, CS8603

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty
{
    internal class DeepCloneAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            var typedMethod = builder.Advice.IntroduceMethod(
                builder.Target,
                nameof(CloneImpl),
                whenExists: OverrideStrategy.Override );

            typedMethod.Name = "Clone";
            typedMethod.ReturnType = builder.Target;

            builder.Advice.ImplementInterface(
                builder.Target,
                typeof(ICloneable),
                whenExists: OverrideStrategy.Ignore );
        }

        [Template( IsVirtual = true )]
        public virtual dynamic CloneImpl()
        {
            IExpression baseCall;

            if (!meta.Target.Method.IsOverride)
            {
                meta.DefineExpression( meta.Base.MemberwiseClone(), out baseCall );
            }
            else
            {
                meta.DefineExpression( meta.Target.Method.Invokers.Base.Invoke( meta.This ), out baseCall );
            }

            var clone = meta.Cast( meta.Target.Type, baseCall );

            // Select clonable fields.
            var clonableFields =
                meta.Target.Type.FieldsAndProperties.Where(
                    f => f.IsAutoPropertyOrField &&
                         ( f.Type.Is( typeof(ICloneable) ) ||
                           ( f.Type is INamedType fieldNamedType && fieldNamedType.Aspects<DeepCloneAttribute>().Any() ) ) );

            foreach (var field in clonableFields)
            {
                field.Invokers.Final.SetValue(
                    clone,
                    meta.Cast( field.Type, ( (ICloneable)field.Invokers.Final.GetValue( meta.This ) ).Clone() ) );
            }

            return clone;
        }

        [InterfaceMember( IsExplicit = true )]
        private object Clone()
        {
            return meta.This.Clone();
        }
    }

    internal class ManuallyCloneable : ICloneable
    {
        public object Clone()
        {
            return new ManuallyCloneable();
        }
    }

    // <target>
    internal class Targets
    {
        [DeepClone]
        private class AutomaticallyCloneable
        {
            private int a;

            private ManuallyCloneable? b;

            private AutomaticallyCloneable? c;
        }

        [DeepClone]
        private class Derived : AutomaticallyCloneable
        {
            private string d;
        }
    }
}