namespace Metalama.Open.Virtuosity.TestApp
{



    [VirtualizeAttribute]

    internal class C
    {
        private void ImplicitPrivate() { }

        private void ExplicitPrivate() { }

        public void Public() { }

        public virtual void PublicVirtual() { }

        protected async void Protected() { }

        private protected void PrivateProtected() { }

        public sealed override string ToString()
        {
            return null;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static void PublicStatic() { }

        public int Property { get; }
    }

    internal sealed partial class SC
    {
        public void M() { }
    }

}
