using Caravela.Framework.Impl;
using Caravela.Framework.Impl.AspectOrdering;
using FakeItEasy;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Caravela.Framework.UnitTests
{
    public class AspectOrderingTests : TestBase
    {
        private string GetOrderedAspectLayers( string code, params string[] aspectNames )
        {
            var compilation = CreateCompilation( code );


            var aspectNamedTypes = aspectNames.Select( name => compilation.DeclaredTypes.OfName( name ).Single() );
            var aspectTypes = aspectNamedTypes.Select( aspectType => new AspectType( aspectType, null, null ) ).ToArray();
            var allLayers = aspectTypes.SelectMany( a => a.Layers ).ToImmutableArray();


            var dependencies = new IAspectOrderingSource[] {new AspectLayerOrderingSource( aspectTypes ), new AttributeAspectOrderingSource( compilation )};
            var onDiagnostics = new Action<Diagnostic>( d => throw new AssertionFailedException() );

            Assert.True(
                AspectLayerSorter.TrySort(
                    allLayers,
                    dependencies,
                    onDiagnostics,
                    out var sortedAspectLayers ) );

            return string.Join(
                ", ",
                sortedAspectLayers.OrderBy( l => l.Order ).ThenBy( l => l.AspectName ) );
        }

        [Fact]
        public void OneSingleLayerAspect()
        {
            var code = @"
class Aspect1 {}
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1" );
            Assert.Equal( "Aspect1 => 0", ordered );
        }

        [Fact]
        public void OneDoubleLayerAspect()
        {
            var code = @"
using Caravela.Framework.Aspects;
[ProvidesAspectLayersAttribute(""Layer1"")]
class Aspect1 {}
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1" );
            Assert.Equal( "Aspect1 => 0, Aspect1:Layer1 => 1", ordered );
        }

        [Fact]
        public void TwoUnorderedDoubleLayerAspects()
        {
            var code = @"
using Caravela.Framework.Aspects;
[ProvidesAspectLayersAttribute(""Layer1"")]
class Aspect1 {}

[ProvidesAspectLayersAttribute(""Layer1"")]
class Aspect2 {}
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect1 => 0, Aspect2 => 0, Aspect1:Layer1 => 1, Aspect2:Layer1 => 1", ordered );
        }

        [Fact]
        public void ThreeOrderedSingleLayerAspects()
        {
            var code = @"
using Caravela.Framework.Aspects;

[assembly: AspectOrder( typeof(Aspect3), typeof(Aspect1), typeof(Aspect2) ) ]

class Aspect3{}

class Aspect1 {}

class Aspect2 {}
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2", "Aspect3" );
            Assert.Equal( "Aspect3 => 0, Aspect1 => 1, Aspect2 => 2", ordered );
        }

        [Fact]
        public void TwoOrderedDoubleLayerAspects()
        {
            var code = @"
using Caravela.Framework.Aspects;

[assembly: AspectOrder( typeof(Aspect1), typeof(Aspect2) ) ]

[ProvidesAspectLayersAttribute(""Layer1"")]
class Aspect1 {}

[ProvidesAspectLayersAttribute(""Layer1"")]
class Aspect2 {}
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect1 => 0, Aspect1:Layer1 => 1, Aspect2 => 2, Aspect2:Layer1 => 3", ordered );
        }


        [Fact]
        public void TwoPartiallyOrderedDoubleLayerAspects()
        {
            var code = @"
using Caravela.Framework.Aspects;

[assembly: AspectOrder( ""Aspect1"", ""Aspect2"" ) ]

[ProvidesAspectLayersAttribute(""Layer1"")]
class Aspect1 {}

[ProvidesAspectLayersAttribute(""Layer1"")]
class Aspect2 {}
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect1 => 0, Aspect1:Layer1 => 1, Aspect2 => 1, Aspect2:Layer1 => 2", ordered );
        }
    }
}