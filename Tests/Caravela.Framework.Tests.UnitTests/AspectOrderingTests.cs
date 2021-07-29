// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.TestFramework;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests
{
    public class AspectOrderingTests : TestBase
    {
        private string GetOrderedAspectLayers( string code, params string[] aspectNames )
        {
            var compilation = CreateCompilationModel( code );
            DiagnosticList diagnostics = new();

            using var isolatedTest = this.WithIsolatedTest();

            var compileTimeDomain = new UnloadableCompileTimeDomain();
            var loader = CompileTimeProjectLoader.Create( compileTimeDomain, isolatedTest.ServiceProvider );

            Assert.True(
                loader.TryGetCompileTimeProject(
                    compilation.RoslynCompilation,
                    null,
                    new DiagnosticList(),
                    false,
                    CancellationToken.None,
                    out var compileTimeProject ) );

            var aspectTypeFactory = new AspectClassMetadataFactory(
                this.ServiceProvider,
                new AspectDriverFactory( this.ServiceProvider, compilation.RoslynCompilation, ImmutableArray<object>.Empty ) );

            var aspectNamedTypes = aspectNames.Select( name => compilation.DeclaredTypes.OfName( name ).Single().GetSymbol() ).ToReadOnlyList();
            var aspectTypes = aspectTypeFactory.GetAspectClasses( aspectNamedTypes, compileTimeProject!, diagnostics ).ToImmutableArray();
            var allLayers = aspectTypes.SelectMany( a => a.Layers ).ToImmutableArray();

            var dependencies = new IAspectOrderingSource[]
            {
                new AspectLayerOrderingSource( aspectTypes ), new AttributeAspectOrderingSource( compilation.RoslynCompilation, loader )
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
    }
}