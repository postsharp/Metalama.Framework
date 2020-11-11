using System;
using System.Linq;
using static System.Reflection.BindingFlags;

[assembly: Caravela.Patterns.Virtuosity.Virtuosity]

namespace Caravela.Patterns.Virtuosity.TestApp
{
    struct S
    {
        public void M() { }
    }

    interface I
    {
        public void M() { }
    }

    class C
    {
        void ImplicitPrivate() { }

        private void ExplicitPrivate() { }

        public void Public() { }

        public virtual void PublicVirtual() { }

        protected async void Protected() { }

        private protected void PrivateProtected() { }

        public sealed override string ToString() => null;

        public override int GetHashCode() => 0;

        public static void PublicStatic() { }

        public int Property { get; }
    }

    sealed partial class SC
    {
        public void M() { }
    }

    class Program
    {
        static void Main()
        {
            var methods =
                from type in new[] { typeof(S), typeof(I), typeof(C), typeof(SC) }
                from method in type.GetMethods(Public | NonPublic | Static | Instance)
                select method;

            foreach (var method in methods)
            {
                Console.WriteLine($"{method.ReflectedType.Name}.{method.Name}: {method.IsVirtual}");
            }
        }
    }
}
