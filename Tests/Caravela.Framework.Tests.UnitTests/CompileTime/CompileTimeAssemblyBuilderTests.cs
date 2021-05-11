﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CompileTime
{
    public class CompileTimeAssemblyBuilderTests : TestBase
    {
        private static ServiceProvider CreateServiceProvider()
        {
            ServiceProvider serviceProvider = new();
            serviceProvider.AddService<IBuildOptions>( new TestBuildOptions() );
            serviceProvider.AddService( ReferenceAssemblyLocator.GetInstance() );

            return serviceProvider;
        }

        [Fact]
        public void RemoveInvalidUsingRewriterTest()
        {
            var compilation = CreateCSharpCompilation(
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

            using var serviceProvider = CreateServiceProvider();

            var roslynCompilation = CreateCSharpCompilation( code );
            var compilation = CompilationModel.CreateInitialInstance( roslynCompilation );

            var compileTimeDomain = new UnloadableCompileTimeDomain();
            var loader = CompileTimeProjectLoader.Create( compileTimeDomain, serviceProvider );
            Assert.True( loader.TryGetCompileTimeProject( compilation.RoslynCompilation, null, new DiagnosticList(), false, out _ ) );

            if ( !loader.AttributeDeserializer.TryCreateAttribute( compilation.Attributes.First(), new DiagnosticList(), out var attribute ) )
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

            var roslynCompilation = CreateCSharpCompilation( referencingCode, referencedCode );

            using var serviceProvider = CreateServiceProvider();
            var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), serviceProvider );

            DiagnosticList diagnosticList = new();
            Assert.True( loader.TryGetCompileTimeProject( roslynCompilation, null, diagnosticList, false, out _ ) );
        }

        [Fact]
        public void BinaryMetadataReferences()
        {
            // This tests that we can create compile-time assemblies that have reference compiled assemblies (out of the solution) with compile-time code.

            var indirectlyReferencedCode = @"
using Caravela.Framework.Project;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            var directlyReferencedCode = @"
using Caravela.Framework.Project;
[assembly: CompileTime]
public class MiddleClass
{
  ReferencedClass c;
}
";

            var referencingCode = @"

using Caravela.Framework.Project;
[assembly: CompileTime]
class ReferencingClass
{
  MiddleClass d;
}
";

            List<string> tempFiles = new();

            PortableExecutableReference indirectlyReferenced;

            try
            {
                using var serviceProvider = CreateServiceProvider();
                var testAssemblyLocator = new TestAssemblyLocator();
                serviceProvider.AddService<IAssemblyLocator>( testAssemblyLocator );

                PortableExecutableReference CompileProject( string code, params MetadataReference[] references )
                {
                    // For this test, we need a different loader every time, because we simulate a series command-line calls,
                    // one for each project.
                    var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), serviceProvider );

                    var compilation = CreateCSharpCompilation( code, additionalReferences: references );
                    DiagnosticList diagnostics = new();

                    Assert.True(
                        loader.TryGetCompileTimeProject(
                            compilation,
                            null,
                            diagnostics,
                            false,
                            out var compileTimeProject ) );

                    var runTimePath = Path.GetTempFileName();
                    tempFiles.Add( runTimePath );

                    // We must create the dll on disk to emulate the path taken by real code.
                    using ( var runTimeStream = File.Create( runTimePath ) )
                    {
                        var emitResult = compilation.Emit(
                            runTimeStream,
                            null,
                            null,
                            null,
                            new[] { compileTimeProject!.ToResource() } );

                        Assert.True( emitResult.Success );
                    }

                    var referenceToSelf = MetadataReference.CreateFromFile( runTimePath );
                    testAssemblyLocator.Files.Add( compilation.Assembly.Identity, referenceToSelf );

                    return referenceToSelf;
                }

                indirectlyReferenced = CompileProject( indirectlyReferencedCode );
                var directlyReferenced = CompileProject( directlyReferencedCode, indirectlyReferenced );
                _ = CompileProject( referencingCode, directlyReferenced );
            }
            finally
            {
                foreach ( var path in tempFiles )
                {
                    if ( File.Exists( path ) )
                    {
                        File.Delete( path );
                    }
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
            var versionedCompilationV1 = CreateCSharpCompilation( GenerateVersionedCode( 1 ), name: "test_Versioned_" + guid );
            var versionedCompilationV2 = CreateCSharpCompilation( GenerateVersionedCode( 2 ), name: "test_Versioned_" + guid );

            var compilationA = CreateCSharpCompilation(
                classA,
                additionalReferences: new[] { versionedCompilationV1.ToMetadataReference() },
                name: "test_A_" + guid );

            var compilationB1 = CreateCSharpCompilation(
                classB,
                additionalReferences: new[] { versionedCompilationV1.ToMetadataReference(), compilationA.ToMetadataReference() },
                name: "test_B_" + guid );

            var compilationB2 = CreateCSharpCompilation(
                classB,
                additionalReferences: new[] { versionedCompilationV2.ToMetadataReference(), compilationA.ToMetadataReference() },
                name: "test_B_" + guid );

            using var domain = new UnloadableCompileTimeDomain();
            using var serviceProvider1 = CreateServiceProvider();

            var loaderV1 = CompileTimeProjectLoader.Create( domain, serviceProvider1 );
            DiagnosticList diagnosticList = new();
            Assert.True( loaderV1.TryGetCompileTimeProject( compilationB1, null, diagnosticList, false, out var project1 ) );
            ExecuteAssertions( project1!, 1 );

            using var serviceProvider2 = CreateServiceProvider();
            var loader2 = CompileTimeProjectLoader.Create( domain, serviceProvider2 );
            Assert.True( loader2.TryGetCompileTimeProject( compilationB2, null, diagnosticList, false, out var project2 ) );

            ExecuteAssertions( project2!, 2 );

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

        [Fact]
        public void CanCreateCompileTimeProjectWithInvalidRunTimeCode()
        {
            // We need to be able to have a compile-time assembly even if there is an error in run-time-only code,
            // otherwise the design-time experience is doomed to fail.

            var code = @"

using Caravela.Framework.Project;
[CompileTime]
class B
{

}

class C 
{
    Intentionally Invalid
}
";

            var domain = new CompileTimeDomain();
            var compilation = CreateCSharpCompilation( code, ignoreErrors: true );
            using var serviceProvider = CreateServiceProvider();
            var loader = CompileTimeProjectLoader.Create( domain, serviceProvider );
            DiagnosticList diagnosticList = new();
            Assert.True( loader.TryGetCompileTimeProject( compilation, null, diagnosticList, false, out _ ) );
        }

        [Fact]
        public void CacheWithSameLoader()
        {
            var code = @"
using Caravela.Framework.Project;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            var roslynCompilation = CreateCSharpCompilation( code );

            var serviceProvider = CreateServiceProvider();
            var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), serviceProvider );

            DiagnosticList diagnosticList = new();

            // Getting from cache should fail.
            Assert.False( loader.TryGetCompileTimeProject( roslynCompilation, null, diagnosticList, true, out _ ) );

            // Building the project should succeed.
            Assert.True( loader.TryGetCompileTimeProject( roslynCompilation, null, diagnosticList, false, out _ ) );

            // After building, getting from cache should succeed.
            Assert.True( loader.TryGetCompileTimeProject( roslynCompilation, null, diagnosticList, true, out _ ) );
        }

        [Fact]
        public void CacheWithDifferentLoader()
        {
            var code = @"
using Caravela.Framework.Project;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            var roslynCompilation = CreateCSharpCompilation( code );

            DiagnosticList diagnosticList = new();

            // We create a single ServiceProvider because we need to share the filesystem cache, and there is one per ServiceProvider
            // in test projects.
            using var serviceProvider = CreateServiceProvider();

            // Getting from cache should fail.

            var loader1 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), serviceProvider );
            Assert.False( loader1.TryGetCompileTimeProject( roslynCompilation, null, diagnosticList, true, out _ ) );

            // Building the project should succeed.
            var loader2 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), serviceProvider );
            Assert.True( loader2.TryGetCompileTimeProject( roslynCompilation, null, diagnosticList, false, out _ ) );

            // After building, getting from cache should succeed.
            var loader3 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), serviceProvider );
            Assert.True( loader3.TryGetCompileTimeProject( roslynCompilation, null, diagnosticList, true, out _ ) );
        }
    }
}