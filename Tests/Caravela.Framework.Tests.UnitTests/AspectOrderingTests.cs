// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.TestFramework;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests
{
    public class AspectOrderingTests : TestBase
    {
        private bool TryGetOrderedAspectLayers( string code, string[] aspectNames, DiagnosticList diagnostics, [NotNullWhen( true )] out string? sortedAspects )
        {
            var compilation = CreateCompilationModel( code );

            using var isolatedTest = this.WithIsolatedTest();

            var compileTimeDomain = new UnloadableCompileTimeDomain();
            var loader = CompileTimeProjectLoader.Create( compileTimeDomain, isolatedTest.ServiceProvider );

            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    compilation.RoslynCompilation,
                    null,
                    new DiagnosticList(),
                    false,
                    CancellationToken.None,
                    out var compileTimeProject ) );

            var aspectTypeFactory = new AspectClassMetadataFactory(
                this.ServiceProvider,
                new AspectDriverFactory( this.ServiceProvider, compilation.RoslynCompilation, ImmutableArray<object>.Empty ) );

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
using Caravela.Framework.Aspects;
class Aspect1 : IAspect 
{
}
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1" );
            Assert.Equal( "Aspect1 => 0", ordered );
        }

        [Fact]
        public void OneDoubleLayerAspect()
        {
            var code = @"
using Caravela.Framework.Aspects;
class Aspect1 : IAspect
{
    public void BuildAspectClass( IAspectClassBuilder builder ) 
    {
        builder.Layers = System.Collections.Immutable.ImmutableArray.Create(""Layer1"");
    }
}
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1" );
            Assert.Equal( "Aspect1 => 0, Aspect1:Layer1 => 1", ordered );
        }

        [Fact]
        public void TwoUnorderedDoubleLayerAspects()
        {
            var code = @"
using Caravela.Framework.Aspects;
class Aspect1 : IAspect
{
    public void BuildAspectClass( IAspectClassBuilder builder ) 
    {
        builder.Layers = System.Collections.Immutable.ImmutableArray.Create(""Layer1"");
    }
}

class Aspect2 : IAspect
{
    public void BuildAspectClass( IAspectClassBuilder builder ) 
    {
        builder.Layers = System.Collections.Immutable.ImmutableArray.Create(""Layer1"");
    }
}
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect1 => 0, Aspect2 => 0, Aspect1:Layer1 => 1, Aspect2:Layer1 => 1", ordered );
        }

        [Fact]
        public void ThreeOrderedSingleLayerAspects()
        {
            var code = @"
using Caravela.Framework.Aspects;

[assembly: AspectOrder( typeof(Aspect2), typeof(Aspect1), typeof(Aspect3) ) ]

class Aspect3 : IAspect
{
    
}

class Aspect1 : IAspect
{
    
}

class Aspect2 : IAspect
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
using Caravela.Framework.Aspects;

[assembly: AspectOrder( typeof(Aspect2), typeof(Aspect1) ) ]

class Aspect1 : IAspect
{
    public void BuildAspectClass( IAspectClassBuilder builder ) 
    {
        builder.Layers = System.Collections.Immutable.ImmutableArray.Create(""Layer1"");
    }
}

class Aspect2 : IAspect
{
    public void BuildAspectClass( IAspectClassBuilder builder ) 
    {
        builder.Layers = System.Collections.Immutable.ImmutableArray.Create(""Layer1"");
    }
}
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect1 => 0, Aspect1:Layer1 => 1, Aspect2 => 2, Aspect2:Layer1 => 3", ordered );
        }

        [Fact]
        public void TwoPartiallyOrderedDoubleLayerAspects()
        {
            var code = @"
using Caravela.Framework.Aspects;

[assembly: AspectOrder( ""Aspect2"", ""Aspect1"" ) ]

class Aspect1  : IAspect
{
    public void BuildAspectClass( IAspectClassBuilder builder ) 
    {
        builder.Layers = System.Collections.Immutable.ImmutableArray.Create(""Layer1"");
    }
}

class Aspect2  : IAspect
{
    public void BuildAspectClass( IAspectClassBuilder builder ) 
    {
        builder.Layers = System.Collections.Immutable.ImmutableArray.Create(""Layer1"");
    }
}
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect1 => 0, Aspect1:Layer1 => 1, Aspect2 => 1, Aspect2:Layer1 => 2", ordered );
        }

        [Fact]
        public void TwoTotallyOrderedDoubleLayerAspects()
        {
            var code = @"
using Caravela.Framework.Aspects;

[assembly: AspectOrder( ""Aspect2:Layer1"", ""Aspect1:Layer1"", ""Aspect2"", ""Aspect1"" ) ]

class Aspect1  : IAspect
{
    public void BuildAspectClass( IAspectClassBuilder builder ) 
    {
        builder.Layers = System.Collections.Immutable.ImmutableArray.Create(""Layer1"");
    }
}

class Aspect2  : IAspect
{
    public void BuildAspectClass( IAspectClassBuilder builder ) 
    {
        builder.Layers = System.Collections.Immutable.ImmutableArray.Create(""Layer1"");
    }
}
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect1 => 0, Aspect2 => 1, Aspect1:Layer1 => 2, Aspect2:Layer1 => 3", ordered );
        }

        [Fact]
        public void InheritedAspects()
        {
            var code = @"
using Caravela.Framework.Aspects;

class Aspect1  : IAspect
{
    public void BuildAspectClass( IAspectClassBuilder builder ) 
    {
        builder.Layers = System.Collections.Immutable.ImmutableArray.Create(""Layer1"");
    }
}

class Aspect2 : Aspect1 {}
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect1 => 0, Aspect2 => 0, Aspect1:Layer1 => 1, Aspect2:Layer1 => 1", ordered );
        }

        [Fact]
        public void InvalidAspectName()
        {
            var code = @"
using Caravela.Framework.Aspects;
using Caravela.Framework.Aspects;

[assembly: AspectOrder( ""NonExistent1"", ""Aspect1"" ) ]

class Aspect1 : IAspect
{
}

";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1" );
            Assert.Equal( "Aspect1 => 0", ordered );
        }

        [Fact]
        public void Cycle()
        {
            var code = @"
using Caravela.Framework.Aspects;

[assembly: AspectOrder( typeof(Aspect2), typeof(Aspect1) ) ]
[assembly: AspectOrder( typeof(Aspect1), typeof(Aspect2) ) ]

class Aspect1 : IAspect
{
}

class Aspect2 : IAspect
{
}
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
using Caravela.Framework.Aspects;

[assembly: AspectOrder( typeof(Aspect2), typeof(Aspect1), typeof(Aspect3) ) ]
[assembly: AspectOrder( typeof(Aspect1), typeof(Aspect2) ) ]

class Aspect1 : IAspect
{
}

class Aspect2 : IAspect
{
}

class Aspect3 : IAspect
{
}
";

            var diagnostics = new DiagnosticList();
            Assert.False( this.TryGetOrderedAspectLayers( code, new[] { "Aspect1", "Aspect2", "Aspect3" }, diagnostics, out _ ) );
            Assert.Single( diagnostics.Select( d => d.Id ), GeneralDiagnosticDescriptors.CycleInAspectOrdering.Id );
        }
    }
}