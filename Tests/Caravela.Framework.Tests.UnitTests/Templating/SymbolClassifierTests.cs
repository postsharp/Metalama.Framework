// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Templating
{
    public class SymbolClassifierTests : TestBase
    {
        private void AssertScope( IDeclaration declaration, TemplatingScope expectedScope )
        {
            this.AssertScope( ((Declaration) declaration).Compilation.RoslynCompilation, declaration.GetSymbol()!, expectedScope );
        }

        private void AssertScope( Compilation compilation, ISymbol symbol, TemplatingScope expectedScope )
        {
            var classifier = this.ServiceProvider.GetService<SymbolClassificationService>()
                .GetClassifier( compilation );

            var actualScope = classifier.GetTemplatingScope( symbol );
            Assert.Equal( expectedScope, actualScope );
        }

        [Fact]
        public void AspectType()
        {
            var code = @"
using Caravela.Framework.Aspects;
class C : IAspect 
{
   void M() {}
  int F;

 [TemplateAttribute]
 void Template() {}
}
";

            var compilation = this.CreateCompilationModel( code );
            var type = compilation.Types.OfName( "C" ).Single();
            this.AssertScope( type, TemplatingScope.Both );
            this.AssertScope( type.Fields.OfName( "F" ).Single(), TemplatingScope.Both );
            this.AssertScope( type.Methods.OfName( "M" ).Single(), TemplatingScope.Both );
            this.AssertScope( type.Methods.OfName( "Template" ).Single(), TemplatingScope.CompileTimeOnly );
        }

        [Fact]
        public void DefaultCode()
        {
            var code = @"
using Caravela.Framework.Aspects;

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

            var compilation = this.CreateCompilationModel( code );
            var type = compilation.Types.OfName( "C" ).Single();
            this.AssertScope( type, TemplatingScope.RunTimeOnly );
            this.AssertScope( type.Fields.OfName( "F" ).Single(), TemplatingScope.RunTimeOnly );
            this.AssertScope( type.Methods.OfName( "M" ).Single(), TemplatingScope.RunTimeOnly );

            this.AssertScope( compilation.Types.OfName( "D" ).Single(), TemplatingScope.RunTimeOnly );
        }

        [Fact]
        public void AssemblyAttribute()
        {
            var code = @"
using Caravela.Framework.Aspects;
[assembly: CompileTime]
class C 
{
}
";

            var compilation = this.CreateCompilationModel( code );
            var type = compilation.Types.OfName( "C" ).Single();
            this.AssertScope( type, TemplatingScope.Both );
        }

        [Fact]
        public void MarkedAsCompileTimeOnly()
        {
            var code = @"
using Caravela.Framework.Aspects;

[CompileTimeOnly]
class C 
{
  void M() {}
  int F;
}
";

            var compilation = CreateCSharpCompilation( code );
            var type = (ITypeSymbol) compilation.GetSymbolsWithName( "C" ).Single();
            this.AssertScope( compilation, type, TemplatingScope.CompileTimeOnly );
            this.AssertScope( compilation, type.GetMembers( "F" ).Single(), TemplatingScope.CompileTimeOnlyReturningBoth );
            this.AssertScope( compilation, type.GetMembers( "M" ).Single(), TemplatingScope.CompileTimeOnly );
        }

        [Fact]
        public void MarkedAsCompileTime()
        {
            // We cannot use CompilationModel for this test because CompileTimeOnly are hidden from the model.

            var code = @"
using Caravela.Framework.Aspects;

[CompileTime]
class C 
{
  void M() {}
  int F;
}
";

            var compilation = this.CreateCompilationModel( code );
            this.AssertScope( compilation.Types.OfName( "C" ).Single(), TemplatingScope.Both );
        }

        [Fact]
        public void NoCaravelaReference()
        {
            var code = "class C {}";
            var compilation = this.CreateCompilationModel( code, addCaravelaReferences: false );
            this.AssertScope( (INamedType) compilation.Factory.GetTypeByReflectionType( typeof(int) ), TemplatingScope.Both );
            this.AssertScope( (INamedType) compilation.Factory.GetTypeByReflectionType( typeof(Console) ), TemplatingScope.RunTimeOnly );
            this.AssertScope( compilation.Types.Single(), TemplatingScope.RunTimeOnly );
        }
    }
}