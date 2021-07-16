// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Tests.UnitTests.Linker.Helpers;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Linker
{
    public class EventFieldLinkingTests : LinkerTestBase
    {
        [Fact]
        public void Single()
        {
            var code = @"
class T
{
    delegate void Handler();
    void Test(string s)
    {
    }

    event Handler Foo;

    [PseudoOverride(Foo, TestAspect)]
    event Handler Foo_Override
    {
        add 
        {
            link(this.Foo.add) += value;
        }
        remove 
        {
            link(this.Foo.remove) -= value;
        }
    }
}
";

            var expectedCode = @"
class T
{
    delegate void Handler();
    void Test(string s)
    {
    }

    private Handler __Foo__BackingField;
    event Handler Foo
    {
        add
        {
            this.__Foo__OriginalImpl += value;
        }

        remove
        {
            this.__Foo__OriginalImpl -= value;
        }
    }

    
    private event Handler __Foo__OriginalImpl
    {
        add
        {
            this.__Foo__BackingField += value;
        }

        remove
        {
            this.__Foo__BackingField -= value;
        }
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( this.ServiceProvider, linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }
    }
}