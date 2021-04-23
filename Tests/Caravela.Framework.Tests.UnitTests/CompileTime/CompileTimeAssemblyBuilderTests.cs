// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CompileTime
{
    public class CompileTimeAssemblyBuilderTests : TestBase
    {
        private static IServiceProvider GetServiceProvider()
        {
            ServiceProvider serviceProvider = new();
            serviceProvider.AddService<IBuildOptions>( new TestBuildOptions() );
            serviceProvider.AddService( new ReferenceAssemblyLocator() );

            return serviceProvider;
        }

        [Fact]
        public void RemoveInvalidUsingRewriterTest()
        {
            var compilation = CreateRoslynCompilation(
                @"
using System;
using Nonsense;
using Foo;

namespace Foo
{
    class C {}
}
",
                ignoreErrors: true );

            var expected = @"
using System;
using Foo;

namespace Foo
{
    class C {}
}
";

            var rewriter = new CompileTimeCompilationBuilder.RemoveInvalidUsingRewriter( compilation );

            var actual = rewriter.Visit( compilation.SyntaxTrees.Single().GetRoot() ).ToFullString();

            Assert.Equal( expected, actual );
        }

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

            var serviceProvider = GetServiceProvider();

            var roslynCompilation = CreateRoslynCompilation( code );
            var compilation = CompilationModel.CreateInitialInstance( roslynCompilation );

            var loader = CompileTimeAssemblyLoader.Create( new CompileTimeDomain(), serviceProvider, roslynCompilation );

            if ( !loader.TryCreateAttributeInstance( compilation.Attributes.First(), new DiagnosticList(), out var attribute ) )
            {
                throw new AssertionFailedException();
            }

            Assert.Equal( "A(42, A, C`1+N`1[System.Int32[],System.String], C`1+N`1[T1,T2], P=13)", attribute.ToString() );
        }

        [Fact]
        public void CompilationMetadataReference()
        {
            // This tests that we can create compile-time assemblies that have reference projects in the same solution with compile-time code.

            var referencedCode = @"
using Caravela.Framework.Project;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            var referencingCode = @"

using Caravela.Framework.Project;
[assembly: CompileTime]
class ReferencingClass
{
  ReferencedClass c;
}
";

            var roslynCompilation = CreateRoslynCompilation( referencingCode, referencedCode );

            var serviceProvider = GetServiceProvider();
            var loader = CompileTimeAssemblyLoader.Create( new CompileTimeDomain(), serviceProvider, roslynCompilation );

            loader.GetCompileTimeProject( roslynCompilation.Assembly );
        }

        [Fact]
        public void BinaryMetadataReferences()
        {
            // This tests that we can create compile-time assemblies that have reference compiled assemblies (out of the solution) with compile-time code.

            var referencedCode = @"
using Caravela.Framework.Project;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            var referencingCode = @"

using Caravela.Framework.Project;
[assembly: CompileTime]
class ReferencingClass
{
  ReferencedClass c;
}
";

            var referencedCompilation = CreateRoslynCompilation( referencedCode );
            DiagnosticList diagnostics = new();
            var serviceProvider = GetServiceProvider();
            var builder = new CompileTimeCompilationBuilder( serviceProvider, new CompileTimeDomain() );

            Assert.True(
                builder.TryCreateCompileTimeProject( referencedCompilation, ArraySegment<CompileTimeProject>.Empty, diagnostics, out var compileTimeProject ) );

            var referencedRunTimePath = Path.GetTempFileName();

            try
            {
                // We must create the dll on disk to emulate the path taken by real code.

                using ( var referenceRunTimeStream = File.Create( referencedRunTimePath ) )
                {
                    var emitResult = referencedCompilation.Emit(
                        referenceRunTimeStream,
                        null,
                        null,
                        null,
                        new[] { new ResourceDescription( CompileTimeCompilationBuilder.ResourceName, () => compileTimeProject!.Serialize(), true ) } );

                    Assert.True( emitResult.Success );
                }

                var reference = MetadataReference.CreateFromFile( referencedRunTimePath );

                var referencingCompilation = CreateRoslynCompilation( referencingCode, additionalReferences: new[] { reference } );

                var loader = CompileTimeAssemblyLoader.Create( new CompileTimeDomain(), serviceProvider, referencingCompilation );
                _ = loader.GetCompileTimeProject( referencingCompilation.Assembly );
            }
            finally
            {
                if ( File.Exists( referencedRunTimePath ) )
                {
                    File.Delete( referencedRunTimePath );
                }
            }
        }

        [Fact]
        public void UpdatedReference()
        {
            // This test verifies that one can create a project A v1.0 that references B v1.0, but it still works when B is updated to v1.1
            // and A is not recompiled.

            string GenerateVersionedCode( int version )
                => @"
using Caravela.Framework.Project;
[assembly: CompileTime]
public class VersionedClass
{
    public static int Version => $version;
}
".Replace( "$version", version.ToString() );

            var classA = @"

using Caravela.Framework.Project;
[assembly: CompileTime]
class A
{
  public  static int Version => VersionedClass.Version;
}
";

            var classB = @"

using Caravela.Framework.Project;
[assembly: CompileTime]
class B
{
  public static int Version => VersionedClass.Version;
}
";

            var guid = Guid.NewGuid();
            var versionedCompilationV1 = CreateRoslynCompilation( GenerateVersionedCode( 1 ), name: "test_Versioned_" + guid );
            var versionedCompilationV2 = CreateRoslynCompilation( GenerateVersionedCode( 2 ), name: "test_Versioned_" + guid );

            var compilationA = CreateRoslynCompilation(
                classA,
                additionalReferences: new[] { versionedCompilationV1.ToMetadataReference() },
                name: "test_A_" + guid );

            var compilationB1 = CreateRoslynCompilation(
                classB,
                additionalReferences: new[] { versionedCompilationV1.ToMetadataReference(), compilationA.ToMetadataReference() },
                name: "test_B_" + guid );

            var compilationB2 = CreateRoslynCompilation(
                classB,
                additionalReferences: new[] { versionedCompilationV2.ToMetadataReference(), compilationA.ToMetadataReference() },
                name: "test_B_" + guid );

            var domain = new CompileTimeDomain();
            CompileTimeAssemblyLoader loaderV1 = CompileTimeAssemblyLoader.Create( domain, GetServiceProvider(), compilationB1 );
            var project1 = loaderV1.GetCompileTimeProject( compilationB1.Assembly )!;
            ExecuteAssertions( project1, 1 );

            CompileTimeAssemblyLoader loader2 = CompileTimeAssemblyLoader.Create( domain, GetServiceProvider(), compilationB2 );
            var project2 = loader2.GetCompileTimeProject( compilationB2.Assembly )!;

            ExecuteAssertions( project2, 2 );

            void ExecuteAssertions( CompileTimeProject project, int expectedVersion )
            {
                var valueFromA = project.References
                    .Single( p => p.RunTimeIdentity.Name == compilationA.AssemblyName )
                    .GetType( "A" )!
                    .GetProperty( "Version" )!
                    .GetValue( null );

                Assert.Equal( expectedVersion, valueFromA );

                var valueFromB = project
                    .GetType( "B" )!
                    .GetProperty( "Version" )!
                    .GetValue( null );

                Assert.Equal( expectedVersion, valueFromB );
            }
        }
    }
}