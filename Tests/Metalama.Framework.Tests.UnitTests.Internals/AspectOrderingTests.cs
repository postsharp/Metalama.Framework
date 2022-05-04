// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.TestFramework;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests
{
    public class AspectOrderingTests : TestBase
    {
        private bool TryGetOrderedAspectLayers( string code, string[] aspectNames, DiagnosticList diagnostics, [NotNullWhen( true )] out string? sortedAspects )
        {
            using var testContext = this.CreateTestContext();

            var compilation = testContext.CreateCompilationModel( code );

            var compileTimeDomain = new UnloadableCompileTimeDomain();
            var loader = CompileTimeProjectLoader.Create( compileTimeDomain, testContext.ServiceProvider );

            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    compilation.RoslynCompilation,
                    RedistributionLicenseInfo.Empty, 
                    null,
                    new DiagnosticList(),
                    false,
                    CancellationToken.None,
                    out var compileTimeProject ) );

            var aspectTypeFactory = new AspectClassMetadataFactory(
                testContext.ServiceProvider,
                new AspectDriverFactory( compilation.RoslynCompilation, ImmutableArray<object>.Empty, testContext.ServiceProvider ) );

            var aspectNamedTypes = aspectNames.Select( name => compilation.Types.OfName( name ).Single().GetSymbol() ).ToReadOnlyList();
            var aspectTypes = aspectTypeFactory.GetAspectClasses( aspectNamedTypes, compileTimeProject!, diagnostics ).ToImmutableArray();
            var allLayers = aspectTypes.SelectMany( a => a.Layers ).ToImmutableArray();

            var dependencies = new IAspectOrderingSource[]
            {
                new AspectLayerOrderingSource( aspectTypes ), new AttributeAspectOrderingSource( compilation.RoslynCompilation, loader )
            };

            if ( AspectLayerSorter.TrySort(
                    allLayers,
                    dependencies,
                    diagnostics,
                    out var sortedAspectLayers ) )
            {
                sortedAspects = string.Join(
                    ", ",
                    sortedAspectLayers.OrderBy( l => l.Order ).ThenBy( l => l.AspectName ) );

                return true;
            }
            else
            {
                sortedAspects = null;

                return false;
            }
        }

        private string GetOrderedAspectLayers( string code, params string[] aspectNames )
        {
            var diagnostics = new DiagnosticList();
            Assert.True( this.TryGetOrderedAspectLayers( code, aspectNames, diagnostics, out var sortedAspects ) );
            Assert.Empty( diagnostics );

            return sortedAspects!;
        }

        [Fact]
        public void OneSingleLayerAspect()
        {
            var code = @"
using Metalama.Framework.Aspects;
class Aspect1 : TypeAspect { }
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1" );
            Assert.Equal( "Aspect1 => 0", ordered );
        }

        [Fact]
        public void OneDoubleLayerAspect()
        {
            var code = @"
using Metalama.Framework.Aspects;
[Layers(""Layer1"")]
class Aspect1 : TypeAspect { }
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1" );
            Assert.Equal( "Aspect1 => 0, Aspect1:Layer1 => 1", ordered );
        }

        [Fact]
        public void TwoUnorderedDoubleLayerAspects()
        {
            var code = @"
using Metalama.Framework.Aspects;
[Layers(""Layer1"")]
class Aspect1 : TypeAspect { }

[Layers(""Layer1"")]
class Aspect2 : TypeAspect { }
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect1 => 0, Aspect2 => 0, Aspect1:Layer1 => 1, Aspect2:Layer1 => 1", ordered );
        }

        [Fact]
        public void ThreeOrderedSingleLayerAspects()
        {
            var code = @"
using Metalama.Framework.Aspects;

[assembly: AspectOrder( typeof(Aspect2), typeof(Aspect1), typeof(Aspect3) ) ]

class Aspect3 : TypeAspect
{
    
}

class Aspect1 : TypeAspect
{
    
}

class Aspect2 : TypeAspect
{
    
}
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2", "Aspect3" );
            Assert.Equal( "Aspect3 => 0, Aspect1 => 1, Aspect2 => 2", ordered );
        }

        [Fact]
        public void TwoOrderedDoubleLayerAspects()
        {
            var code = @"
using Metalama.Framework.Aspects;

[assembly: AspectOrder( typeof(Aspect2), typeof(Aspect1) ) ]

[Layers(""Layer1"")]
class Aspect1 : TypeAspect { }

[Layers(""Layer1"")]
class Aspect2 : TypeAspect { }
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect1 => 0, Aspect1:Layer1 => 1, Aspect2 => 2, Aspect2:Layer1 => 3", ordered );
        }

        [Fact]
        public void TwoPartiallyOrderedDoubleLayerAspects()
        {
            var code = @"
using Metalama.Framework.Aspects;

[assembly: AspectOrder( ""Aspect2"", ""Aspect1"" ) ]

[Layers(""Layer1"")]
class Aspect1  : TypeAspect { }

[Layers(""Layer1"")]
class Aspect2  : TypeAspect { }
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect1 => 0, Aspect1:Layer1 => 1, Aspect2 => 1, Aspect2:Layer1 => 2", ordered );
        }

        [Fact]
        public void TwoTotallyOrderedDoubleLayerAspects()
        {
            var code = @"
using Metalama.Framework.Aspects;

[assembly: AspectOrder( ""Aspect2:Layer1"", ""Aspect1:Layer1"", ""Aspect2"", ""Aspect1"" ) ]

[Layers(""Layer1"")]
class Aspect1  : TypeAspect { }

[Layers(""Layer1"")]
class Aspect2  : TypeAspect { }
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect1 => 0, Aspect2 => 1, Aspect1:Layer1 => 2, Aspect2:Layer1 => 3", ordered );
        }

        [Fact]
        public void InheritedAspects()
        {
            var code = @"
using Metalama.Framework.Aspects;

[Layers(""Layer1"")]
class Aspect1  : TypeAspect { }

class Aspect2 : Aspect1 {}
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect1 => 0, Aspect2 => 0, Aspect1:Layer1 => 1, Aspect2:Layer1 => 1", ordered );
        }

        [Fact]
        public void InvalidAspectName()
        {
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Aspects;

[assembly: AspectOrder( ""NonExistent1"", ""Aspect1"" ) ]

class Aspect1 : TypeAspect { }

";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1" );
            Assert.Equal( "Aspect1 => 0", ordered );
        }

        [Fact]
        public void Cycle()
        {
            var code = @"
using Metalama.Framework.Aspects;

[assembly: AspectOrder( typeof(Aspect2), typeof(Aspect1) ) ]
[assembly: AspectOrder( typeof(Aspect1), typeof(Aspect2) ) ]

class Aspect1 : TypeAspect { }
class Aspect2 : TypeAspect { }
";

            var diagnostics = new DiagnosticList();
            Assert.False( this.TryGetOrderedAspectLayers( code, new[] { "Aspect1", "Aspect2" }, diagnostics, out _ ) );
            Assert.Single( diagnostics.Select( d => d.Id ), GeneralDiagnosticDescriptors.CycleInAspectOrdering.Id );
        }

        [Fact]
        public void Cycle2()
        {
            // The difference of Cycle2 compared to Cycle1 is that Aspect3 has no predecessor (in Cycle test, all nodes have a predecessor),
            // therefore the sort algorithm goes to another branch.

            var code = @"
using Metalama.Framework.Aspects;

[assembly: AspectOrder( typeof(Aspect2), typeof(Aspect1), typeof(Aspect3) ) ]
[assembly: AspectOrder( typeof(Aspect1), typeof(Aspect2) ) ]

class Aspect1 : TypeAspect { }

class Aspect2 : TypeAspect { }

class Aspect3 : TypeAspect { }
";

            var diagnostics = new DiagnosticList();
            Assert.False( this.TryGetOrderedAspectLayers( code, new[] { "Aspect1", "Aspect2", "Aspect3" }, diagnostics, out _ ) );
            Assert.Single( diagnostics.Select( d => d.Id ), GeneralDiagnosticDescriptors.CycleInAspectOrdering.Id );
        }
    }
}