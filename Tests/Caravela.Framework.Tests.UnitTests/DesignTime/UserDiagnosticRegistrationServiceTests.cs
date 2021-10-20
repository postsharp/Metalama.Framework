// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.TestFramework;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.DesignTime
{
    public class UserDiagnosticRegistrationServiceTests : TestBase
    {
        [Fact]
        public void TestUserErrorReporting()
        {
            this.TestUserDiagnosticsFileContent(
                aspectCode: @"
using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Tests.UnitTests.DesignTime.TestCode
{
    internal class ReportErrorAttribute : MethodAspect
    {
        private static readonly DiagnosticDefinition<IMethod> _userError = new(
             ""MY001"",
             Severity.Error,
             ""User error description."");

        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            builder.Diagnostics.Report(_userError, builder.Target);
        }
    }
}
",
                targetCode: @"

using System;

namespace Caravela.Framework.Tests.UnitTests.DesignTime.TestCode
{
    internal class TargetCode
    {
        [ReportError]
        public void Foo() { }
    }
}
",
                expectedUserDiagnosticsFileContent: @"{
  ""Diagnostics"": {
    ""MY001"": {
      ""Severity"": 3,
      ""Id"": ""MY001"",
      ""Category"": ""Caravela.User"",
      ""Title"": ""A Caravela user diagnostic.""
    }
  },
  ""Suppressions"": []
}" );
        }

        [Fact]
        public void TestDiagnosticSuppression()
        {
            this.TestUserDiagnosticsFileContent(
                aspectCode: @"
using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Tests.UnitTests.DesignTime.TestCode
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
            builder.Diagnostics.Suppress(builder.Target, _suppressionDefinition);
            builder.Diagnostics.Report(_userWarning, builder.Target);
        }
    }
}
",
                targetCode: @"

using System;

namespace Caravela.Framework.Tests.UnitTests.DesignTime.TestCode
{
    internal class TargetCode
    {
        [SuppressWarning]
        public void Foo() { }
    }
}
",
                expectedUserDiagnosticsFileContent: @"{
  ""Diagnostics"": {
    ""MY001"": {
      ""Severity"": 2,
      ""Id"": ""MY001"",
      ""Category"": ""Caravela.User"",
      ""Title"": ""A Caravela user diagnostic.""
    }
  },
  ""Suppressions"": [
    ""MY001""
  ]
}" );
        }

        private void TestUserDiagnosticsFileContent( string aspectCode, string targetCode, string expectedUserDiagnosticsFileContent )
        {
            using var testContext = this.CreateTestContext();

            var code = new Dictionary<string, string> { ["Aspect.cs"] = aspectCode, ["Class1.cs"] = targetCode };

            var compilation = CreateCSharpCompilation( code );

            using var domain = new UnloadableCompileTimeDomain();
            using DesignTimeAspectPipeline pipeline = new( testContext.ServiceProvider, domain, true );

            var diagnosticsFileName = Path.Combine( testContext.ProjectOptions.SettingsDirectory, "userDiagnostics.json" );

            Assert.False( File.Exists( diagnosticsFileName ) );
            Assert.True( pipeline.TryExecute( compilation, CancellationToken.None, out _ ) );

            var actualContent = File.ReadAllText( diagnosticsFileName );

            Assert.Equal( expectedUserDiagnosticsFileContent, actualContent );
        }
    }
}