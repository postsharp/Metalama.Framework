using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class CastTests
    {
        private const string RunTimeCast_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [TestTemplate]
    dynamic Template()
    {
        object arg0 = null;
        if (target.Parameters.Count > 0)
        {
            arg0 = target.Parameters[0].Value;
            if (arg0 is string)
            {
                string s = (string) arg0;
                Console.WriteLine(s);
            }
        }
        
        var result = proceed();
        object obj = result;
        string text = obj as string;
        if (text != null)
        {
            return text.Trim();
        }
        
        return obj;
    }
}
";

        private const string RunTimeCast_Target = @"
class TargetCode
{
    string Method(string a)
    {
        return a;
    }
}
";

        private const string RunTimeCast_ExpectedOutput = @"{
    object arg0 = null;
    arg0 = a;
    if (arg0 is string)
    {
        string s = (string)arg0;
        Console.WriteLine(s);
    }

    string result;
    result = a;
    object obj = result;
    string text = obj as string;
    if (text != null)
    {
        return (string)text.Trim();
    }

    return (string)obj;
}";

        [Fact]
        public async Task RunTimeCast()
        {
            var testResult = await this._testRunner.Run( new TestInput( RunTimeCast_Template, RunTimeCast_Target ) );
            testResult.AssertOutput( RunTimeCast_ExpectedOutput );
        }
    }
}
