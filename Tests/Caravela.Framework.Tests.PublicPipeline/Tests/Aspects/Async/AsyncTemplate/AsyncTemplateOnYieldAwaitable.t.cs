class TargetCode
    {
    
        // The normal template should be applied because YieldAwaitable does not have a method builder.
        
        [Aspect]
        public System.Runtime.CompilerServices.YieldAwaitable AsyncMethod(int a)
{
    global::System.Console.WriteLine("Normal template.");
            return Task.Yield();
}
    }
