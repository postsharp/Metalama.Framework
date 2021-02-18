using System;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Transformations;
using FakeItEasy;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Linker
{
    public class IntroductionTests : LinkerTestBase
    {
        [Fact]
        public void Test()
        {
            var code = @"
class T
{
}
";
            var expectedCode = @"
class T
{
    void Foo()
    {
    }
}
";

            var compilationModel = CreateCompilation( code );

            var aspectType = CreateFakeAspectType();
            var aspectPart = new AspectPart( aspectType, null );

            var targetType= compilationModel.DeclaredTypes.OfName( "T" ).Single();
            var introduceMethodTransformation = new MethodBuilder( A.Fake<Advice>(), targetType, "Foo" );
            compilationModel = new CompilationModel( compilationModel, new[] { introduceMethodTransformation } );

            var input = new AdviceLinkerInput( compilationModel.RoslynCompilation, compilationModel, Array.Empty<INonObservableTransformation>(), new[] { aspectPart } );
            var linker = new AspectLinker( input );
            var result = linker.ToResult();

            var transformedText = result.Compilation.SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }
    }
}
