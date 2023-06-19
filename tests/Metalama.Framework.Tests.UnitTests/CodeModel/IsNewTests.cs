// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Testing.UnitTesting;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel
{
    public sealed class IsNewTests : UnitTestClass
    {
        [Fact]
        public void HiddenField()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class C
{
    public int X;
}

class D : C
{
    public new int X;
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var baseField = compilation.Types.OfName( "C" ).Single().Fields.Single();
            var derivedField = compilation.Types.OfName( "D" ).Single().Fields.Single();

            Assert.False( baseField.IsNew );
            Assert.True( derivedField.IsNew );

            Assert.True( derivedField.TryGetHiddenDeclaration( out var hiddenField ) );

            Assert.Equal( baseField, hiddenField );
        }

        [Fact]
        public void HiddenProperty()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class C
{
    public int X { get; set; }
}

class D : C
{
    public new int X { get; set; }
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var baseProperty = compilation.Types.OfName( "C" ).Single().Properties.Single();
            var derivedProperty = compilation.Types.OfName( "D" ).Single().Properties.Single();

            Assert.False( baseProperty.IsNew );
            Assert.True( derivedProperty.IsNew );

            Assert.True( derivedProperty.TryGetHiddenDeclaration( out var hiddenProperty ) );

            Assert.Equal( baseProperty, hiddenProperty );
        }

        [Fact]
        public void HiddenEvent()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class C
{
    public event System.EventHandler X;
}

class D : C
{
    public new event System.EventHandler X;
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var baseEvent = compilation.Types.OfName( "C" ).Single().Events.Single();
            var derivedEvent = compilation.Types.OfName( "D" ).Single().Events.Single();

            Assert.False( baseEvent.IsNew );
            Assert.True( derivedEvent.IsNew );

            Assert.True( derivedEvent.TryGetHiddenDeclaration( out var hiddenEvent ) );

            Assert.Equal( baseEvent, hiddenEvent );
        }

        [Fact]
        public void HiddenIndexer()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class C
{
    public int this[int x] => 42;
}

class D : C
{
    public new int this[int x] => 42;
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var baseIndexer = compilation.Types.OfName( "C" ).Single().Indexers.Single();
            var derivedIndexer = compilation.Types.OfName( "D" ).Single().Indexers.Single();

            Assert.False( baseIndexer.IsNew );
            Assert.True( derivedIndexer.IsNew );

            Assert.True( derivedIndexer.TryGetHiddenDeclaration( out var hiddenIndexer ) );

            Assert.Equal( baseIndexer, hiddenIndexer );
        }

        [Fact]
        public void HiddenMethod()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class C
{
    public void X() {}
}

class D : C
{
    public new void X() {}
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var baseMethod = compilation.Types.OfName( "C" ).Single().Methods.Single();
            var derivedMethod = compilation.Types.OfName( "D" ).Single().Methods.Single();

            Assert.False( baseMethod.IsNew );
            Assert.True( derivedMethod.IsNew );

            Assert.True( derivedMethod.TryGetHiddenDeclaration( out var hiddenMethod ) );

            Assert.Equal( baseMethod, hiddenMethod );
        }

        [Fact]
        public void HiddenByMethod()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class C
{
    public int X;
}

class D : C
{
    public new void X() {}
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var baseField = compilation.Types.OfName( "C" ).Single().Fields.Single();
            var derivedMethod = compilation.Types.OfName( "D" ).Single().Methods.Single();

            Assert.False( baseField.IsNew );
            Assert.True( derivedMethod.IsNew );

            Assert.True( derivedMethod.TryGetHiddenDeclaration( out var hiddenField ) );

            Assert.Equal( baseField, hiddenField );
        }

        [Fact]
        public void HiddenType()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class C
{
    public struct X {}
}

class D : C
{
    public new struct X {}
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var baseType = compilation.Types.OfName( "C" ).Single().NestedTypes.Single();
            var derivedType = compilation.Types.OfName( "D" ).Single().NestedTypes.Single();

            Assert.False( baseType.IsNew );
            Assert.True( derivedType.IsNew );

            Assert.True( derivedType.TryGetHiddenDeclaration( out var hiddenType ) );

            Assert.Equal( baseType, hiddenType );
        }

        [Fact]
        public void Override()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class C
{
    public virtual void X() {}
}

class D : C
{
    public override void X() {}
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var baseMethod = compilation.Types.OfName( "C" ).Single().Methods.Single();
            var derivedMethod = compilation.Types.OfName( "D" ).Single().Methods.Single();

            Assert.False( baseMethod.IsNew );
            Assert.False( derivedMethod.IsNew );

            Assert.False( derivedMethod.TryGetHiddenDeclaration( out var hiddenMethod ) );

            Assert.Null( hiddenMethod );
        }
    }
}