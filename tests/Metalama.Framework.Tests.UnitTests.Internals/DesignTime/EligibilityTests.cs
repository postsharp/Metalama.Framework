// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.TestFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime
{
    public class EligibilityTests : TestBase, IDisposable
    {
        private readonly Dictionary<string, INamedDeclaration> _declarations;
        private readonly UnloadableCompileTimeDomain _domain;
        private readonly DesignTimeAspectPipeline _pipeline;
        private readonly CompilationModel _compilation;

        public EligibilityTests()
        {
            var code = @"
using System;
using Metalama.Framework.Code;
using Metalama.Framework.Aspects;
using Metalama.Framework.Eligibility;

class MethodAspect : IAspect<IMethod> { }
class StaticMethodAspect : IAspect<IMethod> { public void BuildEligibility( IEligibilityBuilder<IMethod> builder ) => builder.MustBeStatic(); }
class ConstructorAspect : IAspect<IConstructor> { }
class MethodBaseAspect : IAspect<IMethodBase> { }
class DeclarationAspect : IAspect<IDeclaration> { }
class ParameterAspect : IAspect<IParameter> { }
class GenericParameterAspect : IAspect<ITypeParameter> { }
class MyTypeAspect : TypeAspect {}

class Class<T>
{
  public Class() {}
  static Class() {}
  void Method( int p ) {}
  static void StaticMethod( int p ) {}
  int Field;
  string Property { get; set; }
  event EventHandler Event;
}

namespace Ns { class C {} }
";

            using var testContext = this.CreateTestContext();
            this._compilation = testContext.CreateCompilationModel( code );

            static string GetName( INamedDeclaration d )
                => d switch
                {
                    IParameter { IsReturnParameter: false } parameter => parameter.DeclaringMember.Name + "." + parameter.Name,
                    IParameter { IsReturnParameter: true } returnParameter => returnParameter.DeclaringMember.Name + "." + "return",
                    IConstructor { IsStatic: false } constructor => constructor.DeclaringType.Name + ".new",
                    IConstructor { IsStatic: true } constructor => constructor.DeclaringType.Name + ".static",
                    _ => d.Name
                };

            var declarationList = this._compilation
                .GetContainedDeclarations()
                .OfType<INamedDeclaration>()
                .Concat( this._compilation.GlobalNamespace.Namespaces )
                .ToList();

            this._declarations = declarationList
                .ToDictionary( GetName, d => d );

            this._domain = new UnloadableCompileTimeDomain();
            this._pipeline = new DesignTimeAspectPipeline( testContext.ServiceProvider, this._domain, this._compilation.RoslynCompilation, true );

            // Force the pipeline configuration to execute so the tests can do queries over it.
            TaskHelper.RunAndWait(
                () => this._pipeline.GetConfigurationAsync(
                    this._compilation.PartialCompilation,
                    NullDiagnosticAdder.Instance,
                    true,
                    CancellationToken.None ) );
        }

#if NET5_0_OR_GREATER
        [Theory]
#else
        [Theory( Skip = "Skipped in .NET Framework (low value)" )]

        // We would need to implement all interface methods.
#endif
        [InlineData( "Class", "DeclarationAspect,MyTypeAspect" )]
        [InlineData( "Class.new", "ConstructorAspect,DeclarationAspect,MethodBaseAspect" )]
        [InlineData( "Class.static", "ConstructorAspect,DeclarationAspect,MethodBaseAspect" )]
        [InlineData( "Method", "DeclarationAspect,MethodAspect,MethodBaseAspect" )]
        [InlineData( "StaticMethod", "DeclarationAspect,MethodAspect,MethodBaseAspect,StaticMethodAspect" )]
        [InlineData( "Method.p", "DeclarationAspect,ParameterAspect" )]
        [InlineData( "Field", "DeclarationAspect" )]
        [InlineData( "Property", "DeclarationAspect" )]
        [InlineData( "Event", "DeclarationAspect" )]
        public void IsEligible( string target, string aspects )
        {
            var targetSymbol = this._declarations[target].GetSymbol().AssertNotNull();

            var eligibleAspects = this._pipeline.GetEligibleAspects( this._compilation.RoslynCompilation, targetSymbol, CancellationToken.None )
                .Where( c => !c.Project!.IsFramework );

            var eligibleAspectsString = string.Join( ",", eligibleAspects.OrderBy( a => a.ShortName ) );

            Assert.Equal( aspects, eligibleAspectsString );
        }

        public void Dispose()
        {
            this._pipeline.Dispose();
            this._domain.Dispose();
        }
    }
}