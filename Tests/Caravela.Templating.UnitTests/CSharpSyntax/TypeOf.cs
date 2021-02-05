using Caravela.TestFramework;
using System.Threading.Tasks;
using Caravela.TestFramework.Templating;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class CSharpSyntaxTests
    {
        private const string TypeOf_Template = @"  
using System;
using System.Collections.Generic;
using Caravela.Framework.Project;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        string s = compileTime(typeof(string).FullName);
        Console.WriteLine(s);
        
        if ( target.Parameters[0].Type.Is(typeof(string)) )
        {
            Console.WriteLine(typeof(string).FullName);
        }
        
        Console.WriteLine(typeof(MyClass1).FullName);
        
        return proceed();
    }
}

    
[CompileTime]
public class MyClass1 {}
";

        private const string TypeOf_Target = @"
class TargetCode
{
    string Method(string a)
    {
        return a;
    }
}
";

        private const string TypeOf_ExpectedOutput = @"{
    Console.WriteLine(""System.String"");
    Console.WriteLine(typeof(string).FullName);
    Console.WriteLine(""MyClass1"");
    return a;
}";

        [Fact]
        public async Task TypeOf()
        {
            var testResult = await this._testRunner.Run( new TestInput( TypeOf_Template, TypeOf_Target ) );
            testResult.AssertOutput( TypeOf_ExpectedOutput );
        }
    }
}
