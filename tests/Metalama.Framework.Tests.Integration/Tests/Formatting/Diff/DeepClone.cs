#if TEST_OPTIONS
// @RequiredConstant(NET5_0_OR_GREATER) - Return type covariance not supported in .NET Framework
#endif

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

#pragma warning disable CS0067, CS8618, CS8602, CS8603, CS0169

namespace Metalama.Framework.Tests.Integration.Tests.Formatting.Diff
{
    internal class DeepCloneAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            var typedMethod = builder.Advice.IntroduceMethod(
                builder.Target,
                nameof(CloneImpl),
                whenExists: OverrideStrategy.Override,
                buildMethod: m =>
                {
                    m.Name = "Clone";
                    m.ReturnType = builder.Target;
                } );

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
                ExpressionFactory.Capture( meta.Base.MemberwiseClone(), out baseCall );
            }
            else
            {
                ExpressionFactory.Capture( meta.Target.Method.Invoke(), out baseCall );
            }

            var clone = meta.Cast( meta.Target.Type.ToNonNullableType(), baseCall )!;

            // Select clonable fields.
            var clonableFields =
                meta.Target.Type.FieldsAndProperties.Where(
                    f => f.IsAutoPropertyOrField.GetValueOrDefault() &&
                         ( f.Type.Is( typeof(ICloneable) ) ||
                           ( f.Type is INamedType {  BelongsToCurrentProject: true } fieldNamedType && fieldNamedType.Enhancements().GetAspects<DeepCloneAttribute>().Any() ) ) );

            foreach (var field in clonableFields)
            {
                field.With( (IExpression) clone ).Value = meta.Cast( field.Type, ( (ICloneable)field.Value! ).Clone() );
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