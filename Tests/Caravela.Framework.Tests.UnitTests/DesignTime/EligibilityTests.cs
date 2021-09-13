// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.DesignTime
{
    public class EligibilityTests : TestBase
    {
        private readonly Dictionary<string, INamedDeclaration> _declarations;
        private readonly TestProjectOptions _buildOptions;
        private readonly UnloadableCompileTimeDomain _domain;
        private readonly Dictionary<string, AspectClass> _aspects;

        public EligibilityTests()
        {
            var code = @"
using System;
using Caravela.Framework.Code;
using Caravela.Framework.Aspects;

class MethodAspect : IAspect<IMethod> { }
class ConstructorAspect : IAspect<IConstructor> { }
class MethodBaseAspect : IAspect<IMethodBase> { }
class DeclarationAspect : IAspect<IDeclaration> { }
class ParameterAspect : IAspect<IParameter> { }
class GenericParameterAspect : IAspect<IGenericParameter> { }

class Class<T>
{
  public Class() {}
  static Class() {}
  void Method( int p ) {}
  int Field;
  string Property { get; set; }
  event EventHandler Event;
}
";

            var compilation = CreateCompilationModel( code );

            static string GetName( INamedDeclaration d )
                => d switch
                {
                    IParameter { IsReturnParameter: false } parameter => parameter.DeclaringMember.Name + "." + parameter.Name,
                    IParameter { IsReturnParameter: true } returnParameter => returnParameter.DeclaringMember.Name + "." + "return",
                    IConstructor { IsStatic: false } constructor => constructor.DeclaringType.Name + ".new",
                    IConstructor { IsStatic: true } constructor => constructor.DeclaringType.Name + ".static",
                    _ => d.Name
                };

            var declarationList = compilation
                .GetContainedDeclarations()
                .OfType<INamedDeclaration>()
                .ToList();

            this._declarations = declarationList
                .ToDictionary( GetName, d => d );

            this._buildOptions = new TestProjectOptions();
            this._domain = new UnloadableCompileTimeDomain();
            DesignTimeAspectPipeline pipeline = new( this._buildOptions, this._domain, true );

            pipeline.TryGetConfiguration(
                PartialCompilation.CreateComplete( compilation.RoslynCompilation ),
                NullDiagnosticAdder.Instance,
                true,
                CancellationToken.None,
                out var configuration );

            this._aspects = configuration!.AspectClasses.ToDictionary( a => a.DisplayName, a => a );
        }

        [Theory]
        [InlineData( "MethodAspect", "Class", false )]
        [InlineData( "MethodAspect", "Class.new", false )]
        [InlineData( "MethodAspect", "Class.static", false )]
        [InlineData( "MethodAspect", "Method", true )]
        [InlineData( "MethodAspect", "Method.p", false )]
        [InlineData( "MethodAspect", "Field", false )]
        [InlineData( "MethodAspect", "Property", false )]
        [InlineData( "MethodAspect", "Event", false )]
        [InlineData( "MethodAspect", "T", false )]
        [InlineData( "MethodBaseAspect", "Class", false )]
        [InlineData( "MethodBaseAspect", "Class.new", true )]
        [InlineData( "MethodBaseAspect", "Class.static", true )]
        [InlineData( "MethodBaseAspect", "Method", true )]
        [InlineData( "MethodBaseAspect", "Method.p", false )]
        [InlineData( "MethodBaseAspect", "Field", false )]
        [InlineData( "MethodBaseAspect", "Property", false )]
        [InlineData( "MethodBaseAspect", "Event", false )]
        [InlineData( "DeclarationAspect", "Class", true )]
        [InlineData( "DeclarationAspect", "Class.new", true )]
        [InlineData( "DeclarationAspect", "Method", true )]
        [InlineData( "DeclarationAspect", "Method.p", true )]
        [InlineData( "DeclarationAspect", "Field", true )]
        [InlineData( "DeclarationAspect", "Property", true )]
        [InlineData( "DeclarationAspect", "Event", true )]
        [InlineData( "ConstructorAspect", "Class.new", true )]
        [InlineData( "ConstructorAspect", "Class.static", true )]
        [InlineData( "ConstructorAspect", "Method", false )]
        [InlineData( "ParameterAspect", "Method.p", true )]
        [InlineData( "GenericParameterAspect", "T", true )]
        public void IsEligible( string aspect, string target, bool isEligible )
        {
            var targetSymbol = this._declarations[target].GetSymbol().AssertNotNull();

            Assert.Equal( isEligible, this._aspects[aspect].IsEligibleFast( targetSymbol ) );
        }

        [Fact]
        public void NamespaceNotEligible()
        {
            var targetSymbol = ((INamedTypeSymbol) this._declarations["Class"].GetSymbol().AssertNotNull()).ContainingNamespace;
            Assert.False( this._aspects["MethodAspect"].IsEligibleFast( targetSymbol ) );
        }

        protected override void Dispose( bool disposing )
        {
            this._buildOptions.Dispose();
            this._domain.Dispose();

            base.Dispose( disposing );
        }
    }
}