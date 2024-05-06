// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Testing.UnitTesting;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.Framework.Tests.UnitTests
{
    public sealed class AspectOrderingTests : UnitTestClass
    {
        public AspectOrderingTests( ITestOutputHelper logger ) : base( logger, false ) { }
        

        private bool TryGetOrderedAspectLayers( string code, string[] aspectNames, DiagnosticBag diagnostics, [NotNullWhen( true )] out string? sortedAspects )
        {
            using var testContext = this.CreateTestContext();

            var compilation = testContext.CreateCompilationModel( code );

            var serviceProvider = testContext.ServiceProvider;

            var compileTimeDomain = testContext.Domain;

            var compileTimeProjectRepository = CompileTimeProjectRepository.Create(
                    compileTimeDomain,
                    serviceProvider,
                    compilation.RoslynCompilation,
                    NullDiagnosticAdder.Instance )
                .AssertNotNull();

            Assert.NotNull( compileTimeProjectRepository );

            var compileTimeProject = compileTimeProjectRepository.RootProject;

            serviceProvider = serviceProvider.WithCompileTimeProjectServices( compileTimeProjectRepository );

            var aspectTypeFactory = new AspectClassFactory(
                new AspectDriverFactory( compilation, ImmutableArray<object>.Empty, serviceProvider ),
                compilation.CompilationContext );

            var aspectNamedTypes = aspectNames.SelectAsImmutableArray( name => compilation.Types.OfName( name ).Single().GetSymbol() );

            var aspectTypes = aspectTypeFactory.GetClasses(
                    serviceProvider,
                    compilation.CompilationContext,
                    aspectNamedTypes,
                    compileTimeProject,
                    diagnostics )
                .ToImmutableArray();

            var dependencies = new IAspectOrderingSource[]
            {
                new AspectLayerOrderingSource( aspectTypes ), new AttributeAspectOrderingSource( serviceProvider, compilation.RoslynCompilation )
            };

            if ( AspectLayerSorter.TrySort(
                    aspectTypes,
                    dependencies, // We don't apply alphabetical ordering for better testing.
                    diagnostics,
                    out var sortedAspectLayers ) )
            {
                sortedAspects = string.Join(
                    ", ",
                    sortedAspectLayers.OrderBy( l => l.Order ).ThenBy( l => l.AspectName ) );

                this.TestOutput.WriteLine( sortedAspects );
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
            var diagnostics = new DiagnosticBag();
            Assert.True( this.TryGetOrderedAspectLayers( code, aspectNames, diagnostics, out var sortedAspects ), "A cycle was detected." );
            Assert.Empty( diagnostics );

            return sortedAspects;
        }

        [Fact]
        public void OneSingleLayerAspect()
        {
            const string code = @"
using Metalama.Framework.Aspects;
class Aspect1 : TypeAspect { }
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1" );
            Assert.Equal( "Aspect1 => 0", ordered );
        }

        [Fact]
        public void OneDoubleLayerAspect()
        {
            const string code = @"
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
            const string code = @"
using Metalama.Framework.Aspects;
[Layers(""Layer1"")]
class Aspect1 : TypeAspect { }

[Layers(""Layer1"")]
class Aspect2 : TypeAspect { }
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect2 => 0, Aspect1 => 0, Aspect2:Layer1 => 1, Aspect1:Layer1 => 1", ordered );
        }

        [Fact]
        public void ThreeOrderedSingleLayerAspects()
        {
            const string code = @"
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
            const string code = @"
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
            const string code = @"
using Metalama.Framework.Aspects;

[assembly: AspectOrder( ""Aspect2"", ""Aspect1"" ) ]

[Layers(""Layer1"")]
class Aspect1  : TypeAspect { }

[Layers(""Layer1"")]
class Aspect2  : TypeAspect { }
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect1 => 0, Aspect2 => 1, Aspect1:Layer1 => 1, Aspect2:Layer1 => 2", ordered );
        }

        [Fact]
        public void TwoTotallyOrderedDoubleLayerAspects()
        {
            const string code = @"
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
            const string code = @"
using Metalama.Framework.Aspects;

[Layers(""Layer1"")]
class Aspect1  : TypeAspect { }

class Aspect2 : Aspect1 {}
";

            var ordered = this.GetOrderedAspectLayers( code, "Aspect1", "Aspect2" );
            Assert.Equal( "Aspect2 => 0, Aspect1 => 0, Aspect2:Layer1 => 1, Aspect1:Layer1 => 1", ordered );
        }
        
        [Fact]
        public void ApplyToDerivedTypes()
        {
            const string code = @"
using Metalama.Framework.Aspects;

[assembly: AspectOrder( ""Aspect10"", ""Aspect30"", ApplyToDerivedTypes = true ) ]

class Aspect00 : Aspect10 {}

class Aspect10  : TypeAspect { }

class Aspect20 : Aspect10 {}

class Aspect30  : TypeAspect { }
";

            var ordered = this.GetOrderedAspectLayers( code,  "Aspect00", "Aspect10", "Aspect20", "Aspect30" );
            Assert.Equal( "Aspect30 => 0, Aspect20 => 1, Aspect10 => 1, Aspect00 => 1", ordered );
        }

        [Fact]
        public void InvalidAspectName()
        {
            const string code = @"
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
            const string code = @"
using Metalama.Framework.Aspects;

[assembly: AspectOrder( typeof(Aspect2), typeof(Aspect1) ) ]
[assembly: AspectOrder( typeof(Aspect1), typeof(Aspect2) ) ]

class Aspect1 : TypeAspect { }
class Aspect2 : TypeAspect { }
";

            var diagnostics = new DiagnosticBag();
            Assert.False( this.TryGetOrderedAspectLayers( code, new[] { "Aspect1", "Aspect2" }, diagnostics, out _ ) );
            Assert.Single( diagnostics.SelectAsReadOnlyCollection( d => d.Id ), GeneralDiagnosticDescriptors.CycleInAspectOrdering.Id );
        }

        [Fact]
        public void Cycle2()
        {
            // The difference of Cycle2 compared to Cycle1 is that Aspect3 has no predecessor (in Cycle test, all nodes have a predecessor),
            // therefore the sort algorithm goes to another branch.

            const string code = @"
using Metalama.Framework.Aspects;

[assembly: AspectOrder( typeof(Aspect2), typeof(Aspect1), typeof(Aspect3) ) ]
[assembly: AspectOrder( typeof(Aspect1), typeof(Aspect2) ) ]

class Aspect1 : TypeAspect { }

class Aspect2 : TypeAspect { }

class Aspect3 : TypeAspect { }
";

            var diagnostics = new DiagnosticBag();
            Assert.False( this.TryGetOrderedAspectLayers( code, new[] { "Aspect1", "Aspect2", "Aspect3" }, diagnostics, out _ ) );
            Assert.Single( diagnostics.SelectAsReadOnlyCollection( d => d.Id ), GeneralDiagnosticDescriptors.CycleInAspectOrdering.Id );
        }
    }
}