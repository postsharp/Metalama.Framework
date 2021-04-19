// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Pipeline;
using Caravela.TestFramework;
using System.Linq;
using Xunit;
using Attribute = System.Attribute;

namespace Caravela.Framework.Tests.UnitTests.CompileTime
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
            serviceProvider.AddService<IBuildOptions>( new TestBuildOptions() );

            var roslynCompilation = CreateRoslynCompilation( code );
            var compilation = CompilationModel.CreateInitialInstance( roslynCompilation );

            var builder = new CompileTimeAssemblyBuilder( serviceProvider, roslynCompilation );
            var loader = new CompileTimeAssemblyLoader( serviceProvider, roslynCompilation, builder );

            var attribute = Assert.IsAssignableFrom<Attribute>( loader.CreateAttributeInstance( compilation.Attributes.First() ) );
            Assert.Equal( "A(42, A, C`1+N`1[System.Int32[],System.String], C`1+N`1[T1,T2], P=13)", attribute.ToString() );
        }
    }
}