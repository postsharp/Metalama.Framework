    // Warning LAMA0035 on ``: `The aspect layers 'Metalama.Framework.Tests.Integration.Aspects.Initialization.TypeConstructing.Aspect1' and 'Metalama.Framework.Tests.Integration.Aspects.Initialization.TypeConstructing.AspectBase' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
    [Aspect1]
        [Aspect2]
        public class TargetCode
        {
            static TargetCode()
            {
        TypeConstructing_Aspect1();
        TypeConstructing_Aspect2();
            }
    
            private int Method(int a)
            {
                return a;
            }
    
    
    private static void TypeConstructing_Aspect1()
    {
        global::System.Console.WriteLine($"TargetCode: Aspect1");
    }
    
    private static void TypeConstructing_Aspect2()
    {
        global::System.Console.WriteLine($"TargetCode: Aspect2");
    }    }