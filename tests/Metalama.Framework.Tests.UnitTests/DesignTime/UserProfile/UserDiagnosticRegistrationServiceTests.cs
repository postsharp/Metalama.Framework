// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Configuration;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Testing.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using Metalama.Framework.Advising; 
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
        public void TestUserErrorFromDependency()
        {
            var log = new StringBuilder();

            this.GetUserDiagnosticsFileContent(
                dependentCode: """
                               using System;
                               using Metalama.Framework.Advising;
                               using Metalama.Framework.Aspects; 
                               using Metalama.Framework.Code;
                               using Metalama.Framework.Diagnostics;

                               namespace Metalama.Framework.Tests.UnitTests.DesignTime.TestCode;

                               public class ReportErrorAttribute : MethodAspect
                               {
                                   private static readonly DiagnosticDefinition<IMethod> _userError = new(
                                           "MY001",
                                           Severity.Error,
                                           "User error description.");
                               
                                   public override void BuildAspect(IAspectBuilder<IMethod> builder)
                                   {
                                       builder.Diagnostics.Report( _userError.WithArguments( builder.Target ) );
                                   }
                               }
                               """,
                targetCode: """
                            namespace Metalama.Framework.Tests.UnitTests.DesignTime.TestCode;

                            class TargetCode
                            {
                                [ReportError]
                                public void Foo() { }
                            }
                            """,
                aspectCode: "",
                configurationFileChanged: ( configurationManager, file ) =>
                {
                    if ( file is UserDiagnosticsConfiguration { Diagnostics.IsEmpty: false } userDiagnostics )
                    {
                        log.AppendLine( string.Join( ", ", userDiagnostics.Diagnostics.SelectAsReadOnlyCollection( d => d.Value.Id ) ) );

                        configurationManager.Set( new UserDiagnosticsConfiguration() );
                    }
                } );

            // Verify that the diagnostic is reported for both compilations.
            AssertEx.EolInvariantEqual(
                """
                MY001
                MY001

                """,
                log.ToString() );
        }

        [Fact]
        public void TestPropertyUserErrorFromDependency()
        {
            var log = new StringBuilder();

            this.GetUserDiagnosticsFileContent(
                dependentCode: """
                               using System;
                               using Metalama.Framework.Advising;
                               using Metalama.Framework.Aspects; 
                               using Metalama.Framework.Code;
                               using Metalama.Framework.Diagnostics;

                               namespace Metalama.Framework.Tests.UnitTests.DesignTime.TestCode;

                               public class ReportErrorAttribute : MethodAspect
                               {
                                   private static DiagnosticDefinition<IMethod> UserError { get; } = new(
                                           "MY001",
                                           Severity.Error,
                                           "User error description.");
                               
                                   public override void BuildAspect(IAspectBuilder<IMethod> builder)
                                   {
                                       builder.Diagnostics.Report( UserError.WithArguments( builder.Target ) );
                                   }
                               }
                               """,
                targetCode: """
                            namespace Metalama.Framework.Tests.UnitTests.DesignTime.TestCode;

                            class TargetCode
                            {
                                [ReportError]
                                public void Foo() { }
                            }
                            """,
                aspectCode: "",
                configurationFileChanged: ( configurationManager, file ) =>
                {
                    if ( file is UserDiagnosticsConfiguration { Diagnostics.IsEmpty: false } userDiagnostics )
                    {
                        log.AppendLine( string.Join( ", ", userDiagnostics.Diagnostics.SelectAsReadOnlyCollection( d => d.Value.Id ) ) );

                        configurationManager.Set( new UserDiagnosticsConfiguration() );
                    }
                } );

            // Verify that the diagnostic is reported for both compilations.
            AssertEx.EolInvariantEqual(
                """
                MY001
                MY001

                """,
                log.ToString() );
        }

        [Fact]
        public void TestDiagnosticSuppression()
        {
            var output =
                this.GetUserDiagnosticsFileContent(
                    aspectCode: @"
using System;
using Metalama.Framework.Advising; 
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

        private UserDiagnosticsConfiguration GetUserDiagnosticsFileContent(
            string aspectCode,
            string targetCode,
            string? dependentCode = null,
            Action<InMemoryConfigurationManager, ConfigurationFile>? configurationFileChanged = null )
        {
            using var testContext = this.CreateTestContext();

            var code = new Dictionary<string, string> { ["Aspect.cs"] = aspectCode, ["Class1.cs"] = targetCode };

            var compilation = TestCompilationFactory.CreateCSharpCompilation( code, dependentCode );

            // Create a service provider with our own configuration manager.
            var configurationManager = testContext.ServiceProvider.Global.GetRequiredBackstageService<IConfigurationManager>();
            var inMemoryConfigurationManager = Assert.IsType<InMemoryConfigurationManager>( configurationManager );
            inMemoryConfigurationManager.ConfigurationFileChanged += file => configurationFileChanged?.Invoke( inMemoryConfigurationManager, file );
            var serviceProvider = testContext.ServiceProvider.Global.Underlying.WithUntypedService( typeof(IConfigurationManager), configurationManager );

            using var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext, serviceProvider );
            using var pipeline = pipelineFactory.CreatePipeline( compilation );

            Assert.True( pipeline.TryExecute( compilation, default, out _ ) );

            return configurationManager.Get<UserDiagnosticsConfiguration>();
        }
    }
}