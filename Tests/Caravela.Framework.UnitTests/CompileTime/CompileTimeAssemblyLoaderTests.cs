using System.Linq;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Pipeline;
using Xunit;

namespace Caravela.Framework.UnitTests
{
    public class CompileTimeAssemblyLoaderTests : TestBase
    {
        [Fact]
        public void Attributes()
        {
            var code = @"
using System;
using Caravela.Framework.Project;

[assembly: A(42, new[] { E.A }, new[] { typeof(C<int[]>.N<string>), typeof(C<>.N<>) }, P = 13)]
[assembly: CompileTime]

enum E { A }

class C<T1>
{
    public class N<T2> {}
}

class A : Attribute
{
    private string constructorArguments;

    public int P { get; set; }

    public A(int i, E[] es, Type[] types) => constructorArguments = $""{i}, {es[0]}, {types[0]}, {types[1]}"";

    public override string ToString() => $""A({constructorArguments}, P={P})"";
}";
            ServiceProvider serviceProvider = new();
            serviceProvider.AddService<IBuildOptions>( new Options() );

            var roslynCompilation = CreateRoslynCompilation( code );
            var compilation = new CompilationModel( roslynCompilation );

            var builder = new CompileTimeAssemblyBuilder( serviceProvider, roslynCompilation );
            var loader = new CompileTimeAssemblyLoader( serviceProvider, roslynCompilation, builder );

            var attribute = Assert.IsAssignableFrom<System.Attribute>( loader.CreateAttributeInstance( compilation.Attributes.First() ) );
            Assert.Equal( "A(42, A, C`1+N`1[System.Int32[],System.String], C`1+N`1[T1,T2], P=13)", attribute.ToString() );
        }
    }

    internal class Options : IBuildOptions
    {
        public bool AttachDebugger => throw new System.NotImplementedException();

        public bool MapPdbToTransformedCode => throw new System.NotImplementedException();

        public string? CompileTimeProjectDirectory => null;
    }
}
