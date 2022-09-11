// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Templating
{
    public class SymbolClassifierTests : TestBase
    {
        private void AssertScope( IDeclaration declaration, TemplatingScope expectedScope )
        {
            this.AssertScope( declaration.GetCompilationModel().RoslynCompilation, declaration.GetSymbol()!, expectedScope );
        }

        private void AssertScope( INamedType declaration, TemplatingScope expectedScope )
        {
            this.AssertScope( declaration.GetCompilationModel().RoslynCompilation, declaration.GetSymbol(), expectedScope );
        }

        private void AssertScope( IType type, TemplatingScope expectedScope )
        {
            this.AssertScope( type.GetCompilationModel().RoslynCompilation, type.GetSymbol(), expectedScope );
        }

        private void AssertScope( Compilation compilation, ISymbol symbol, TemplatingScope expectedScope )
        {
            using var testContext = this.CreateTestContext();

            var classifier = testContext.ServiceProvider.GetRequiredService<SymbolClassificationService>()
                .GetClassifier( compilation );

            var actualScope = classifier.GetTemplatingScope( symbol );
            Assert.Equal( expectedScope, actualScope );
        }

        [Fact]
        public void AspectType()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
using Metalama.Framework.Aspects;
class C : TypeAspect 
{
   void M() {}
  int F;

 [TemplateAttribute]
 void Template() {}
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types.OfName( "C" ).Single();
            this.AssertScope( (IDeclaration) type, TemplatingScope.RunTimeOrCompileTime );
            this.AssertScope( type.Fields.OfName( "F" ).Single(), TemplatingScope.RunTimeOrCompileTime );
            this.AssertScope( type.Methods.OfName( "M" ).Single(), TemplatingScope.RunTimeOrCompileTime );
            this.AssertScope( type.Methods.OfName( "Template" ).Single(), TemplatingScope.CompileTimeOnly );
        }

        [Fact]
        public void DefaultCode()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
using Metalama.Framework.Aspects;

class C 
{
  void M() {}
  int F;
}

class D : System.IDisposable 
{
   public void Dispose(){} 
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types.OfName( "C" ).Single();
            this.AssertScope( type, TemplatingScope.RunTimeOnly );
            this.AssertScope( type.Fields.OfName( "F" ).Single(), TemplatingScope.RunTimeOnly );
            this.AssertScope( type.Methods.OfName( "M" ).Single(), TemplatingScope.RunTimeOnly );

            this.AssertScope( compilation.Types.OfName( "D" ).Single(), TemplatingScope.RunTimeOnly );
        }

        [Fact]
        public void AssemblyAttribute()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
using Metalama.Framework.Aspects;
[assembly: RunTimeOrCompileTime]
class C 
{
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types.OfName( "C" ).Single();
            this.AssertScope( type, TemplatingScope.RunTimeOrCompileTime );
        }

        [Fact]
        public void MarkedAsCompileTimeOnly()
        {
            var code = @"
using Metalama.Framework.Aspects;

[CompileTime]
class C 
{
  void M() {}
  int F;

  class Nested {}
}
";

            var compilation = CreateCSharpCompilation( code );
            var type = (ITypeSymbol) compilation.GetSymbolsWithName( "C" ).Single();
            this.AssertScope( compilation, type, TemplatingScope.CompileTimeOnly );
            this.AssertScope( compilation, type.GetMembers( "F" ).Single(), TemplatingScope.CompileTimeOnlyReturningBoth );
            this.AssertScope( compilation, type.GetMembers( "M" ).Single(), TemplatingScope.CompileTimeOnly );
            this.AssertScope( compilation, type.GetMembers( "Nested" ).Single(), TemplatingScope.CompileTimeOnly );
        }

        [Fact]
        public void MarkedAsCompileTime()
        {
            using var testContext = this.CreateTestContext();

            // We cannot use CompilationModel for this test because CompileTimeOnly are hidden from the model.

            var code = @"
using Metalama.Framework.Aspects;

[RunTimeOrCompileTime]
class C 
{
  void M() {}
  int F;
}
";

            var compilation = testContext.CreateCompilationModel( code );
            this.AssertScope( compilation.Types.OfName( "C" ).Single(), TemplatingScope.RunTimeOrCompileTime );
        }

        [Fact]
        public void NoMetalamaReference()
        {
            using var testContext = this.CreateTestContext();

            var code = "class C {}";
            var compilation = testContext.CreateCompilationModel( code, addMetalamaReferences: false );
            this.AssertScope( (INamedType) compilation.Factory.GetTypeByReflectionType( typeof(int) ), TemplatingScope.RunTimeOrCompileTime );
            this.AssertScope( (INamedType) compilation.Factory.GetTypeByReflectionType( typeof(Console) ), TemplatingScope.RunTimeOnly );
            this.AssertScope( compilation.Types.Single(), TemplatingScope.RunTimeOnly );
        }

        [Fact]
        public void UnmarkedGenericMethod()
        {
            using var testContext = this.CreateTestContext();

            // The main purpose of these tests is to check that there is no infinite recursion.

            var code = @"
using Metalama.Framework.Aspects;
using System.Collections.Generic;

internal class C : TypeAspect
{
    public T M1<T>() => default!;
    public T[] M2<T>() => default!;
    public T M3<T>(List<T> l) => default!;
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var m1 = compilation.Types.OfName( "C" ).Single().Methods.OfName( "M1" ).Single();
            this.AssertScope( m1, TemplatingScope.RunTimeOrCompileTime );
            this.AssertScope( m1.ReturnType, TemplatingScope.RunTimeOrCompileTime );

            var m2 = compilation.Types.OfName( "C" ).Single().Methods.OfName( "M2" ).Single();
            this.AssertScope( m2, TemplatingScope.RunTimeOrCompileTime );
            this.AssertScope( m2.ReturnType, TemplatingScope.RunTimeOrCompileTime );

            var m3 = compilation.Types.OfName( "C" ).Single().Methods.OfName( "M3" ).Single();
            this.AssertScope( m3, TemplatingScope.RunTimeOrCompileTime );
            this.AssertScope( m3.ReturnType, TemplatingScope.RunTimeOrCompileTime );
        }

        [Fact]
        public void GenericTemplate()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
using Metalama.Framework.Aspects;
using System.Collections.Generic;

internal class C : TypeAspect
{
    [Template]
    public T M1<[CompileTime] T>() => default!;

    [Template]
    public T[] M2<[CompileTime] T>() => default!;

    [Template]
    public T M3<[CompileTime] T>(List<T> p1, T[] p2, List<T[]> p3) => default!;
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var m1 = compilation.Types.OfName( "C" ).Single().Methods.OfName( "M1" ).Single();
            this.AssertScope( m1.ReturnType, TemplatingScope.CompileTimeOnlyReturningRuntimeOnly );

            var m2 = compilation.Types.OfName( "C" ).Single().Methods.OfName( "M2" ).Single();
            this.AssertScope( m2.ReturnType, TemplatingScope.RunTimeOnly );

            var m3 = compilation.Types.OfName( "C" ).Single().Methods.OfName( "M3" ).Single();
            this.AssertScope( m3.Parameters[0].Type, TemplatingScope.RunTimeOnly );
            this.AssertScope( m3.Parameters[1].Type, TemplatingScope.RunTimeOnly );
            this.AssertScope( m3.Parameters[2].Type, TemplatingScope.RunTimeOnly );
        }
    }
}