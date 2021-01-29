
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class IfStatementTests
    {
        private const string IfRunTimeIfCompileTime_Template = @"
using System;
using System.Collections.Generic;

class Aspect
{
  [Template]
  dynamic OverrideMethod()
  {
      if (target.Parameters[0].Value == null)
      {
          if (target.Method.Name == ""DontThrowMethod"")
          {
              Console.WriteLine( ""Oops"" );
          }
          else
          {
              throw new ArgumentNullException();
          }
      }
      return proceed();
  }
}
";

        private const string IfRunTimeIfCompileTime_Target = @"
class TargetCode
{
    void Method(object a)
    {
    }
}
";

        private const string IfRunTimeIfCompileTime_ExpectedOutput = @"{
    if (a == null)
    {
        throw new ArgumentNullException();
    }
}";

        [Fact]
        public async Task IfRunTimeIfCompileTime()
        {
            var testResult = await this._testRunner.Run( new TestInput( IfRunTimeIfCompileTime_Template, IfRunTimeIfCompileTime_Target ) );
            testResult.AssertOutput( IfRunTimeIfCompileTime_ExpectedOutput );
        }
    }
}
