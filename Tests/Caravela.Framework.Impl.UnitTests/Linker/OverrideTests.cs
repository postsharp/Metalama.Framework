using Caravela.Framework.Impl.CodeModel.Symbolic;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Transformations;
using FakeItEasy;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests.Linker
{
    public class OverrideTests : TestBase
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

            var compilationModel = CreateCompilation( code );
            var overrideTransformation = A.Fake<INonObservableTransformation>( builder => builder.Implements<IMemberIntroduction>() );

            var input = new AdviceLinkerInput( ((CompilationModel) compilationModel).RoslynCompilation, compilationModel, new[] { overrideTransformation }, Array.Empty<IObservableTransformation>() );

            AspectLinker linker = new AspectLinker( input );
        }
    }
}
