using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Linking;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Linker
{
    public class OverrideTests : LinkerTestBase
    {
        [Fact]
        public void Test()
        {
            var code = @"
class T
{
    void Foo()
    {
    }
}
";
            var expectedCode = @"
class T
{
    void Foo()
    {
        this.__Foo__TestAspect();
    }

    void __Foo__OriginalBody()
    {
    }

    void __Foo__TestAspect()
    {
        this.__Foo__OriginalBody();
    }
}
";

            var compilationModel = CreateCompilation( code );

            var aspectType = CreateFakeAspectType();
            var aspectPart = new AspectPart( aspectType, null );

            var targetMethod = compilationModel.DeclaredTypes.OfName( "T" ).Single().Methods.OfName( "Foo" ).Single();
            var overrideTransformation = CreateFakeOverride( aspectPart.ToAspectPartId(), targetMethod, CreateOverrideSyntax( aspectPart.ToAspectPartId(), targetMethod ) );

            var input = new AdviceLinkerInput( compilationModel.RoslynCompilation, compilationModel, new[] { overrideTransformation }, new[] { aspectPart } );
            var linker = new AspectLinker( input );
            var result = linker.ToResult();

            string transformedText = result.Compilation.SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }
    }
}
