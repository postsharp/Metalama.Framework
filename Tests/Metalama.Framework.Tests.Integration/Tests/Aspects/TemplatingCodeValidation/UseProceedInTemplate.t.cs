namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.UseProceedInTemplate;

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
#pragma warning disable CS0067


internal class RetryAttribute : OverrideMethodAspect
{
    // Template that overrides the methods to which the aspect is applied.
    public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");


    [Template]
public void Method() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

}
#pragma warning restore CS0067

