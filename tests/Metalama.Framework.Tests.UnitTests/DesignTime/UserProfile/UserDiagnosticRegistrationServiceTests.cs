// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Testing.UnitTesting;
using System.Collections.Generic;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.UserProfile
{
    public sealed class UserDiagnosticRegistrationServiceTests : DesignTimeTestBase
    {
        protected override void ConfigureServices( IAdditionalServiceCollection services )
        {
            base.ConfigureServices( services );
            ((AdditionalServiceCollection) services).BackstageServices.Add<IConfigurationManager>( sp => new InMemoryConfigurationManager( sp ), true );
        }

        [Fact]
        public void TestUserErrorReporting()
        {
            var output =
                this.GetUserDiagnosticsFileContent(
                    aspectCode: @"
using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.TestCode
{
    internal class ReportErrorAttribute : MethodAspect
    {
        private static readonly DiagnosticDefinition<IMethod> _userError = new(
             ""MY001"",
             Severity.Error,
             ""User error description."");

        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
builder.Diagnostics.Report(             _userError.WithArguments( builder.Target ) );
        }
    }
}
",
                    targetCode: @"

using System;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.TestCode
{
    internal class TargetCode
    {
        [ReportError]
        public void Foo() { }
    }
}
" );

            Assert.Single( output.Diagnostics );
            Assert.Empty( output.Suppressions );
        }

        [Fact]
        public void TestDiagnosticSuppression()
        {
            var output =
                this.GetUserDiagnosticsFileContent(
                    aspectCode: @"
using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.TestCode
{
    internal class SuppressWarningAttribute : MethodAspect
    {
        private static readonly DiagnosticDefinition<IMethod> _userWarning = new(
             ""MY001"",
             Severity.Warning,
             ""User warning description."");

        private static readonly SuppressionDefinition _suppressionDefinition = new(""MY001"");

        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            builder.Diagnostics.Suppress(_suppressionDefinition);
            builder.Diagnostics.Report( _userWarning.WithArguments(builder.Target) ); 
        }
    }
}
",
                    targetCode: @"

using System;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.TestCode
{
    internal class TargetCode
    {
        [SuppressWarning]
        public void Foo() { }
    }
}
" );

            Assert.Single( output.Diagnostics );
            Assert.Single( output.Suppressions );
        }

        private UserDiagnosticsConfiguration GetUserDiagnosticsFileContent( string aspectCode, string targetCode )
        {
            using var testContext = this.CreateTestContext();

            var code = new Dictionary<string, string> { ["Aspect.cs"] = aspectCode, ["Class1.cs"] = targetCode };

            var compilation = TestCompilationFactory.CreateCSharpCompilation( code );

            // Create a service provider with our own configuration manager.
            var configurationManager = testContext.ServiceProvider.Global.GetRequiredBackstageService<IConfigurationManager>();
            Assert.IsType<InMemoryConfigurationManager>( configurationManager );
            var serviceProvider = testContext.ServiceProvider.Global.Underlying.WithUntypedService( typeof(IConfigurationManager), configurationManager );

            using var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext, serviceProvider );
            using var pipeline = pipelineFactory.CreatePipeline( compilation );

            Assert.True( pipeline.TryExecute( compilation, default, out _ ) );

            return configurationManager.Get<UserDiagnosticsConfiguration>();
        }
    }
}