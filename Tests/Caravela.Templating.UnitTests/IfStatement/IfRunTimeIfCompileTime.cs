
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
  dynamic Template()
  {
      if (AdviceContext.Method.Parameters[0].Value == null)
      {
          if (AdviceContext.Method.Name == ""DontThrowMethod"")
          {
              Console.WriteLine( ""Oops"" );
          }
          else
          {
              throw new ArgumentNullException();
          }
      }
      dynamic result = AdviceContext.Proceed();
      return result;
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

        private const string IfRunTimeIfCompileTime_ExpectedOutput = @"
{
    if (a == null)
    {
        throw new ArgumentNullException();
    }

    __Void result;
    return result;
}";

        [Fact]
        public async Task IfRunTimeIfCompileTime()
        {
            var testResult = await this._testRunner.Run( new TestInput( IfRunTimeIfCompileTime_Template, IfRunTimeIfCompileTime_Target ) );
            testResult.AssertOutput( IfRunTimeIfCompileTime_ExpectedOutput );
        }
    }
}
