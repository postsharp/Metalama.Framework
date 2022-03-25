    // Warning LAMA0035 on ``: `The aspect layers 'Metalama.Framework.Tests.Integration.Aspects.Initialization.InstanceConstructing.Aspect1' and 'Metalama.Framework.Tests.Integration.Aspects.Initialization.InstanceConstructing.AspectBase' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
    [Aspect1]
        [Aspect2]
        public class TargetCode
        {
            public TargetCode()
            {
        this.Constructing_Aspect1();
        this.Constructing_Aspect2();
            }
    
            private int Method( int a )
            {
                return a;
            }
    
    
    private void Constructing_Aspect1()
    {
        global::System.Console.WriteLine($"TargetCode: Aspect1");
    }
    
    private void Constructing_Aspect2()
    {
        global::System.Console.WriteLine($"TargetCode: Aspect2");
    }    }