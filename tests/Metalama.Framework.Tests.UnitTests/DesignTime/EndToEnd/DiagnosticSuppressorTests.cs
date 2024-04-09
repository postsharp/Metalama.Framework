﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Tests.UnitTests.DesignTime.Mocks;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.EndToEnd;

#pragma warning disable VSTHRD200

public sealed class DiagnosticSuppressorTests : UnitTestClass
{
    protected override void ConfigureServices( IAdditionalServiceCollection services )
    {
        base.ConfigureServices( services );
        services.AddGlobalService<IUserDiagnosticRegistrationService>( new TestUserDiagnosticRegistrationService() );
    }

    private async Task<List<Suppression>> ExecuteSuppressorAsync( string code, string diagnosticId )
    {
        using var testContext = this.CreateTestContext();

        var pipelineFactory = new TestDesignTimeAspectPipelineFactory( testContext );

        var workspaceProvider = new TestWorkspaceProvider( testContext.ServiceProvider );
        workspaceProvider.AddOrUpdateProject( "project", new Dictionary<string, string>() { ["code.cs"] = code } );
        var compilation = await workspaceProvider.GetProject( "project" ).GetCompilationAsync();
        var diagnostics = compilation!.GetDiagnostics();

        var suppressor = new TheDiagnosticSuppressor( pipelineFactory.ServiceProvider );
        var analysisContext = new TestSuppressionAnalysisContext( compilation, diagnostics, testContext.ProjectOptions );

        suppressor.ReportSuppressions(
            analysisContext,
            ImmutableDictionary<string, SuppressionDescriptor>.Empty.Add( diagnosticId, new SuppressionDescriptor( diagnosticId, diagnosticId, "Because" ) ) );

        return analysisContext.ReportedSuppressions;
    }

    [Fact]
    public async Task SuppressVariableLevelWarning()
    {
        const string code = """
                            using Metalama.Framework.Aspects;
                            using Metalama.Framework.Code;
                            using Metalama.Framework.Diagnostics;

                            namespace Metalama.Framework.Tests.Integration.Aspects.Suppressions.Methods
                            {
                                public class SuppressWarningAttribute : MethodAspect
                                {
                                    private static readonly SuppressionDefinition _suppression1 = new( "CS0219" );
                            
                                    public override void BuildAspect( IAspectBuilder<IMethod> builder )
                                    {
                                        builder.Diagnostics.Suppress( _suppression1, builder.Target );
                                    }
                                }
                            
                                // <target>
                                internal class TargetClass
                                {
                                    [SuppressWarning]
                                    private void M2( string m )
                                    {
                                        var x = 0;
                                    }
                            
                                    // CS0219 expected
                                    private void M1( string m )
                                    {
                                        var x = 0;
                                    }
                                }
                            }
                            """;

        var suppressions = await this.ExecuteSuppressorAsync( code, "CS0219" );

        var suppression = Assert.Single( suppressions );

        Assert.Equal( "code.cs(23,17): warning CS0219: The variable 'x' is assigned but its value is never used", suppression.SuppressedDiagnostic.ToString() );
    }

    [Fact]
    public async Task SuppressFieldLevelWarning()
    {
        const string code = """
                            using Metalama.Framework.Aspects;
                            using Metalama.Framework.Code;
                            using Metalama.Framework.Diagnostics;

                            namespace Metalama.Framework.Tests.Integration.Aspects.Suppressions.Methods
                            {
                                public class SuppressWarningAttribute : FieldAspect
                                {
                                    private static readonly SuppressionDefinition _suppression1 = new( "CS0169" );
                            
                                    public override void BuildAspect( IAspectBuilder<IField> builder )
                                    {
                                        builder.Diagnostics.Suppress( _suppression1, builder.Target );
                                    }
                                }
                            
                                // <target>
                                internal class TargetClass
                                {
                                    [SuppressWarning]
                                    int _field;
                                }
                            }
                            """;

        var suppressions = await this.ExecuteSuppressorAsync( code, "CS0169" );

        var suppression = Assert.Single( suppressions );

        Assert.Equal( "code.cs(21,13): warning CS0169: The field 'TargetClass._field' is never used", suppression.SuppressedDiagnostic.ToString() );
    }

    [Fact]
    public async Task ParametricSuppression()
    {
        const string code = """
            using Metalama.Framework.Aspects;
            using Metalama.Framework.Code;
            using Metalama.Framework.Diagnostics;
            using System.Linq;

            class SuppressWarningAttribute : ConstructorAspect
            {
                private static readonly SuppressionDefinition _suppression = new("CS8618");

                public override void BuildAspect(IAspectBuilder<IConstructor> builder)
                {
                    builder.Diagnostics.Suppress(
                        _suppression.WithFilter(static diag => diag.Arguments.Any(arg => arg is string s && s == "o1")), builder.Target);
                }
            }

            class TargetClass
            {
                object o1;
                object o2;

                [SuppressWarning]
                public TargetClass() { }
            }

            class AnotherClass
            {
                object o1;
                object o2;

                public AnotherClass() { }
            }
            """;

        var suppressions = await this.ExecuteSuppressorAsync( code, "CS8618" );

        var suppression = Assert.Single( suppressions );

        Assert.Equal( "code.cs(23,12): warning CS8618: Non-nullable field 'o1' must contain a non-null value when exiting constructor. Consider declaring the field as nullable.", suppression.SuppressedDiagnostic.ToString() );
    }
}