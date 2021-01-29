
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class CombinedTests
    {
        private const string ForEachParamIfName_Template = @"
using System;
using System.Collections.Generic;

class Aspect
{
  [Template]
  dynamic OverrideMethod()
  {
      foreach ( var p in target.Parameters )
      {
          if (p.Name.Length == 1)
          {
              Console.WriteLine(""{0} = {1}"", p.Name, p.Value);
          }
      }

      foreach ( var p in target.Parameters )
      {
          if (p.Name.StartsWith(""b""))
          {
              Console.WriteLine(""{0} = {1}"", p.Name, p.Value);
          }
      }

      dynamic result = proceed();
      return result;
  }
}
";

        private const string ForEachParamIfName_Target = @"
class TargetCode
{
    string Method(object a, object bb)
    {
        return a.ToString() + bb.ToString();
    }
}";

        private const string ForEachParamIfName_ExpectedOutput = @"{
    Console.WriteLine(""{0} = {1}"", ""a"", a);
    Console.WriteLine(""{0} = {1}"", ""bb"", bb);
    string result;
    result = a.ToString() + bb.ToString();
    return (string)result;
}
";

        [Fact]
        public async Task ForEachParamIfName()
        {
            var testResult = await this._testRunner.Run( new TestInput( ForEachParamIfName_Template, ForEachParamIfName_Target ) );
            testResult.AssertOutput( ForEachParamIfName_ExpectedOutput );
        }
    }
}
