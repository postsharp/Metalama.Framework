using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class LocalVariablesTests
    {
        private const string NameClashCompileTimeForEach_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        foreach(var p in target.Parameters)
        {
            string text = p.Name + "" = "" + p.Value;
            Console.WriteLine(text);
        }
        
        return proceed();
    }
}
";

        private const string NameClashCompileTimeForEach_Target = @"
class TargetCode
{
    int Method(int a, int b)
    {
        return a + b;
    }
}
";

        private const string NameClashCompileTimeForEach_ExpectedOutput = @"{
    string text = ""a = "" + a;
    Console.WriteLine(text);
    string text_1 = ""b = "" + b;
    Console.WriteLine(text_1);
    return a + b;
}";

        [Fact]
        public async Task NameClashCompileTimeForEach()
        {
            var testResult = await this._testRunner.Run( new TestInput( NameClashCompileTimeForEach_Template, NameClashCompileTimeForEach_Target ) );
            testResult.AssertOutput( NameClashCompileTimeForEach_ExpectedOutput );
        }
    }
}
