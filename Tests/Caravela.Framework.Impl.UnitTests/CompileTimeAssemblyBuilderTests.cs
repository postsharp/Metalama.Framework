using System.Linq;
using Caravela.Framework.Impl.CompileTime;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests
{
    public class CompileTimeAssemblyBuilderTests : TestBase
    {
        [Fact]
        public void RemoveInvalidUsingsRewriterTest()
        {
            var compilation = CreateRoslynCompilation(
                @"
using System;
using Nonsense;
using Foo;

namespace Foo
{
    class C {}
}
", ignoreErrors: true );

            var expected = @"
using System;
using Foo;

namespace Foo
{
    class C {}
}
";

            var rewriter = new CompileTimeAssemblyBuilder.RemoveInvalidUsingsRewriter( compilation );

            var actual = rewriter.Visit( compilation.SyntaxTrees.Single().GetRoot() ).ToFullString();

            Assert.Equal( expected, actual );
        }
    }
}
