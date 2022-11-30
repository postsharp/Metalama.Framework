// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#if NET5_0_OR_GREATER
using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Testing.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime
{
    // We skip this test in .NET Framework because we would need to implement all implicit interface methods, and it would have low value anyway.
    public class EligibilityTests : UnitTestSuite, IDisposable
    {
        private readonly Dictionary<string, INamedDeclaration> _declarations;
        private readonly DesignTimeAspectPipeline _pipeline;
        private readonly CompilationModel _compilation;
        private readonly TestDesignTimeAspectPipelineFactory _pipelineFactory;
        private readonly TestContext _testContext;

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
  void Method( int intParameter, string stringParameter ) {}
  static void StaticMethod( int p ) {}
  int Field;
  string Property { get; set; }
  event EventHandler Event;
}

namespace Ns { class C {} }
";

            this._testContext = this.CreateTestContext();
            this._compilation = this._testContext.CreateCompilationModel( code );

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

            this._pipelineFactory = new TestDesignTimeAspectPipelineFactory( this._testContext );
            this._pipeline = this._pipelineFactory.CreatePipeline( this._compilation.RoslynCompilation );

            // Force the pipeline configuration to execute so the tests can do queries over it.
            TaskHelper.RunAndWait(
                () => this._pipeline.GetConfigurationAsync(
                    this._compilation.PartialCompilation,
                    true,
                    default ) );
        }

        [Theory]
        [InlineData( "Class", "DeclarationAspect,MyTypeAspect" )]
        [InlineData( "Class.new", "ConstructorAspect,DeclarationAspect,MethodBaseAspect" )]
        [InlineData( "Class.static", "ConstructorAspect,DeclarationAspect,MethodBaseAspect" )]
        [InlineData( "Method", "DeclarationAspect,MethodAspect,MethodBaseAspect" )]
        [InlineData( "StaticMethod", "DeclarationAspect,MethodAspect,MethodBaseAspect,StaticMethodAspect" )]
        [InlineData( "Method.intParameter", "DeclarationAspect,ParameterAspect" )]
        [InlineData( "Field", "DeclarationAspect" )]
        [InlineData( "Property", "DeclarationAspect" )]
        [InlineData( "Event", "DeclarationAspect" )]
        public void IsEligible( string target, string aspects )
        {
            var targetSymbol = this._declarations[target].GetSymbol().AssertNotNull();

            var eligibleAspects = this._pipeline.GetEligibleAspects( this._compilation.RoslynCompilation, targetSymbol, default )
                .Where( c => !c.Project!.IsFramework );

            var eligibleAspectsString = string.Join( ",", eligibleAspects.OrderBy( a => a.ShortName ) );

            Assert.Equal( aspects, eligibleAspectsString );
        }

        [Fact]
        public void MustBeOfType_ThrowsWhenImpossible()
        {
            Assert.Throws<ArgumentOutOfRangeException>( () => EligibilityRuleFactory.CreateRule<IType>( d => d.MustBeOfType( typeof(int) ) ) );
        }

        [Fact]
        public void MustBeOfAnyType_ThrowsWhenImpossible()
        {
            Assert.Throws<ArgumentOutOfRangeException>( () => EligibilityRuleFactory.CreateRule<IType>( d => d.MustBeOfAnyType( typeof(int) ) ) );
        }

        [Fact]
        public void MustBeOfType()
        {
            var eligibility = EligibilityRuleFactory.CreateRule<IType>( d => d.MustBeOfType( typeof(INamedType) ) );
            var intParameter = ((IMethod) this._declarations["Method"]).Parameters[0];
            Assert.Equal( EligibleScenarios.All, eligibility.GetEligibility( intParameter.Type ) );
        }

        [Fact]
        public void MustBeOfAnyType()
        {
            var eligibility = EligibilityRuleFactory.CreateRule<IType>( d => d.MustBeOfAnyType( typeof(INamedType), typeof(IArrayType) ) );
            var intParameter = ((IMethod) this._declarations["Method"]).Parameters[0];
            Assert.Equal( EligibleScenarios.All, eligibility.GetEligibility( intParameter.Type ) );
        }

        [Fact]
        public void MustBe()
        {
            var eligibility = EligibilityRuleFactory.CreateRule<IType>( d => d.MustBe( typeof(int) ) );
            var intParameter = ((IMethod) this._declarations["Method"]).Parameters[0];
            var stringParameter = ((IMethod) this._declarations["Method"]).Parameters[1];

            Assert.Equal( EligibleScenarios.All, eligibility.GetEligibility( intParameter.Type ) );
            Assert.Equal( EligibleScenarios.None, eligibility.GetEligibility( stringParameter.Type ) );
        }

        public void Dispose()
        {
            this._pipeline.Dispose();
            this._pipelineFactory.Dispose();
            this._testContext.Dispose();
        }
    }
}
#endif