// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.TestFramework;
using System.Linq;
using System.Threading;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.DesignTime
{
    public class LiveTemplateAspectPipelineTests : TestBase
    {
        // TODO: rename
        [Fact]
        public void FirstTest()
        {
            var compilation = CreateCompilationModel( 
                @"
using Caravela.Framework.Code;
using Caravela.Framework.Aspects;
class Aspect : IAspect<INamedType> {}
class C{}" );
            
            using var buildOptions = new TestProjectOptions();
            using var domain = new UnloadableCompileTimeDomain();
            var cache = new DesignTimeAspectPipelineCache( domain, true );

            cache.GetOrCreatePipeline( buildOptions, CancellationToken.None )
                ?.TryGetConfiguration(
                    PartialCompilation.CreateComplete( compilation.RoslynCompilation ),
                    NullDiagnosticAdder.Instance,
                    true,
                    CancellationToken.None,
                    out _ );
            
            var target = compilation.DeclaredTypes.OfName( "C" ).Single().GetSymbol();
            var aspect = cache.GetEligibleAspects( target, buildOptions, CancellationToken.None ).Single();

            var success = cache.TryApplyAspectToCode( buildOptions, aspect, compilation.RoslynCompilation, target, CancellationToken.None, out var outputCompilation, out var diagnostics );
            
            Assert.True( success );
            Assert.Empty( diagnostics );
            Assert.Equal( "", outputCompilation!.ToString() );
        }
    }
}