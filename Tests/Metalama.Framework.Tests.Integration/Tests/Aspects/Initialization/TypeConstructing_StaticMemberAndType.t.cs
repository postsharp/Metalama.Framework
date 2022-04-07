// Warning LAMA0035 on ``: `The aspect layers 'Metalama.Framework.Tests.Integration.Aspects.Initialization.TypeConstructing_StaticMemberAndType.Aspect1' and 'Metalama.Framework.Tests.Integration.Aspects.Initialization.TypeConstructing_StaticMemberAndType.AspectBase' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
[Aspect1]
[Aspect2]
public class TargetCode
{
    static TargetCode()
    {
        global::System.Console.WriteLine($"Foo: Aspect2");
        global::System.Console.WriteLine($"TargetCode: Aspect1");
    }
    public static int Foo { get; }
}