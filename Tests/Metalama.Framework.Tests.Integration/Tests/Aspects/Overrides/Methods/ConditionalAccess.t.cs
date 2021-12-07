internal class TargetClass
    {
        [Override]
        public void TargetMethod_Void()
{
    global::System.Console.WriteLine("This is the overriding method.");
    var x = this;
    ((global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Methods.ConditionalAccess.TargetClass)x)?.TargetMethod_Void_Source();
    return;
}

private void TargetMethod_Void_Source()
        {
            Console.WriteLine( "This is the original method." );
        }

        [Override]
        public int? TargetMethod_Int()
{
    global::System.Console.WriteLine("This is the overriding method.");
    var x = this;
    return ((global::Metalama.Framework.IntegrationTests.Aspects.Overrides.Methods.ConditionalAccess.TargetClass)x)?.TargetMethod_Int_Source();
}

private int? TargetMethod_Int_Source()
        {
            Console.WriteLine( "This is the original method." );

            return 42;
        }
    }