
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class IfStatementTests
    {
        private const string IfCompileTimeIfRunTime_Template = @"
using System;
using System.Collections.Generic;

class Aspect
{
  [Template]
  dynamic Template()
  {
      var p = AdviceContext.Method.Parameters[0];
      if (AdviceContext.Method.Name == ""NotNullMethod"")
      {
          if (p.Value == null)
          {
              throw new ArgumentNullException(p.Name);
          }
      }
      else
      {
          if (string.IsNullOrEmpty(p.Value))
          {
              throw new ArgumentException(""IsNullOrEmpty"", p.Name);
          }
      }
      dynamic result = AdviceContext.Proceed();
      return result;
  }
}
";

        private const string IfCompileTimeIfRunTime_Target = @"
class TargetCode
{
    void Method(string a)
    {
    }
}
";

        private const string IfCompileTimeIfRunTime_ExpectedOutput = @"
{
    if (string.IsNullOrEmpty(a))
    {
        throw new ArgumentException(""IsNullOrEmpty"", ""a"");
    }

    __Void result;
    return result;
}";

        [Fact]
        public async Task IfCompileTimeIfRunTime()
        {
            var testResult = await this._testRunner.Run( new TestInput( IfCompileTimeIfRunTime_Template, IfCompileTimeIfRunTime_Target ) );
            testResult.AssertOutput( IfCompileTimeIfRunTime_ExpectedOutput );
        }
    }
}
