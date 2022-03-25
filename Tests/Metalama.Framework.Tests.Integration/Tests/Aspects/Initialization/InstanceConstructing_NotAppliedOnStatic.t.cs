    [Aspect]
        public class TargetCode
        {
            public TargetCode()
            {
        this.Constructing_Aspect();
            }
            static TargetCode()
            {
            }
    
            private int Method(int a)
            {
                return a;
            }
    
    
    private void Constructing_Aspect()
    {
        global::System.Console.WriteLine($"TargetCode: Aspect");
    }    }