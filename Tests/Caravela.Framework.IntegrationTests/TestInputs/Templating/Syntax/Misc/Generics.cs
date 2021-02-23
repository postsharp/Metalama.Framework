#pragma warning disable CS8600, CS8603, CS8618, CS0169, CS0067
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.CSharpSyntax.Misc.Generics
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            A<Aspect, int, string> x = new C<object, int, string>();
            target.Parameters[0].Value = x;
            dynamic result = proceed();
            return result;
        }
    }

    internal interface A<in T1, out T2, T3> : B
        where T1 : class, new()
        where T2 : struct
    {
    }

    internal interface B
    {
    }

    internal class C<T1, T2, T3> : A<T1, T2, T3>
        where T1 : class, new()
        where T2 : struct
    {
        private B c1;
        private D.E c2;
        private (int i, string) t;

        private event Action1<T1> Event1;

        private ref int M<T>(ref int n)
           where T : D.E
        {
            return ref n;
        }

        public int this[int i]
        {
            get
            {
                return i;
            }
        }

        ~C()
        {
        }
    }

    internal class D
    {
        public class E
        {
            public static E operator !(E e) => null;

            public static implicit operator D(E e) => null;
        }
    }

    internal delegate void Action1<T>(in int i) where T : class;

    internal class TargetCode
    {
        private object Method(object a)
        {
            return a;
        }
    }
}