
using Caravela.TestFramework;
using System.Threading.Tasks;
using Caravela.TestFramework.Templating;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class IfStatementTests
    {
        private const string IfCompileTimeNested_Template = @"
using System;
using System.Collections.Generic;

class Aspect
{
  [TestTemplate]
  dynamic Template()
  {
      int t = 0;
      string name = target.Parameters[0].Name;
      if (name.Contains(""a""))
      {
          if (name.Contains(""b""))
          {
              t = 1;
          }
          else
          {
              if (name.Contains(""c""))
              {
                  t = 42;
              }
              else
              {
                  t = 3;
              }
          }
      }
      else
      {
          t = 4;
      }

      Console.WriteLine(t);
      dynamic result = proceed();
      return result;
  }
}
";

        private const string IfCompileTimeNested_Target = @"
class TargetCode
{
    void Method(string aBc)
    {
    }
}
";

        private const string IfCompileTimeNested_ExpectedOutput = @"
{
    Console.WriteLine(42);

    __Void result;
    return result;
}";

        [Fact( Skip = "#28034 Template compiler: compile-time variable is not replaced with a value in the final code" )]
        public async Task IfCompileTimeNested()
        {
            var testResult = await this._testRunner.Run( new TestInput( IfCompileTimeNested_Template, IfCompileTimeNested_Target ) );
            testResult.AssertOutput( IfCompileTimeNested_ExpectedOutput );
        }
    }
}
