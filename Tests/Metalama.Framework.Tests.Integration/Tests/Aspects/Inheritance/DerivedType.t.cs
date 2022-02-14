internal class Targets
    {
        [Aspect]
        private class BaseClass
        {
            private void M(){
    global::System.Console.WriteLine("Overridden!");
}
        }

        private class DerivedClass : BaseClass
        {
            private void N() {
    global::System.Console.WriteLine("Overridden!");
}
        }
    }
