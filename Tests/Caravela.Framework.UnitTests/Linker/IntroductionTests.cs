// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Transformations;
using Xunit;

namespace Caravela.Framework.UnitTests.Linker
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
    public void Foo()
    {
    }
}
";

            var compilationModel = CreateCompilation( code );

            var aspectType = CreateFakeAspectType( compilationModel );
            var aspectLayer = new AspectLayer( aspectType, null );

            var targetType= compilationModel.DeclaredTypes.OfName( "T" ).Single();
            var introducedMethodSyntax = CreateIntroducedMethodSyntax( false, Accessibility.Public, "void", "Foo" );
            var introduceMethodTransformation = CreateFakeMethodIntroduction( aspectLayer.AspectLayerId, targetType, introducedMethodSyntax);
            compilationModel = CompilationModel.CreateRevisedInstance( compilationModel, new[] { introduceMethodTransformation } );

            var input = new AspectLinkerInput( compilationModel.RoslynCompilation, compilationModel, Array.Empty<INonObservableTransformation>(), new[] { new OrderedAspectLayer( 1, aspectLayer) } );
            var linker = new AspectLinker( input );
            var result = linker.ToResult();

            var transformedText = result.Compilation.SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }
    }
}
