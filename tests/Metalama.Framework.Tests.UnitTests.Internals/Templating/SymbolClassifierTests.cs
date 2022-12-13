// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Templating
{
    public sealed class SymbolClassifierTests : UnitTestClass
    {
        private void AssertScope( IDeclaration declaration, TemplatingScope expectedScope, IDiagnosticAdder? diagnosticAdder = null )
        {
            this.AssertScope( declaration.GetCompilationModel().RoslynCompilation, declaration.GetSymbol()!, expectedScope, diagnosticAdder );
        }

        private void AssertScope( INamedType declaration, TemplatingScope expectedScope, IDiagnosticAdder? diagnosticAdder = null )
        {
            this.AssertScope( declaration.GetCompilationModel().RoslynCompilation, declaration.GetSymbol(), expectedScope, diagnosticAdder );
        }

        private void AssertScope( IType type, TemplatingScope expectedScope, IDiagnosticAdder? diagnosticAdder = null )
        {
            this.AssertScope( type.GetCompilationModel().RoslynCompilation, type.GetSymbol(), expectedScope, diagnosticAdder );
        }

        private void AssertScope( Compilation compilation, ISymbol symbol, TemplatingScope expectedScope, IDiagnosticAdder? diagnosticAdder = null )
        {
            using var testContext = this.CreateTestContext();

            var classifier = testContext.ServiceProvider.GetRequiredService<CompilationContextFactory>().GetInstance( compilation ).SymbolClassifier;

            var actualScope = classifier.GetTemplatingScope( symbol );
            Assert.Equal( expectedScope, actualScope );

            if ( diagnosticAdder != null )
            {
                classifier.ReportScopeError( symbol.DeclaringSyntaxReferences.First().GetSyntax(), symbol, diagnosticAdder );
            }
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

            var compilation = TestCompilationFactory.CreateCSharpCompilation( code );
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
        public void UnmarkedMethodInAspect()
        {
            using var testContext = this.CreateTestContext();

            // The main purpose of these tests is to check that there is no infinite recursion.

            var code = @"
using Metalama.Framework.Aspects;
using System.Collections.Generic;

internal class C : TypeAspect
{
    public int M() => 0;
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var m1 = compilation.Types.OfName( "C" ).Single().Methods.OfName( "M" ).Single();
            this.AssertScope( m1, TemplatingScope.RunTimeOrCompileTime );
            this.AssertScope( m1.ReturnType, TemplatingScope.RunTimeOrCompileTime );
        }

        [Fact]
        public void UnmarkedGenericMethodInAspect()
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

        [Fact]
        public void TypeArgumentBug()
        {
            var code = @"
using System.Collections.Immutable;

class C 
{
   void M() 
   {
      var ids = ImmutableArray.CreateBuilder<int>();
   }
}
";

            using var testContext = this.CreateTestContext();
            var compilation = testContext.CreateCompilationModel( code );

            var classifier = testContext.ServiceProvider.GetRequiredService<CompilationContextFactory>()
                .GetInstance( compilation.RoslynCompilation )
                .SymbolClassifier;

            var syntaxTree = compilation.RoslynCompilation.SyntaxTrees.First();
            var semanticModel = compilation.RoslynCompilation.GetSemanticModel( syntaxTree );
            var nodes = syntaxTree.GetRoot().DescendantNodes();

            foreach ( var node in nodes )
            {
                var symbol = semanticModel.GetSymbolInfo( node ).Symbol;

                if ( symbol != null )
                {
                    classifier.GetTemplatingScope( symbol );
                }
            }
        }

        [Fact]
        public void RecordStruct()
        {
            var code = @"record struct S ( int X );
";

            using var testContext = this.CreateTestContext();
            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types.Single();

            this.AssertScope( type, TemplatingScope.RunTimeOnly );
        }

        [Fact]
        public void NestedClassCompileTimeByInheritance()
        {
            var code = @"
using Metalama.Framework.Aspects;

class C : TypeAspect 
{
  class S : IAspectState {}
}
";

            using var testContext = this.CreateTestContext();
            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types.Single();

            this.AssertScope( type, TemplatingScope.RunTimeOrCompileTime );
        }

        [Fact]
        public void ConflictDiagnostic()
        {
            var code = @"
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

class RunTimeClass { } 

[RunTimeOrCompileTime]
class C  {
   void M( RunTimeClass c, IAspectBuilder<IDeclaration> a ) {}
}

";

            DiagnosticBag diagnosticBag = new();

            using var testContext = this.CreateTestContext();
            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types.OfName( "C" ).Single();
            var method = type.Methods.Single();

            this.AssertScope( method, TemplatingScope.Conflict, diagnosticBag );

            var diagnostic = Assert.Single( diagnosticBag );

            Assert.Equal( TemplatingDiagnosticDescriptors.TemplatingScopeConflict.Id, diagnostic.Id );
        }
    }
}