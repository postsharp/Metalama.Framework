internal class TargetCode
    {
        [CompileTimeIf]
        public void InstanceMethod()
{
    global::System.Console.WriteLine($"Invoking Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.CompileTimeIf.TargetCode.InstanceMethod() on instance {base.ToString()}.");
            Console.WriteLine("InstanceMethod");
    return;
}
    
        [CompileTimeIf]
        public static void StaticMethod()
{
    global::System.Console.WriteLine($"Invoking Caravela.Framework.Tests.Integration.Tests.Aspects.Samples.CompileTimeIf.TargetCode.StaticMethod()");
            Console.WriteLine("StaticMethod");
    return;
}
    }