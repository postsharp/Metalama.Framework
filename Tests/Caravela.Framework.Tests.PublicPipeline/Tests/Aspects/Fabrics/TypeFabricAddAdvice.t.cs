class TargetCode
    {
        int Method1(int a) {
    global::System.Console.WriteLine("overridden");
return a;};
        string Method2(string s) {
    global::System.Console.WriteLine("overridden");
return s;};
        
        
        class Fabric : ITypeFabric
        {
            public void AmendType(ITypeAmender builder) => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

            
            [Template]
dynamic? Template() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

        
        }
        
    }