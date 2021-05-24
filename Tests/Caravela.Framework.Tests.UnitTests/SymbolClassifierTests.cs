// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Sdk;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests
{
    public class SymbolClassifierTests : TestBase
    {
        private void AssertScope( IDeclaration declaration, SymbolDeclarationScope expectedScope )
        {
            var classifier = this.ServiceProvider.GetService<SymbolClassificationService>()
                .GetClassifier( ((Declaration) declaration).Compilation.RoslynCompilation );

            var actualScope = classifier.GetSymbolDeclarationScope( declaration.GetSymbol()! );
            Assert.Equal( expectedScope, actualScope );
        }

        [Fact]
        public void AspectType()
        {
            var code = @"
using Caravela.Framework.Aspects;
using Caravela.Framework.Advices;
class C : IAspect 
{
  void M() {}
  int F;

 [OverrideMethodTemplateAttribute]
 void Template() {}
}
";

            var compilation = CreateCompilationModel( code );
            var type = compilation.DeclaredTypes.OfName( "C" ).Single();
            this.AssertScope( type, SymbolDeclarationScope.Both );
            this.AssertScope( type.Fields.OfName( "F" ).Single(), SymbolDeclarationScope.Both );
            this.AssertScope( type.Methods.OfName( "M" ).Single(), SymbolDeclarationScope.Both );
            this.AssertScope( type.Methods.OfName( "Template" ).Single(), SymbolDeclarationScope.CompileTimeOnly );
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

            var compilation = CreateCompilationModel( code );
            var type = compilation.DeclaredTypes.OfName( "C" ).Single();
            this.AssertScope( type, SymbolDeclarationScope.RunTimeOnly );
            this.AssertScope( type.Fields.OfName( "F" ).Single(), SymbolDeclarationScope.RunTimeOnly );
            this.AssertScope( type.Methods.OfName( "M" ).Single(), SymbolDeclarationScope.RunTimeOnly );

            this.AssertScope( compilation.DeclaredTypes.OfName( "D" ).Single(), SymbolDeclarationScope.RunTimeOnly );
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

            var compilation = CreateCompilationModel( code );
            var type = compilation.DeclaredTypes.OfName( "C" ).Single();
            this.AssertScope( type, SymbolDeclarationScope.Both );
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

            var compilation = CreateCompilationModel( code );
            var type = compilation.DeclaredTypes.OfName( "C" ).Single();
            this.AssertScope( type, SymbolDeclarationScope.CompileTimeOnly );
            this.AssertScope( type.Fields.OfName( "F" ).Single(), SymbolDeclarationScope.CompileTimeOnly );
            this.AssertScope( type.Methods.OfName( "M" ).Single(), SymbolDeclarationScope.CompileTimeOnly );
        }

        [Fact]
        public void MarkedAsCompileTime()
        {
            var code = @"
using Caravela.Framework.Aspects;

[CompileTime]
class C 
{
  void M() {}
  int F;
}
";

            var compilation = CreateCompilationModel( code );
            this.AssertScope( compilation.DeclaredTypes.OfName( "C" ).Single(), SymbolDeclarationScope.Both );
        }
    }
}