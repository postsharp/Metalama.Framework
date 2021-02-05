
using Caravela.TestFramework;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class CombinedTests
    {
        private const string ForEachParamIfValue_Template = @"
using System;
using System.Collections.Generic;

class Aspect
{
  [TestTemplate]
  dynamic Template()
  {
      foreach ( var p in target.Parameters )
      {
          if (p.Value == null)
          {
              throw new ArgumentNullException(p.Name);
          }
      }

      dynamic result = proceed();
      return result;
  }
}
";

        private const string ForEachParamIfValue_Target = @"
class TargetCode
{
    string Method(object a, object b)
    {
        return a.ToString() + b.ToString();
    }
}";

        private const string ForEachParamIfValue_ExpectedOutput = @"{
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
    return (string)result;
}";

        [Fact]
        public async Task ForEachParamIfValue()
        {
            var testResult = await this._testRunner.Run( new TestInput( ForEachParamIfValue_Template, ForEachParamIfValue_Target ) );
            testResult.AssertOutput( ForEachParamIfValue_ExpectedOutput );
        }
    }
}
