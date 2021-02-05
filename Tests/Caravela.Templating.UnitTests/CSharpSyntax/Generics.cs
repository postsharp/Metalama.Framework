using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class CSharpSyntaxTests
    {
        private const string Generics_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        A<Aspect, int, string> x = new C<object, int, string>();
        target.Parameters[0].Value = x;
        dynamic result = proceed();
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
";

        private const string Generics_Target = @"
class TargetCode
{
    object Method(object a)
    {
        return a;
    }
}
";

        private const string Generics_ExpectedOutput = @"
{
    A<Aspect, int, string> x = new C<object, int, string>();
    a = x;
    object result;
    result = a;
    return (object)result;
}
";

        [Fact]
        public async Task Generics()
        {
            var testResult = await this._testRunner.Run( new TestInput( Generics_Template, Generics_Target ) );
            testResult.AssertOutput( Generics_ExpectedOutput );
        }
    }
}
