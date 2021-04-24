// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests
{
    public class AspectOrderingTests : TestBase
    {
        private static string GetOrderedAspectLayers( string code, params string[] aspectNames )
        {
            var compilation = CreateCompilation( code );
            DiagnosticList diagnostics = new();

            var aspectTypeFactory = new AspectTypeFactory(
                compilation.RoslynCompilation,
                new AspectDriverFactory( compilation.RoslynCompilation, ImmutableArray<object>.Empty ) );

            var aspectNamedTypes = aspectNames.Select( name => compilation.DeclaredTypes.OfName( name ).Single().GetSymbol() ).ToReadOnlyList();
            var aspectTypes = aspectTypeFactory.GetAspectTypes( aspectNamedTypes, diagnostics ).ToImmutableArray();
            var allLayers = aspectTypes.SelectMany( a => a.Layers ).ToImmutableArray();

            var dependencies = new IAspectOrderingSource[]
            {
                new AspectLayerOrderingSource( aspectTypes ), new AttributeAspectOrderingSource( compilation.RoslynCompilation )
            };

            Assert.True(
                AspectLayerSorter.TrySort(
                    allLayers,
                    dependencies,
                    diagnostics,
                    out var sortedAspectLayers ) );

            Assert.Empty( diagnostics );

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

            var ordered = GetOrderedAspectLayers( code, "Aspect1" );
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

            var ordered = GetOrderedAspectLayers( code, "Aspect1" );
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

            var ordered = GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
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

            var ordered = GetOrderedAspectLayers( code, "Aspect1", "Aspect2", "Aspect3" );
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

            var ordered = GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
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

            var ordered = GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect1 => 0, Aspect1:Layer1 => 1, Aspect2 => 1, Aspect2:Layer1 => 2", ordered );
        }

        [Fact]
        public void InheritedAspects()
        {
            var code = @"
using Caravela.Framework.Aspects;

[ProvidesAspectLayersAttribute(""Layer1"")]
class Aspect1 {}

class Aspect2 : Aspect1 {}
";

            var ordered = GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect1 => 0, Aspect2 => 0, Aspect1:Layer1 => 1, Aspect2:Layer1 => 1", ordered );
        }
    }
}