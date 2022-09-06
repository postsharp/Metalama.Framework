// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Metrics;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Metrics
{
    public class StatementCountMetricTests : TestBase
    {
        [Fact]
        public void BasicTest()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
class C
{
  void M1()  { int k = 0; for ( int i = 0; i < 5; i++ ) {  k++; if ( k == 5 ) { k = 0; } for ( int j = 0; j < i; j++ ) { k++; }  } }
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var m1 = compilation.Types.Single().Methods.OfName( "M1" ).Single();
            var metric = m1.Metrics().Get<StatementNumberMetric>();
            Assert.Equal( 7, metric.Value );
        }
    }
}