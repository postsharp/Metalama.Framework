
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
  dynamic Template()
  {
      foreach ( var p in AdviceContext.Method.Parameters )
      {
          if (p.Name.Length == 1)
          {
              Console.WriteLine(p.Name + "" = "" + p.Value);
          }
      }

      dynamic result = AdviceContext.Proceed();
      return result;
  }
}
";

        private const string ForEachParamIfName_Target = @"
class TargetCode
{
    string Method(object a, object b)
    {
        return a.ToString() + b.ToString();
    }
}";

        private const string ForEachParamIfName_ExpectedOutput = @"
{
    if (a == null)
    {
        throw new ArgumentNullException(""a"");
    }

    if (b == null)
    {
        throw new ArgumentNullException(""b"");
    }

    string result;
    result = a.ToString() + b.ToString();
    return result;
}";

        [Fact]
        public async Task ForEachParamIfName()
        {
            var testResult = await this._testRunner.Run( new TestInput( ForEachParamIfName_Template, ForEachParamIfName_Target ) );
            testResult.AssertOutput( ForEachParamIfName_ExpectedOutput );
        }
    }
}
