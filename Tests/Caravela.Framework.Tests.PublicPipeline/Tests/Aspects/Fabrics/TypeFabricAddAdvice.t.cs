class TargetCode
    {
        int Method1(int a) {
    global::System.Console.WriteLine("overridden");
return a;};
        string Method2(string s) {
    global::System.Console.WriteLine("overridden");
return s;};
#pragma warning disable CS0067
        
        
        class Fabric : ITypeFabric
        {
            public void AmendType(ITypeAmender amender) => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

            
            [Template]
object? Template() => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

        
        }
#pragma warning restore CS0067
        
    }