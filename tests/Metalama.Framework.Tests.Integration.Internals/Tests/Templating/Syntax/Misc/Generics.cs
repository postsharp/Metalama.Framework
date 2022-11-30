#pragma warning disable CS8600, CS8603, CS8618, CS0169, CS0067
using Metalama.Testing.AspectTesting;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.CSharpSyntax.Misc.Generics
{
    [RunTimeOrCompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            A<Aspect, int, string> x = new C<object, int, string>();
            meta.Target.Parameters[0].Value = x;
            dynamic result = meta.Proceed();
            return result;
        }
    }

    interface A<in T1, out T2, T3> : B
        where T1 : class, new()
        where T2 : struct
    {
    }

    interface B
    {
    }

    class C<T1, T2, T3> : A<T1, T2, T3>
        where T1 : class, new()
        where T2 : struct
    {
        B c1;
        D.E c2;
        (int i, string) t;
        event Action1<T1> Event1;

        ref int M<T>(ref int n)
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

    class D
    {
        public class E
        {
            public static E operator !(E e) => null;
            public static implicit operator D(E e) => null;
        }
    }

    delegate void Action1<T>(in int i) where T : class;

    class TargetCode
    {
        object Method(object a)
        {
            return a;
        }
    }
}