﻿using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Linking;
using Xunit;

namespace Caravela.Framework.UnitTests.Linker
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
            var aspectLayer = new AspectLayer( aspectType, null );

            var targetMethod = compilationModel.DeclaredTypes.OfName( "T" ).Single().Methods.OfName( "Foo" ).Single();
            var overrideTransformation = CreateFakeMethodOverride( aspectLayer.AspectLayerId, targetMethod, CreateOverrideSyntax( aspectLayer.AspectLayerId, targetMethod ) );

            var input = new AspectLinkerInput( compilationModel.RoslynCompilation, compilationModel, new[] { overrideTransformation }, new[] { new OrderedAspectLayer( 1, aspectLayer ) } );
            var linker = new AspectLinker( input );
            var result = linker.ToResult();

            var transformedText = result.Compilation.SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }
    }
}
