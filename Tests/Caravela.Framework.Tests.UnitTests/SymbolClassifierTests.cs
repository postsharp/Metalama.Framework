// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Sdk;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests
{
    public class SymbolClassifierTests : TestBase
    {
        private static void AssertScope( ICodeElement codeElement, SymbolDeclarationScope expectedScope )
        {
            var classifier = SymbolClassifier.GetInstance( ((CodeElement) codeElement).Compilation.RoslynCompilation );
            var actualScope = classifier.GetSymbolDeclarationScope( codeElement.GetSymbol()! );
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

            var compilation = CreateCompilation( code );
            var type = compilation.DeclaredTypes.OfName( "C" ).Single();
            AssertScope( type, SymbolDeclarationScope.Default );
            AssertScope( type.Fields.OfName( "F" ).Single(), SymbolDeclarationScope.Default );
            AssertScope( type.Methods.OfName( "M" ).Single(), SymbolDeclarationScope.Default );
            AssertScope( type.Methods.OfName( "Template" ).Single(), SymbolDeclarationScope.CompileTimeOnly );
        }

        [Fact]
        public void DefaultCode()
        {
            var code = @"
using Caravela.Framework.Project;

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

            var compilation = CreateCompilation( code );
            var type = compilation.DeclaredTypes.OfName( "C" ).Single();
            AssertScope( type, SymbolDeclarationScope.RunTimeOnly );
            AssertScope( type.Fields.OfName( "F" ).Single(), SymbolDeclarationScope.RunTimeOnly );
            AssertScope( type.Methods.OfName( "M" ).Single(), SymbolDeclarationScope.RunTimeOnly );

            AssertScope( compilation.DeclaredTypes.OfName( "D" ).Single(), SymbolDeclarationScope.RunTimeOnly );
        }

        [Fact]
        public void AssemblyAttribute()
        {
            var code = @"
using Caravela.Framework.Project;
[assembly: CompileTime]
class C 
{
}
";

            var compilation = CreateCompilation( code );
            var type = compilation.DeclaredTypes.OfName( "C" ).Single();
            AssertScope( type, SymbolDeclarationScope.Default );
        }

        [Fact]
        public void MarkedAsCompileTimeOnly()
        {
            var code = @"
using Caravela.Framework.Project;

[CompileTimeOnly]
class C 
{
  void M() {}
  int F;
}
";

            var compilation = CreateCompilation( code );
            var type = compilation.DeclaredTypes.OfName( "C" ).Single();
            AssertScope( type, SymbolDeclarationScope.CompileTimeOnly );
            AssertScope( type.Fields.OfName( "F" ).Single(), SymbolDeclarationScope.CompileTimeOnly );
            AssertScope( type.Methods.OfName( "M" ).Single(), SymbolDeclarationScope.CompileTimeOnly );
        }

        [Fact]
        public void MarkedAsCompileTime()
        {
            var code = @"
using Caravela.Framework.Project;

[CompileTime]
class C 
{
  void M() {}
  int F;
}
";

            var compilation = CreateCompilation( code );
            AssertScope( compilation.DeclaredTypes.OfName( "C" ).Single(), SymbolDeclarationScope.Default );
        }
    }
}