// Final Compilation.Emit failed.
// Error CS0052 on `_field`: `Inconsistent accessibility: field type 'RunTimeOnlyClass' is less accessible than field 'IntroducePrivateFieldAttribute._field'`
using Metalama.Framework.Aspects;
using System;
namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Fields.PrivateFieldAccessibility;
#pragma warning disable CS0414 // The field 'IntroducePrivateFieldAttribute._field' is assigned but its value is never used
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
public class IntroducePrivateFieldAttribute : IAspect
{
    [Introduce]
    [global::Metalama.Framework.Aspects.CompiledTemplateAttribute(Accessibility = global::Metalama.Framework.Code.Accessibility.Private, IsAsync = false, IsIteratorMethod = false)]
    public readonly RunTimeOnlyClass _field;
}
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
internal class RunTimeOnlyClass
{
}