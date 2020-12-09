
using Caravela.TestFramework.Templating;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ForEachStatementTests
    {
        private const string ForEachCompileTimeNested_Template = @"
using System;
using System.Collections.Generic;

class Aspect
{
  [Template]
  dynamic Template()
  {
      int[] array = new int[] {1,2};
      AdviceContext.BuildTime(array);
      
      foreach (int n in array)
      {
          foreach (var p in AdviceContext.Method.Parameters)
          {
              if (p.Value <= n)
              {
                  Console.WriteLine(""Oops "" + p.Name + "" <= "" + n);
              }
          }
      }

      dynamic result = AdviceContext.Proceed();
      return result;
  }
}
";

        private const string ForEachCompileTimeNested_Target = @"
class TargetCode
{
    int Method(int a, int b)
    {
        return a + b;
    }
}";

        private const string ForEachCompileTimeNested_ExpectedOutput = @"
{
    if (a <= 1)
    {
        Console.WriteLine(""Oops a <= 1"");
    }

    if (b <= 1)
    {
        Console.WriteLine(""Oops b <= 1"");
    }

    if (a <= 2)
    {
        Console.WriteLine(""Oops a <= 2"");
    }

    if (b <= 2)
    {
        Console.WriteLine(""Oops b <= 2"");
    }

    int result;
    result = a + b;
    return result;
}
";

        [Fact]
        public async Task ForEachCompileTimeNested()
        {
            var testResult = await this._testRunner.Run( new TestInput( ForEachCompileTimeNested_Template, ForEachCompileTimeNested_Target ) );
            testResult.AssertOutput( ForEachCompileTimeNested_ExpectedOutput );
        }
    }
}
