using System.Threading.Tasks;
using Caravela.TestFramework;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class LocalVariablesTests
    {
        private const string NameClashWithMemberAccess_Template = @"  
using System;
using System.Collections.Generic;

class Aspect
{
    [Template]
    dynamic Template()
    {
        var n = target.Parameters.Count; // build-time
        
        if (n == 1)
        {
            var WriteLine = 0;
            Console.WriteLine(WriteLine);
        }
        
        if (n == 1)
        {
            var WriteLine = 1;
            Console.WriteLine(WriteLine);
        }
        
        return proceed();
    }
}
";

        private const string NameClashWithMemberAccess_Target = @"
class TargetCode
{
    int Method(int a)
    {
        return a;
    }
}
";

        private const string NameClashWithMemberAccess_ExpectedOutput = @"{
    var WriteLine = 0;
    Console.WriteLine(WriteLine);
    var WriteLine_1 = 1;
    Console.WriteLine(WriteLine_1);
    return a;
}";

        [Fact]
        public async Task NameClashWithMemberAccess()
        {
            var testResult = await this._testRunner.Run( new TestInput( NameClashWithMemberAccess_Template, NameClashWithMemberAccess_Target ) );
            testResult.AssertOutput( NameClashWithMemberAccess_ExpectedOutput );
        }
    }
}
