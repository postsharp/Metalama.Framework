﻿using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.Dirty
{
    class DeepCloneAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var typedMethod = builder.AdviceFactory.IntroduceMethod(
                builder.TargetDeclaration,
                nameof(CloneImpl),
                whenExists: OverrideStrategy.Override);

            typedMethod.Name = "Clone";
            typedMethod.ReturnType = builder.TargetDeclaration;

            builder.AdviceFactory.ImplementInterface(
                builder.TargetDeclaration,
                typeof(ICloneable),
                whenExists: OverrideStrategy.Ignore);
        }

        [Template(IsVirtual = true)]
        public virtual dynamic CloneImpl()
        {
            // Define a local variable of the same type as the target type.
            var clone = meta.Type.DefaultValue();

            // TODO: access to meta.Method.Invokers.Base does not work.
            if (meta.Method.Invokers.Base == null)
            {
                // Invoke base.MemberwiseClone().
                clone = meta.Cast(meta.Type, meta.Base.MemberwiseClone());
            }
            else
            {
                // Invoke the base method.
                clone = meta.Method.Invokers.Base.Invoke(meta.This);
            }

            // Select clonable fields.
            var clonableFields =
                meta.Type.FieldsAndProperties.Where(
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
    [DeepClone]
    class AutomaticallyCloneable
    {
        int a;

        ManuallyCloneable? b;

        AutomaticallyCloneable? c;
    }
}
