// Warning CS0414 on `_f`: `The field 'C._f' is assigned but its value is never used`
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Initialization.ToConstructor;
#pragma warning disable CS0067

public class MyAspect : ConstructorAspect
{
    public override void BuildAspect(IAspectBuilder<IConstructor> builder) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");


    [Introduce]
    private int _f;

    [Template]
private void Initialize() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

}

#pragma warning restore CS0067

public class C
{
   [MyAspect]
   public C() {    this._f = 5;
}

private global::System.Int32 _f;}
