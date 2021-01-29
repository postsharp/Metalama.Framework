
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
  dynamic OverrideMethod()
  {
      var p = target.Parameters[0];
      if (target.Method.Name == ""NotNullMethod"")
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
      
      return proceed();
  }
}
";

        private const string IfCompileTimeIfRunTime_Target = @"
class TargetCode
{
    string Method(string a)
    {
        return a;
    }
}
";

        private const string IfCompileTimeIfRunTime_ExpectedOutput = @"{
    if (string.IsNullOrEmpty(a))
    {
        throw new ArgumentException(""IsNullOrEmpty"", ""a"");
    }

    return a;
}";

        [Fact]
        public async Task IfCompileTimeIfRunTime()
        {
            var testResult = await this._testRunner.Run( new TestInput( IfCompileTimeIfRunTime_Template, IfCompileTimeIfRunTime_Target ) );
            testResult.AssertOutput( IfCompileTimeIfRunTime_ExpectedOutput );
        }
    }
}
