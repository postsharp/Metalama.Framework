﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Engine.Utilities.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public class SourceGeneratorIntegrationTests : LoggingTestBase
{
    [Fact]
    public void ChangeInDependency_ChangeNotification()
    {
        // This is to avoid a complete freeze of the test agent in case of bug.
        var timeout = new CancellationTokenSource( TimeSpan.FromMinutes( 1 ) ).Token;

        var dependentProjectKey = ProjectKeyFactory.CreateTest( "Dependent" );
        var masterProjectKey = ProjectKeyFactory.CreateTest( "Master" );

        var mocks = new TestServiceCollection();
        using var testContext = this.CreateTestContext( new TestProjectOptions( hasSourceGeneratorTouchFile: true ), mocks );

        using TestDesignTimeAspectPipelineFactory factory = new( testContext );

        var dependentCode = new Dictionary<string, string>()
        {
            ["dependent.cs"] = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

class MyAspect : TypeAspect
{
   public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
   {
        foreach ( var field in aspectBuilder.Target.BaseType.Fields )
        {
            aspectBuilder.Advice.IntroduceField( aspectBuilder.Target,  field.Name + ""Plus"", field.Type );
        }
   }
}

[MyAspect]
partial class C : BaseClass
{
   
}
"
        };

        // First compilation of everything.
        var masterCode1 = new Dictionary<string, string>() { ["master.cs"] = @"public class BaseClass { public int Field1; }" };

        var masterCompilation1 = CreateCSharpCompilation( masterCode1, name: masterProjectKey.AssemblyName );

        var dependentCompilation1 = CreateCSharpCompilation(
            dependentCode,
            name: dependentProjectKey.AssemblyName,
            additionalReferences: new[] { masterCompilation1.ToMetadataReference() } );

        Assert.True( factory.TryExecute( testContext.ProjectOptions, dependentCompilation1, TestableCancellationToken.None, out var results1 ) );

        BlockingCollection<ProjectKey> dirtyProjectNotifications = new();
        factory.EventHub.DirtyProject += project => dirtyProjectNotifications.Add( project, timeout );

        Assert.Single( results1!.TransformationResult.IntroducedSyntaxTrees );
        Assert.Contains( "Field1Plus", results1.TransformationResult.IntroducedSyntaxTrees.Single().Value.GeneratedSyntaxTree.ToString(), StringComparison.Ordinal );

        // Second compilation of the master compilation.
        var masterCode2 = new Dictionary<string, string>() { ["master.cs"] = @"public partial class BaseClass { public int Field2; }" };

        var masterCompilation2 = CreateCSharpCompilation( masterCode2, name: "Master" );

        Assert.True( factory.TryExecute( testContext.ProjectOptions, masterCompilation2, TestableCancellationToken.None, out _ ) );

        var notification = dirtyProjectNotifications.Take();
        
        Assert.Equal( dependentProjectKey.AssemblyName, notification.AssemblyName );
    }
}