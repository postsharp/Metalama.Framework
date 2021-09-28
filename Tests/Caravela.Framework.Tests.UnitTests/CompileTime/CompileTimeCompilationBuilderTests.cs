// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CompileTime
{
    public class CompileTimeCompilationBuilderTests : TestBase
    {
        [Fact]
        public void RemoveInvalidNamespaceImport()
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
using Caravela.Framework.Aspects;
using Caravela.Framework.Fabrics;

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

            using var isolatedTest = this.WithIsolatedTest();

            var roslynCompilation = CreateCSharpCompilation( code );
            var compilation = CompilationModel.CreateInitialInstance( NullProject.Instance, roslynCompilation );

            var compileTimeDomain = new UnloadableCompileTimeDomain();
            var loader = CompileTimeProjectLoader.Create( compileTimeDomain, isolatedTest.ServiceProvider );

            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    compilation.RoslynCompilation,
                    null,
                    new DiagnosticList(),
                    false,
                    CancellationToken.None,
                    out _ ) );

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
using Caravela.Framework.Aspects;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            var referencingCode = @"

using Caravela.Framework.Aspects;
[assembly: CompileTime]
class ReferencingClass
{
  ReferencedClass c;
}
";

            var roslynCompilation = CreateCSharpCompilation( referencingCode, referencedCode );

            using var isolatedTest = this.WithIsolatedTest();
            var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), isolatedTest.ServiceProvider );

            DiagnosticList diagnosticList = new();
            Assert.True( loader.TryGetCompileTimeProjectFromCompilation( roslynCompilation, null, diagnosticList, false, CancellationToken.None, out _ ) );
        }

        [Fact]
        public void BinaryMetadataReferences()
        {
            // This tests that we can create compile-time assemblies that have reference compiled assemblies (out of the solution) with compile-time code.

            var indirectlyReferencedCode = @"
using Caravela.Framework.Aspects;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            var directlyReferencedCode = @"
using Caravela.Framework.Aspects;
[assembly: CompileTime]
public class MiddleClass
{
  ReferencedClass c;
}
";

            var referencingCode = @"

using Caravela.Framework.Aspects;
[assembly: CompileTime]
class ReferencingClass
{
  MiddleClass d;
}
";

            List<string> tempFiles = new();

            try
            {
                using var isolatedTest = this.WithIsolatedTest();
                var testAssemblyLocator = new TestAssemblyLocator();
                isolatedTest.ServiceProvider.AddService( testAssemblyLocator );

                PortableExecutableReference CompileProject( string code, params MetadataReference[] references )
                {
                    // For this test, we need a different loader every time, because we simulate a series command-line calls,
                    // one for each project.
                    var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), isolatedTest.ServiceProvider );

                    var compilation = CreateCSharpCompilation( code, additionalReferences: references );
                    DiagnosticList diagnostics = new();

                    Assert.True(
                        loader.TryGetCompileTimeProjectFromCompilation(
                            compilation,
                            null,
                            diagnostics,
                            false,
                            CancellationToken.None,
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

                var indirectlyReferenced = CompileProject( indirectlyReferencedCode );
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
using Caravela.Framework.Aspects;
[assembly: CompileTime]
public class VersionedClass
{
    public static int Version => $version;
}
".Replace( "$version", version.ToString( CultureInfo.InvariantCulture ), StringComparison.Ordinal );

            var classA = @"

using Caravela.Framework.Aspects;
[assembly: CompileTime]
class A
{
  public  static int Version => VersionedClass.Version;
}
";

            var classB = @"

using Caravela.Framework.Aspects;
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
            using var isolatedTest1 = this.WithIsolatedTest();

            var loaderV1 = CompileTimeProjectLoader.Create( domain, isolatedTest1.ServiceProvider );
            DiagnosticList diagnosticList = new();

            Assert.True(
                loaderV1.TryGetCompileTimeProjectFromCompilation( compilationB1, null, diagnosticList, false, CancellationToken.None, out var project1 ) );

            ExecuteAssertions( project1!, 1 );

            using var isolatedTest2 = this.WithIsolatedTest();
            var loader2 = CompileTimeProjectLoader.Create( domain, isolatedTest2.ServiceProvider );

            Assert.True(
                loader2.TryGetCompileTimeProjectFromCompilation( compilationB2, null, diagnosticList, false, CancellationToken.None, out var project2 ) );

            ExecuteAssertions( project2!, 2 );

            void ExecuteAssertions( CompileTimeProject project, int expectedVersion )
            {
                var valueFromA = project.References
                    .Single( p => p.RunTimeIdentity.Name == compilationA.AssemblyName )
                    .GetType( "A" )
                    .GetProperty( "Version" )!
                    .GetValue( null );

                Assert.Equal( expectedVersion, valueFromA );

                var valueFromB = project
                    .GetType( "B" )
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

using Caravela.Framework.Fabrics;
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
            using var isolatedTest = this.WithIsolatedTest();
            var loader = CompileTimeProjectLoader.Create( domain, isolatedTest.ServiceProvider );
            DiagnosticList diagnosticList = new();
            Assert.True( loader.TryGetCompileTimeProjectFromCompilation( compilation, null, diagnosticList, false, CancellationToken.None, out _ ) );
        }

        [Fact]
        public void CacheWithSameLoader()
        {
            var code = @"
using Caravela.Framework.Aspects;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            var roslynCompilation = CreateCSharpCompilation( code );

            using var isolatedTest = this.WithIsolatedTest();
            var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), isolatedTest.ServiceProvider );

            DiagnosticList diagnosticList = new();

            // Getting from cache should fail.
            Assert.False( loader.TryGetCompileTimeProjectFromCompilation( roslynCompilation, null, diagnosticList, true, CancellationToken.None, out _ ) );

            // Building the project should succeed.
            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    roslynCompilation,
                    null,
                    diagnosticList,
                    false,
                    CancellationToken.None,
                    out var compileTimeProject1 ) );

            // After building, getting from cache should succeed.
            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    roslynCompilation,
                    null,
                    diagnosticList,
                    true,
                    CancellationToken.None,
                    out var compileTimeProject2 ) );

            Assert.Same( compileTimeProject1, compileTimeProject2 );
        }

        [Fact]
        public void CacheWithDifferentIdentityButSameCodeSameLoader()
        {
            var code = @"
using Caravela.Framework.Aspects;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            using var isolatedTest = this.WithIsolatedTest();
            var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), isolatedTest.ServiceProvider );

            DiagnosticList diagnosticList = new();

            // Building the project should succeed.
            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    CreateCSharpCompilation( code ),
                    null,
                    diagnosticList,
                    false,
                    CancellationToken.None,
                    out var compileTimeProject1 ) );

            // After building, getting from cache should succeed.
            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    CreateCSharpCompilation( code ),
                    null,
                    diagnosticList,
                    true,
                    CancellationToken.None,
                    out var compileTimeProject2 ) );

            Assert.Same( compileTimeProject1, compileTimeProject2 );
        }

        [Fact]
        public void CacheWithDifferentIdentityButSameCodeDifferentLoader()
        {
            var code = @"
using Caravela.Framework.Aspects;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            DiagnosticList diagnosticList = new();

            using var isolatedTest = this.WithIsolatedTest();
            var loader1 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), isolatedTest.ServiceProvider );

            // Building the project should succeed.
            Assert.True(
                loader1.TryGetCompileTimeProjectFromCompilation(
                    CreateCSharpCompilation( code ),
                    null,
                    diagnosticList,
                    false,
                    CancellationToken.None,
                    out _ ) );

            // After building, getting from cache should fail because the memory cache is empty and the disk cache checks the assembly name.
            var loader2 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), isolatedTest.ServiceProvider );

            Assert.False(
                loader2.TryGetCompileTimeProjectFromCompilation(
                    CreateCSharpCompilation( code ),
                    null,
                    diagnosticList,
                    true,
                    CancellationToken.None,
                    out _ ) );
        }

        [Fact]
        public void CacheWithDifferentLoader()
        {
            var code = @"
using Caravela.Framework.Aspects;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            var roslynCompilation = CreateCSharpCompilation( code );

            DiagnosticList diagnosticList = new();

            // We create a single isolatedTest.ServiceProvider because we need to share the filesystem cache, and there is one per isolatedTest.ServiceProvider
            // in test projects.
            using var isolatedTest = this.WithIsolatedTest();

            // Getting from cache should fail.

            var loader1 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), isolatedTest.ServiceProvider );
            Assert.False( loader1.TryGetCompileTimeProjectFromCompilation( roslynCompilation, null, diagnosticList, true, CancellationToken.None, out _ ) );

            // Building the project should succeed.
            var loader2 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), isolatedTest.ServiceProvider );
            Assert.True( loader2.TryGetCompileTimeProjectFromCompilation( roslynCompilation, null, diagnosticList, false, CancellationToken.None, out _ ) );

            // After building, getting from cache should succeed.
            var loader3 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), isolatedTest.ServiceProvider );
            Assert.True( loader3.TryGetCompileTimeProjectFromCompilation( roslynCompilation, null, diagnosticList, true, CancellationToken.None, out _ ) );
        }

        [Fact]
        public void CleanCacheAndDeserialize()
        {
            var referencedCode = @"
using Caravela.Framework.Aspects;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            var referencingCode = @"

using Caravela.Framework.Aspects;
[assembly: CompileTime]
class ReferencingClass
{
  ReferencedClass c;
}
";

            var referencedCompilation = CreateCSharpCompilation( referencedCode );

            var isolatedTest2 = this.WithIsolatedTest();

            var referencedPath = Path.Combine( isolatedTest2.ProjectOptions.CompileTimeProjectCacheDirectory, "referenced.dll" );

            using ( var isolatedTest = this.WithIsolatedTest() )
            {
                var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), isolatedTest.ServiceProvider );

                DiagnosticList diagnosticList = new();

                Assert.True(
                    loader.TryGetCompileTimeProjectFromCompilation(
                        referencedCompilation,
                        null,
                        diagnosticList,
                        false,
                        CancellationToken.None,
                        out var referencedCompileTimeProject ) );

                using var peStream = File.Create( referencedPath );

                Assert.True(
                    referencedCompilation.Emit(
                            peStream,
                            manifestResources: new[] { referencedCompileTimeProject!.ToResource() } )
                        .Success );
            }

            var referencingCompilation = CreateCSharpCompilation(
                referencingCode,
                additionalReferences: new[] { MetadataReference.CreateFromFile( referencedPath ) } );

            using ( isolatedTest2 )
            {
                var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), isolatedTest2.ServiceProvider );

                DiagnosticList diagnosticList = new();

                Assert.True(
                    loader.TryGetCompileTimeProjectFromCompilation(
                        referencingCompilation,
                        null,
                        diagnosticList,
                        false,
                        CancellationToken.None,
                        out _ ) );
            }
        }

        [Fact]
        public void EmptyProjectWithReference()
        {
            using var isolatedTest = this.WithIsolatedTest();

            var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), isolatedTest.ServiceProvider );

            var referencedCode = @"
using Caravela.Framework.Aspects;
[assembly: CompileTime]
public class ReferencedClass
{
}";

            var referencingCode = @"/* Intentionally empty. */";

            // Emit the referenced assembly.
            var referencedCompilation = CreateCSharpCompilation( referencedCode );
            var referencedPath = Path.Combine( isolatedTest.ProjectOptions.CompileTimeProjectCacheDirectory, "referenced.dll" );

            DiagnosticList diagnosticList = new();

            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    referencedCompilation,
                    null,
                    diagnosticList,
                    false,
                    CancellationToken.None,
                    out var referencedCompileTimeProject ) );

            using ( var peStream = File.Create( referencedPath ) )
            {
                Assert.True(
                    referencedCompilation.Emit(
                            peStream,
                            manifestResources: new[] { referencedCompileTimeProject!.ToResource() } )
                        .Success );
            }

            // Create the referencing compile-time project.
            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    CreateCSharpCompilation( referencingCode, additionalReferences: new[] { MetadataReference.CreateFromFile( referencedPath ) } ),
                    null,
                    diagnosticList,
                    false,
                    CancellationToken.None,
                    out var compileTimeProject ) );

            Assert.NotNull( compileTimeProject );
            Assert.Single( compileTimeProject!.References );
        }

        [Fact]
        public void EmptyProjectWithoutReference()
        {
            using var isolatedTest = this.WithIsolatedTest();
            DiagnosticList diagnosticList = new();

            var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), isolatedTest.ServiceProvider );

            // Create the referencing compile-time project.
            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    CreateCSharpCompilation( @"/* Intentionally empty. */" ),
                    null,
                    diagnosticList,
                    false,
                    CancellationToken.None,
                    out var compileTimeProject ) );

            Assert.Null( compileTimeProject );
        }

        [Fact]
        public void RewriteTypeOf()
        {
            var code = @"
using System;
using Caravela.Framework.Aspects;

[CompileTimeOnly]
public class CompileTimeOnlyClass
{
   static Type Type1 = typeof(RunTimeOnlyClass);
   static Type Type2 = typeof(CompileTimeOnlyClass);
   static string Name1 = nameof(RunTimeOnlyClass);
   static string Name2 = nameof(CompileTimeOnlyClass);

   void Method() { var t = typeof(RunTimeOnlyClass); }
   string Property => nameof(RunTimeOnlyClass);
}

public class RunTimeOnlyClass
{
   static Type Type1 = typeof(RunTimeOnlyClass);
   static Type Type3 = typeof(CompileTimeOnlyClass);

}
";

            var expected = @"
using System;
using Caravela.Framework.Aspects;

[CompileTimeOnly]
public class CompileTimeOnlyClass
{
   static Type Type1 = global::Caravela.Framework.Impl.ReflectionMocks.CompileTimeType.CreateFromDocumentationId(""T:RunTimeOnlyClass"",""RunTimeOnlyClass"");
   static Type Type2 = typeof(CompileTimeOnlyClass);
   static string Name1 = ""RunTimeOnlyClass"";
   static string Name2 = ""CompileTimeOnlyClass"";

   void Method() { var t = global::Caravela.Framework.Impl.ReflectionMocks.CompileTimeType.CreateFromDocumentationId(""T:RunTimeOnlyClass"",""RunTimeOnlyClass""); }
   string Property => ""RunTimeOnlyClass"";
}
";

            var compilation = CreateCSharpCompilation( code );

            var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), this.ServiceProvider );

            DiagnosticList diagnosticList = new();

            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    compilation,
                    null,
                    diagnosticList,
                    false,
                    CancellationToken.None,
                    out var compileTimeProject ) );

            var transformed = File.ReadAllText( Path.Combine( compileTimeProject!.Directory!, compileTimeProject.CodeFiles[0].TransformedPath ) );

            Assert.Equal( expected, transformed );

            // We are not testing the rewriting of typeof in a template because this is done by the template compiler and covered by template tests.
        }

        [Fact]
        public void CompileTimeAssemblyBinaryRewriter()
        {
            using var isolatedTest = this.WithIsolatedTest();
            var rewriter = new Rewriter();
            isolatedTest.ServiceProvider.AddService( rewriter );

            var code = @"
using System;
using Caravela.Framework.Aspects;

[CompileTimeOnly]
public class Anything
{
}
";

            var roslynCompilation = CreateCSharpCompilation( code );
            var loader1 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), isolatedTest.ServiceProvider );
            DiagnosticList diagnosticList = new();
            Assert.True( loader1.TryGetCompileTimeProjectFromCompilation( roslynCompilation, null, diagnosticList, false, CancellationToken.None, out _ ) );

            Assert.True( rewriter.IsInvoked );
        }

        [Fact]
        public void NoBuildTimeCodeNoDependency()
        {
            using var isolatedTest = this.WithIsolatedTest();

            var code = @"
using System;
using Caravela.Framework.Aspects;

public class SomeRunTimeClass
{
}

";

            var roslynCompilation = CreateCSharpCompilation( code );
            var loader1 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), isolatedTest.ServiceProvider );
            DiagnosticList diagnosticList = new();

            Assert.True(
                loader1.TryGetCompileTimeProjectFromCompilation( roslynCompilation, null, diagnosticList, false, CancellationToken.None, out var project ) );

            Assert.Null( project );
        }

        [Fact]
        public void FormatCompileTimeCode()
        {
            using var isolatedTest = this.WithIsolatedTest();
            isolatedTest.ProjectOptions.FormatCompileTimeCode = true;

            var code = @"
using System;
using Caravela.Framework.Aspects;

public class MyAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        return default;
    }
}

";

            var compileTimeCode = GetCompileTimeCode( isolatedTest, code );

            Assert.Contains( "using Microsoft.CodeAnalysis", compileTimeCode, StringComparison.Ordinal );
        }

        private static string GetCompileTimeCode( IsolatedTest test, string code )
            => GetCompileTimeCode( test, new Dictionary<string, string> { { "main.cs", code } } ).Values.Single();

        private static IReadOnlyDictionary<string, string> GetCompileTimeCode( IsolatedTest test, IReadOnlyDictionary<string, string> code )
        {
            var roslynCompilation = CreateCSharpCompilation( code );
            var loader1 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), test.ServiceProvider );
            DiagnosticList diagnosticList = new();

            Assert.True(
                loader1.TryGetCompileTimeProjectFromCompilation( roslynCompilation, null, diagnosticList, false, CancellationToken.None, out var project ) );

            Assert.NotNull( project );
            Assert.NotNull( project!.Directory );

            // Just test that the output file has gone through formatting (we don't test that the whole formatting is correct). 
            var files = Directory
                .GetFiles( project.Directory!, "*.cs" )
                .Where( f => !f.EndsWith( CompileTimeConstants.PredefinedTypesFileName, StringComparison.OrdinalIgnoreCase ) );

            return files.ToImmutableDictionary( f => Path.GetFileName( f ), File.ReadAllText );
        }

        [Fact]
        public void EmptyNamespacesAreRemovedFromCompileTimeAssembly()
        {
            using var isolatedTest = this.WithIsolatedTest();

            // The namespace Ns should be removed because it does not contain any build-time code.
            var compileTimeCode = GetCompileTimeCode(
                isolatedTest,
                new Dictionary<string, string>
                {
                    ["BuildTime.cs"] = "class Aspect : Caravela.Framework.Aspects.IAspect<Caravela.Framework.Code.IMethod> {}",
                    ["RunTime.cs"] = @"namespace Ns { class C {} } ",
                    ["Both.cs"] =
                        "namespace Ns1 { class Aspect : Caravela.Framework.Aspects.IAspect<Caravela.Framework.Code.IMethod> {} } namespace Ns2 { class C {} }"
                } );

            // Test that run-time-only trees are removed from the build-time compilation.
            Assert.DoesNotContain( compileTimeCode.Keys, k => k.StartsWith( "RunTime_", StringComparison.OrdinalIgnoreCase ) );

            // Test that run-time-only namespaces are removed from the build-time compilation.
            Assert.DoesNotContain(
                "namespace Ns2",
                compileTimeCode.Single( p => p.Key.StartsWith( "Both_", StringComparison.OrdinalIgnoreCase ) ).Value,
                StringComparison.Ordinal );
        }

        [Fact]
        public void FabricClassesAreUnNested()
        {
            using var isolatedTest = this.WithIsolatedTest();
            isolatedTest.ProjectOptions.FormatCompileTimeCode = true;

            var code = @"
using System;
using Caravela.Framework.Fabrics;

public class SomeClass
{
    class Fabric : IFabric {}
}

namespace SomeNamespace
{
    class OtherClass<T>
    {
        class NestedTwice
        {
            class Fabric : IFabric {}
        }
    }
}
";

            var compileTimeCode = GetCompileTimeCode( isolatedTest, code );

            var expected = @"
using System;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.CompileTime;

[OriginalPath(""main.cs"")]
[OriginalId(""T:SomeClass.Fabric"")]
class SomeClass_Fabric : IFabric
{ }

namespace SomeNamespace
{
    [OriginalPath(""main.cs"")]
    [OriginalId(""T:SomeNamespace.OtherClass`1.NestedTwice.Fabric"")]
    class OtherClassX1_NestedTwice_Fabric : IFabric
    { }
}
";

            Assert.Equal( expected, compileTimeCode );
        }

        private class Rewriter : ICompileTimeAssemblyBinaryRewriter
        {
            public bool IsInvoked { get; private set; }

            public void Rewrite( Stream input, Stream output, string directory )
            {
                input.CopyTo( output );
                this.IsInvoked = true;
            }
        }
    }
}