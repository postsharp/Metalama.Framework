internal class Targets
    {
        private class BaseClass
        {
            [Aspect]
            public virtual void M() {
    global::System.Console.WriteLine("Overridden!");
}
        }

        private class DerivedClass : BaseClass
        {
            public override void M() {
    global::System.Console.WriteLine("Overridden!");
}
        }
    }
