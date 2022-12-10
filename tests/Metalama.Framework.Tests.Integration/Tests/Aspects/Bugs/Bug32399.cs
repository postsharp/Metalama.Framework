using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32399;

[Inherited]
[EditorExperience( SuggestAsLiveTemplate = true )]
internal class DeepCloneAttribute : TypeAspect
{
    [Template( IsVirtual = true )]
    public T CloneImpl<[CompileTime] T>()
    {
        // The typeof in the expression below could not compile.
        var clonableFields =
            meta.Target.Type.FieldsAndProperties.Where(
                f => f.IsAutoPropertyOrField.GetValueOrDefault() &&
                     !f.IsImplicitlyDeclared &&
                     ( ( f.Type.Is( typeof(ICloneable) ) && f.Type.SpecialType != SpecialType.String ) ||
                       ( f.Type is INamedType fieldNamedType && fieldNamedType.Enhancements().HasAspect( typeof(DeepCloneAttribute) ) ) ) );

        return default;
    }
}

// <target>
[DeepClone]
internal partial class C { }