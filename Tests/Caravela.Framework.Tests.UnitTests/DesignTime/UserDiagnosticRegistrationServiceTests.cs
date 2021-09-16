// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.DesignTime.Diagnostics;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.TestFramework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.DesignTime
{
    public class UserDiagnosticRegistrationServiceTests : TestBase
    {
        [Fact]
        public void TestUserErrorReporting()
        {
            var code = new Dictionary<string, string>
            {
                ["Aspect.cs"] = @"
using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Tests.UnitTests.DesignTime.TestCode
{
    internal class ReportErrorAttribute : OverrideMethodAspect
    {
        private static readonly DiagnosticDefinition<INamedType> _userError = new(
             ""MY001"",
             Severity.Error,
             ""User error description."");

        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            builder.Diagnostics.Report(_userError, builder.Target.DeclaringType);
            return;
        }

        public override dynamic? OverrideMethod()
        {
            return meta.Proceed();
        }
    }
}
",
                ["Class1.cs"] = @"

using System;

namespace Caravela.Framework.Tests.UnitTests.DesignTime.TestCode
{
    internal class TargetCode
    {
        [ReportError]
        public void Foo() { }
    }
}

"
            };

            var compilation = CreateCSharpCompilation( code );

            using var buildOptions = new TestProjectOptions();
            using var domain = new UnloadableCompileTimeDomain();
            DesignTimeAspectPipeline pipeline = new( buildOptions, domain, true, directoryOptions: buildOptions );
            
            var syntaxTree = compilation.SyntaxTrees.Single( t => t.FilePath == "Class1.cs" );

            var diagnosticsFileName = Path.Combine( buildOptions.SettingsDirectory, "userDiagnostics.json" );
            
            Assert.False( File.Exists( diagnosticsFileName ) );
            var result = pipeline.Execute( PartialCompilation.CreatePartial( compilation, syntaxTree ), CancellationToken.None );

            Assert.False( result.Success );

            var actualContent = File.ReadAllText( diagnosticsFileName );
            
            Assert.Equal(
                @"{
  ""Diagnostics"": {
    ""MY001"": {
      ""Severity"": 3,
      ""Id"": ""MY001"",
      ""Category"": ""Caravela.User"",
      ""Title"": ""A Caravela user diagnostic.""
    }
  },
  ""Suppressions"": []
}",
                actualContent! );
        }
    }
}