#if TEST_OPTIONS
// @RequiredConstant(NET5_0_OR_GREATER)
#endif

// In .NET Framework we get: Target runtime doesn't support covariant return types in overrides. Return type must be 'Targets.AutomaticallyCloneable'
// to match overridden member 'Targets.AutomaticallyCloneable.Clone()'`

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Samples.DeepClone2;

#pragma warning disable CS0169

[Inheritable]
public class DeepCloneAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.IntroduceMethod(
            builder.Target,
            nameof(CloneImpl),
            whenExists: OverrideStrategy.Override,
            buildMethod: t =>
            {
                t.Name = "Clone";
                t.ReturnType = builder.Target;
            } );

        builder.Advice.ImplementInterface(
            builder.Target,
            typeof(ICloneable),
            whenExists: OverrideStrategy.Ignore );
    }

    [Template( IsVirtual = true )]
    public virtual dynamic CloneImpl()
    {
        // This compile-time variable will receive the expression representing the base call.
        // If we have a public Clone method, we will use it (this is the chaining pattern). Otherwise,
        // we will call MemberwiseClone (this is the initialization of the pattern).
        IExpression baseCall;

        if (meta.Target.Method.IsOverride)
        {
            ExpressionFactory.Capture( meta.Base.Clone(), out baseCall );
        }
        else
        {
            ExpressionFactory.Capture( meta.Base.MemberwiseClone(), out baseCall );
        }

        // Define a local variable of the same type as the target type.
        var clone = meta.Cast( meta.Target.Type.ToNonNullableType(), baseCall )!;

        // Select clonable fields.
        var clonableFields =
            meta.Target.Type.FieldsAndProperties.Where(
                f => f.IsAutoPropertyOrField == true &&
                     ( ( f.Type.Is( typeof(ICloneable) ) && f.Type.SpecialType != SpecialType.String ) ||
                       ( f.Type is INamedType fieldNamedType &&
                         fieldNamedType.Enhancements().HasAspect<DeepCloneAttribute>() ) ) );

        foreach (var field in clonableFields)
        {
            // Check if we have a public method 'Clone()' for the type of the field.
            var fieldType = (INamedType)field.Type;
            var cloneMethod = fieldType.Methods.OfExactSignature( "Clone", Array.Empty<IType>() );

            IExpression callClone;

            if (cloneMethod is { Accessibility: Accessibility.Public } ||
                fieldType.Enhancements().HasAspect<DeepCloneAttribute>())
            {
                // If yes, call the method without a cast.
                callClone = field.Value?.Clone()!;
            }
            else
            {
                // If no, explicitly cast to the interface.
                callClone = (IExpression)( (ICloneable?)field.Value )?.Clone()!;
            }

            if (cloneMethod == null || !cloneMethod.ReturnType.ToNullableType().Is( fieldType ))
            {
                // If necessary, cast the return value of Clone to the field type.
                callClone = (IExpression)meta.Cast( fieldType, callClone.Value );
            }

            // Finally, set the field value.
            field.With( (IExpression)clone ).Value = callClone.Value;
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
[DeepClone]
internal class AutomaticallyCloneable
{
    private int _a;
    private ManuallyCloneable? _b;
    private AutomaticallyCloneable? _c;
}

// <target>
internal class DerivedCloneable : AutomaticallyCloneable
{
    private string? _d;
}