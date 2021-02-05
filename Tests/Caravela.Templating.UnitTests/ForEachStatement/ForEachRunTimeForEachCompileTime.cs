
using Caravela.TestFramework;
using System.Threading.Tasks;
using Caravela.TestFramework.Templating;
using Xunit;

namespace Caravela.Templating.UnitTests
{
    public partial class ForEachStatementTests
    {
        private const string ForEachRunTimeForEachCompileTime_Template = @"
using System;
using System.Collections.Generic;
using System.Linq;

class Aspect
{
  [TestTemplate]
  dynamic Template()
  {
      IEnumerable<int> array = Enumerable.Range(1, 2);
      
      foreach (int n in array)
      {
          foreach (var p in target.Parameters)
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

        private const string ForEachRunTimeForEachCompileTime_Target = @"
class TargetCode
{
    int Method(int a, int b)
    {
        return a + b;
    }
}";

        private const string ForEachRunTimeForEachCompileTime_ExpectedOutput = @"{
    IEnumerable<int> array = Enumerable.Range(1, 2);
    foreach (int n in array)
    {
        if (a <= n)
        {
            Console.WriteLine(""Oops a <= "" + n);
        }

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
        public async Task ForEachRunTimeForEachCompileTime()
        {
            var testResult = await this._testRunner.Run( new TestInput( ForEachRunTimeForEachCompileTime_Template, ForEachRunTimeForEachCompileTime_Target ) );
            testResult.AssertOutput( ForEachRunTimeForEachCompileTime_ExpectedOutput );
        }
    }
}
