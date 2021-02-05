
using Caravela.TestFramework;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ForEachStatementTests
    {
        private const string ForEachCompileTimeForEachRunTime_Template = @"
using System;
using System.Collections.Generic;
using System.Linq;

class Aspect
{
  [TestTemplate]
  dynamic Template()
  {
      IEnumerable<int> array = Enumerable.Range(1, 2);
      
      foreach (var p in target.Parameters)
      {
          foreach (int n in array)
          {
              if (p.Value <= n)
              {
                  Console.WriteLine(""Oops "" + p.Name + "" <= "" + n);
              }
          }
      }

      dynamic result = proceed();
      return result;
  }
}
";

        private const string ForEachCompileTimeForEachRunTime_Target = @"
class TargetCode
{
    int Method(int a, int b)
    {
        return a + b;
    }
}";

        private const string ForEachCompileTimeForEachRunTime_ExpectedOutput = @"{
    IEnumerable<int> array = Enumerable.Range(1, 2);
    foreach (int n in array)
    {
        if (a <= n)
        {
            Console.WriteLine(""Oops a <= "" + n);
        }
    }

    foreach (int n in array)
    {
        if (b <= n)
        {
            Console.WriteLine(""Oops b <= "" + n);
        }
    }

    int result;
    result = a + b;
    return (int)result;
}";

        [Fact]
        public async Task ForEachCompileTimeForEachRunTime()
        {
            var testResult = await this._testRunner.Run( new TestInput( ForEachCompileTimeForEachRunTime_Template, ForEachCompileTimeForEachRunTime_Target ) );
            testResult.AssertOutput( ForEachCompileTimeForEachRunTime_ExpectedOutput );
        }
    }
}
