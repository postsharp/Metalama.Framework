// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Engine.Utilities;
using Metalama.TestFramework;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CompileTime
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

            var actual = rewriter.Visit( compilation.SyntaxTrees.Single().GetRoot() )!.ToFullString();

            Assert.Equal( expected, actual );
        }

        [Fact]
        public void Attributes()
        {
            var code = @"
using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;

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

            using var testContext = this.CreateTestContext();

            var roslynCompilation = CreateCSharpCompilation( code );
            var compilation = CompilationModel.CreateInitialInstance( new NullProject( testContext.ServiceProvider ), roslynCompilation );

            var compileTimeDomain = new UnloadableCompileTimeDomain();
            var loader = CompileTimeProjectLoader.Create( compileTimeDomain, testContext.ServiceProvider );

            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    compilation.RoslynCompilation,
                    ProjectLicenseInfo.Empty,
                    null,
                    new DiagnosticBag(),
                    false,
                    CancellationToken.None,
                    out _ ) );

            if ( !loader.AttributeDeserializer.TryCreateAttribute( compilation.Attributes.First(), new DiagnosticBag(), out var attribute ) )
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
using Metalama.Framework.Aspects;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            var referencingCode = @"

using Metalama.Framework.Aspects;
[assembly: CompileTime]
class ReferencingClass
{
  ReferencedClass c;
}
";

            var roslynCompilation = CreateCSharpCompilation( referencingCode, referencedCode );

            using var testContext = this.CreateTestContext();
            var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), testContext.ServiceProvider );

            DiagnosticBag diagnosticBag = new();

            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    roslynCompilation,
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    false,
                    CancellationToken.None,
                    out _ ) );
        }

        [Fact]
        public void BinaryMetadataReferences()
        {
            // This tests that we can create compile-time assemblies that have reference compiled assemblies (out of the solution) with compile-time code.

            var indirectlyReferencedCode = @"
using Metalama.Framework.Aspects;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            var directlyReferencedCode = @"
using Metalama.Framework.Aspects;
[assembly: CompileTime]
public class MiddleClass
{
  ReferencedClass c;
}
";

            var referencingCode = @"

using Metalama.Framework.Aspects;
[assembly: CompileTime]
class ReferencingClass
{
  MiddleClass d;
}
";

            List<string> tempFiles = new();

            try
            {
                var testAssemblyLocator = new TestAssemblyLocator();
                using var testContext = this.CreateTestContext( p => p.WithService( testAssemblyLocator ) );

                PortableExecutableReference CompileProject( string code, params MetadataReference[] references )
                {
                    // For this test, we need a different loader every time, because we simulate a series command-line calls,
                    // one for each project.
                    var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), testContext.ServiceProvider );

                    var compilation = CreateCSharpCompilation( code, additionalReferences: references );
                    DiagnosticBag diagnostics = new();

                    Assert.True(
                        loader.TryGetCompileTimeProjectFromCompilation(
                            compilation,
                            ProjectLicenseInfo.Empty,
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
                            new[] { compileTimeProject!.ToResource().Resource } );

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
using Metalama.Framework.Aspects;
[assembly: CompileTime]
public class VersionedClass
{
    public static int Version => $version;
}
".ReplaceOrdinal( "$version", version.ToString( CultureInfo.InvariantCulture ) );

            var classA = @"

using Metalama.Framework.Aspects;
[assembly: CompileTime]
class A
{
  public  static int Version => VersionedClass.Version;
}
";

            var classB = @"

using Metalama.Framework.Aspects;
[assembly: CompileTime]
class B
{
  public static int Version => VersionedClass.Version;
}
";

            var guid = RandomIdGenerator.GenerateId();
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
            using var testContext1 = this.CreateTestContext();

            var loaderV1 = CompileTimeProjectLoader.Create( domain, testContext1.ServiceProvider );
            DiagnosticBag diagnosticBag = new();

            Assert.True(
                loaderV1.TryGetCompileTimeProjectFromCompilation(
                    compilationB1,
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    false,
                    CancellationToken.None,
                    out var project1 ) );

            ExecuteAssertions( project1!, 1 );

            using var testContext2 = this.CreateTestContext();
            var loader2 = CompileTimeProjectLoader.Create( domain, testContext2.ServiceProvider );

            Assert.True(
                loader2.TryGetCompileTimeProjectFromCompilation(
                    compilationB2,
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    false,
                    CancellationToken.None,
                    out var project2 ) );

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

using Metalama.Framework.Fabrics;
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
            using var testContext = this.CreateTestContext();
            var loader = CompileTimeProjectLoader.Create( domain, testContext.ServiceProvider );
            DiagnosticBag diagnosticBag = new();

            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    compilation,
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    false,
                    CancellationToken.None,
                    out _ ) );
        }

        [Fact]
        public void CacheWithSameLoader()
        {
            var code = @"
using Metalama.Framework.Aspects;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            var roslynCompilation = CreateCSharpCompilation( code );

            using var testContext = this.CreateTestContext();
            var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), testContext.ServiceProvider );

            DiagnosticBag diagnosticBag = new();

            // Getting from cache should fail.
            Assert.False(
                loader.TryGetCompileTimeProjectFromCompilation(
                    roslynCompilation,
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    true,
                    CancellationToken.None,
                    out _ ) );

            // Building the project should succeed.
            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    roslynCompilation,
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    false,
                    CancellationToken.None,
                    out var compileTimeProject1 ) );

            // After building, getting from cache should succeed.
            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    roslynCompilation,
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    true,
                    CancellationToken.None,
                    out var compileTimeProject2 ) );

            Assert.Same( compileTimeProject1, compileTimeProject2 );
        }

        [Fact]
        public void CacheWithDifferentIdentityButSameCodeSameLoader()
        {
            var code = @"
using Metalama.Framework.Aspects;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            using var testContext = this.CreateTestContext();
            var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), testContext.ServiceProvider );

            DiagnosticBag diagnosticBag = new();

            // Building the project should succeed.
            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    CreateCSharpCompilation( code ),
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    false,
                    CancellationToken.None,
                    out var compileTimeProject1 ) );

            // After building, getting from cache should succeed.
            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    CreateCSharpCompilation( code ),
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    true,
                    CancellationToken.None,
                    out var compileTimeProject2 ) );

            Assert.Same( compileTimeProject1, compileTimeProject2 );
        }

        [Fact]
        public void CacheWithDifferentIdentityButSameCodeDifferentLoader()
        {
            var code = @"
using Metalama.Framework.Aspects;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            DiagnosticBag diagnosticBag = new();

            using var testContext = this.CreateTestContext();
            var loader1 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), testContext.ServiceProvider );

            // Building the project should succeed.
            Assert.True(
                loader1.TryGetCompileTimeProjectFromCompilation(
                    CreateCSharpCompilation( code ),
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    false,
                    CancellationToken.None,
                    out _ ) );

            // After building, getting from cache should fail because the memory cache is empty and the disk cache checks the assembly name.
            var loader2 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), testContext.ServiceProvider );

            Assert.False(
                loader2.TryGetCompileTimeProjectFromCompilation(
                    CreateCSharpCompilation( code ),
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    true,
                    CancellationToken.None,
                    out _ ) );
        }

        [Fact]
        public void CacheWithDifferentLoader()
        {
            var code = @"
using Metalama.Framework.Aspects;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            var roslynCompilation = CreateCSharpCompilation( code );

            DiagnosticBag diagnosticBag = new();

            // We create a single testContext.ServiceProvider because we need to share the filesystem cache, and there is one per testContext.ServiceProvider
            // in test projects.
            using var testContext = this.CreateTestContext();

            // Getting from cache should fail.

            var loader1 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), testContext.ServiceProvider );

            Assert.False(
                loader1.TryGetCompileTimeProjectFromCompilation(
                    roslynCompilation,
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    true,
                    CancellationToken.None,
                    out _ ) );

            // Building the project should succeed.
            var loader2 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), testContext.ServiceProvider );

            Assert.True(
                loader2.TryGetCompileTimeProjectFromCompilation(
                    roslynCompilation,
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    false,
                    CancellationToken.None,
                    out _ ) );

            // After building, getting from cache should succeed.
            var loader3 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), testContext.ServiceProvider );

            Assert.True(
                loader3.TryGetCompileTimeProjectFromCompilation(
                    roslynCompilation,
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    true,
                    CancellationToken.None,
                    out _ ) );
        }

        [Fact]
        public void CleanCacheAndDeserialize()
        {
            var referencedCode = @"
using Metalama.Framework.Aspects;
[assembly: CompileTime]
public class ReferencedClass
{
}
";

            var referencingCode = @"

using Metalama.Framework.Aspects;
[assembly: CompileTime]
class ReferencingClass
{
  ReferencedClass c;
}
";

            var referencedCompilation = CreateCSharpCompilation( referencedCode );

            var testContext2 = this.CreateTestContext();

            var referencedPath = Path.Combine( testContext2.ProjectOptions.BaseDirectory, "referenced.dll" );

            using ( var testContext = this.CreateTestContext() )
            {
                var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), testContext.ServiceProvider );

                DiagnosticBag diagnosticBag = new();

                Assert.True(
                    loader.TryGetCompileTimeProjectFromCompilation(
                        referencedCompilation,
                        ProjectLicenseInfo.Empty,
                        null,
                        diagnosticBag,
                        false,
                        CancellationToken.None,
                        out var referencedCompileTimeProject ) );

                using var peStream = File.Create( referencedPath );

                Assert.True(
                    referencedCompilation.Emit(
                            peStream,
                            manifestResources: new[] { referencedCompileTimeProject!.ToResource().Resource } )
                        .Success );
            }

            var referencingCompilation = CreateCSharpCompilation(
                referencingCode,
                additionalReferences: new[] { MetadataReference.CreateFromFile( referencedPath ) } );

            using ( testContext2 )
            {
                var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), testContext2.ServiceProvider );

                DiagnosticBag diagnosticBag = new();

                Assert.True(
                    loader.TryGetCompileTimeProjectFromCompilation(
                        referencingCompilation,
                        ProjectLicenseInfo.Empty,
                        null,
                        diagnosticBag,
                        false,
                        CancellationToken.None,
                        out _ ) );
            }
        }

        [Fact]
        public void EmptyProjectWithReference()
        {
            using var testContext = this.CreateTestContext();

            var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), testContext.ServiceProvider );

            var referencedCode = @"
using Metalama.Framework.Aspects;
[assembly: CompileTime]
public class ReferencedClass
{
}";

            var referencingCode = @"/* Intentionally empty. */";

            // Emit the referenced assembly.
            var referencedCompilation = CreateCSharpCompilation( referencedCode );
            var referencedPath = Path.Combine( testContext.ProjectOptions.BaseDirectory, "referenced.dll" );

            DiagnosticBag diagnosticBag = new();

            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    referencedCompilation,
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    false,
                    CancellationToken.None,
                    out var referencedCompileTimeProject ) );

            using ( var peStream = File.Create( referencedPath ) )
            {
                Assert.True(
                    referencedCompilation.Emit(
                            peStream,
                            manifestResources: new[] { referencedCompileTimeProject!.ToResource().Resource } )
                        .Success );
            }

            // Create the referencing compile-time project.
            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    CreateCSharpCompilation( referencingCode, additionalReferences: new[] { MetadataReference.CreateFromFile( referencedPath ) } ),
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    false,
                    CancellationToken.None,
                    out var compileTimeProject ) );

            Assert.NotNull( compileTimeProject );
            Assert.Single( compileTimeProject!.References.Where( r => !r.IsFramework ) );
        }

        [Fact]
        public void RewriteTypeOf()
        {
            var code = @"
using System;
using Metalama.Framework.Aspects;

[CompileTime]
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
using global::System;
using global::Metalama.Framework.Aspects;

[CompileTime]
public class CompileTimeOnlyClass
{
   static global::System.Type Type1 = global::Metalama.Framework.Engine.ReflectionMocks.CompileTimeType.ResolveCompileTimeTypeOf(""1 (D \""RunTimeOnlyClass\"" (N \""\"" 0 (U (S \""test\"" 3) 2) 1) 0 0 (% 0) 0)"",null);
   static global::System.Type Type2 = typeof(global::CompileTimeOnlyClass);
   static string Name1 = ""RunTimeOnlyClass"";
   static string Name2 = ""CompileTimeOnlyClass"";

   void Method() { var t = global::Metalama.Framework.Engine.ReflectionMocks.CompileTimeType.ResolveCompileTimeTypeOf(""1 (D \""RunTimeOnlyClass\"" (N \""\"" 0 (U (S \""test\"" 3) 2) 1) 0 0 (% 0) 0)"",null); }
   string Property => ""RunTimeOnlyClass"";
}
";

            var compilation = CreateCSharpCompilation( code, name: "test" );

            using var testContext = this.CreateTestContext();
            var loader = CompileTimeProjectLoader.Create( new CompileTimeDomain(), testContext.ServiceProvider );

            DiagnosticBag diagnosticBag = new();

            Assert.True(
                loader.TryGetCompileTimeProjectFromCompilation(
                    compilation,
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
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
            var rewriter = new Rewriter();
            using var testContext = this.CreateTestContext( p => p.WithService( rewriter ) );

            var code = @"
using System;
using Metalama.Framework.Aspects;

[CompileTime]
public class Anything
{
}
";

            var roslynCompilation = CreateCSharpCompilation( code );
            var loader1 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), testContext.ServiceProvider );
            DiagnosticBag diagnosticBag = new();

            Assert.True(
                loader1.TryGetCompileTimeProjectFromCompilation(
                    roslynCompilation,
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    false,
                    CancellationToken.None,
                    out _ ) );

            Assert.True( rewriter.IsInvoked );
        }

        [Fact]
        public void NoBuildTimeCodeNoDependency()
        {
            using var testContext = this.CreateTestContext();

            var code = @"
using System;
using Metalama.Framework.Aspects;

public class SomeRunTimeClass
{
}

";

            var roslynCompilation = CreateCSharpCompilation( code );
            var loader1 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), testContext.ServiceProvider );
            DiagnosticBag diagnosticBag = new();

            Assert.True(
                loader1.TryGetCompileTimeProjectFromCompilation(
                    roslynCompilation,
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    false,
                    CancellationToken.None,
                    out var project ) );

            Assert.NotNull( project );
            Assert.Single( project!.References );
            Assert.True( project.References[0].IsFramework );
        }

        [Fact]
        public void FormatCompileTimeCode()
        {
            using var testContext = this.CreateTestContext( new TestProjectOptions( formatCompileTimeCode: true ) );

            var code = @"
using System;
using Metalama.Framework.Aspects;

public class MyAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        return default;
    }
}

";

            var compileTimeCode = GetCompileTimeCode( testContext, code );

            Assert.Contains( "using Microsoft.CodeAnalysis", compileTimeCode, StringComparison.Ordinal );
        }

        private static string GetCompileTimeCode( TestContext testContext, string code, OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary )
        {
            var compileTimeSyntaxTrees = GetCompileTimeCode( testContext, new Dictionary<string, string> { { "main.cs", code } }, outputKind );

            return compileTimeSyntaxTrees
                .Single( x => !x.Key.StartsWith( "__", StringComparison.Ordinal ) )
                .Value;
        }

        private static IReadOnlyDictionary<string, string> GetCompileTimeCode(
            TestContext testContext,
            IReadOnlyDictionary<string, string> code,
            OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary )
        {
            var roslynCompilation = CreateCSharpCompilation( code, outputKind: outputKind );
            var loader1 = CompileTimeProjectLoader.Create( new CompileTimeDomain(), testContext.ServiceProvider );
            DiagnosticBag diagnosticBag = new();

            Assert.True(
                loader1.TryGetCompileTimeProjectFromCompilation(
                    roslynCompilation,
                    ProjectLicenseInfo.Empty,
                    null,
                    diagnosticBag,
                    false,
                    CancellationToken.None,
                    out var project ) );

            Assert.NotNull( project );
            Assert.NotNull( project!.Directory );

            // Just test that the output file has gone through formatting (we don't test that the whole formatting is correct). 
            var files = Directory
                .GetFiles( project.Directory!, "*.cs" )
                .Where( f => !CompileTimeConstants.IsPredefinedSyntaxTree( f ) );

            return files.ToImmutableDictionary( f => Path.GetFileName( f ), File.ReadAllText );
        }

        [Fact]
        public void EmptyNamespacesAreRemovedFromCompileTimeAssembly()
        {
            using var testContext = this.CreateTestContext();

            // The namespace Ns should be removed because it does not contain any build-time code.
            var compileTimeCode = GetCompileTimeCode(
                testContext,
                new Dictionary<string, string>
                {
                    ["BuildTime.cs"] = "class Aspect : Metalama.Framework.Aspects.MethodAspect {}",
                    ["RunTime.cs"] = @"namespace Ns { class C {} } ",
                    ["Both.cs"] =
                        "namespace Ns1 { class Aspect : Metalama.Framework.Aspects.MethodAspect {} } namespace Ns2 { class C {} }"
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
        public void TopLevelStatementsAreRemoved()
        {
            using var testContext = this.CreateTestContext( new TestProjectOptions( formatCompileTimeCode: true ) );

            var code = @"
using System;
using Metalama.Framework.Aspects;

Method();

void Method() { }
int field;

[CompileTime]
class CompileTimeClass { }
";

            var compileTimeCode = GetCompileTimeCode( testContext, code, OutputKind.ConsoleApplication );

            var expected = @"
using System;
using Metalama.Framework.Aspects;

[CompileTime]
class CompileTimeClass { }
";

            Assert.Equal( expected, compileTimeCode );
        }

        [Fact]
        public void FabricClassesAreUnNested()
        {
            using var testContext = this.CreateTestContext( new TestProjectOptions( formatCompileTimeCode: true ) );

            var code = @"
using System;
using Metalama.Framework.Fabrics;

public class SomeClass
{
    class Fabric : TypeFabric { public override void AmendType( ITypeAmender amender ) {} }
}

namespace SomeNamespace
{
    class OtherClass<T>
    {
        class NestedTwice
        {
            class Fabric  : TypeFabric { public override void AmendType( ITypeAmender amender ) {} }
        }
    }
}
";

            var compileTimeCode = GetCompileTimeCode( testContext, code );

            var expected = @"
using System;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Serialization;

[OriginalPath(""main.cs"")]
[OriginalId(""T:SomeClass.Fabric"")]
internal class SomeClass_Fabric : TypeFabric
{
    public override void AmendType(ITypeAmender amender) { }
    public SomeClass_Fabric()
    {
    }
    protected SomeClass_Fabric(IArgumentsReader reader)
    {
    }
    public class Serializer : ReferenceTypeSerializer
    {
        public Serializer()
        {
        }

        public override object CreateInstance(Type type, IArgumentsReader constructorArguments)
        {
            return new global::SomeClass_Fabric(constructorArguments);
        }

        public override void SerializeObject(object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments)
        {
        }

        public override void DeserializeFields(object obj, IArgumentsReader initializationArguments)
        {
        }
    }
}

namespace SomeNamespace
{
    [OriginalPath(""main.cs"")]
    [OriginalId(""T:SomeNamespace.OtherClass`1.NestedTwice.Fabric"")]
    internal class OtherClassX1_NestedTwice_Fabric : TypeFabric
    {
        public override void AmendType(ITypeAmender amender) { }
        public OtherClassX1_NestedTwice_Fabric()
        {
        }
        protected OtherClassX1_NestedTwice_Fabric(IArgumentsReader reader)
        {
        }
        public class Serializer : ReferenceTypeSerializer
        {
            public Serializer()
            {
            }

            public override object CreateInstance(Type type, IArgumentsReader constructorArguments)
            {
                return new global::SomeNamespace.OtherClassX1_NestedTwice_Fabric(constructorArguments);
            }

            public override void SerializeObject(object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments)
            {
            }

            public override void DeserializeFields(object obj, IArgumentsReader initializationArguments)
            {
            }
        }
    }
}
";

            Assert.Equal( expected, compileTimeCode );
        }

        [Fact]
        public void CompileTypeTypesOfAllTKindsAreCopied()
        {
            using var testContext = this.CreateTestContext( new TestProjectOptions( formatCompileTimeCode: true ) );

            var code = @"
using System;
using Metalama.Framework.Aspects;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit {} }

[CompileTime]
public class SomeClass
{
    public void M() {}
}

[CompileTime]
public struct SomeStruct
{
    public void M() {}
}

[CompileTime]
public interface SomeInterface
{
    void M();
}

[CompileTime]
public record SomeRecord( int P );

[CompileTime]
public delegate void SomeDelegate();
";

            var compileTimeCode = GetCompileTimeCode( testContext, code );

            var expected = @"
using System;
using Metalama.Framework.Aspects;

[CompileTime]
public class SomeClass
{
    public void M() { }
}

[CompileTime]
public struct SomeStruct
{
    public void M() { }
}

[CompileTime]
public interface SomeInterface
{
    void M();
}

[CompileTime]
public record SomeRecord(int P);

[CompileTime]
public delegate void SomeDelegate();
";

            Assert.Equal( expected, compileTimeCode );
        }

        [Fact]
        public void SyntaxTreeWithOnlyCompileTimeInterfaceIsCopied()
        {
            using var testContext = this.CreateTestContext( new TestProjectOptions( formatCompileTimeCode: true ) );

            var code = @"
using System;
using Metalama.Framework.Aspects;

[CompileTime]
public interface SomeInterface
{
    void M();
}
";

            var compileTimeCode = GetCompileTimeCode( testContext, code );

            var expected = @"
using System;
using Metalama.Framework.Aspects;

[CompileTime]
public interface SomeInterface
{
    void M();
}
";

            Assert.Equal( expected, compileTimeCode );
        }

        [Fact]
        public async Task CacheWithPreprocessorSymbolsAsync()
        {
            // Create a compilation that depends on a preprocessor symbol.
            using var testContext1 = this.CreateTestContext();

            var code1 = @"
using Metalama.Framework.Aspects;
[assembly: CompileTime]
#if SYMBOL
public class ReferencedClass
{
}
#else
Intentional syntax error.
#endif
";

            var compilation1 = CreateCSharpCompilation( code1, preprocessorSymbols: new[] { "METALAMA", "SYMBOL" } );

            using var domain1 = new UnloadableCompileTimeDomain();
            var pipeline1 = new CompileTimeAspectPipeline( testContext1.ServiceProvider, true, domain1 );

            var pipelineResult1 = await pipeline1.ExecuteAsync(
                NullDiagnosticAdder.Instance,
                compilation1,
                ImmutableArray<ManagedResource>.Empty,
                CancellationToken.None );

            Assert.True( pipelineResult1.IsSuccess );

            var peFilePath = Path.Combine( testContext1.ProjectOptions.BaseDirectory, "reference.dll" );

            // ReSharper disable once UseAwaitUsing
            using ( var peFile = File.Create( peFilePath ) )
            {
                Assert.True(
                    pipelineResult1.Value.ResultingCompilation.Compilation.Emit(
                            peFile,
                            manifestResources: pipelineResult1.Value.AdditionalResources.Select( x => x.Resource ) )
                        .Success );
            }

            // Create compilation that references the compilation above, but
            // we use a different test context so that the cache of the first step is not used.

            using var testContext2 = this.CreateTestContext();
            var compilation2 = CreateCSharpCompilation( "", additionalReferences: new[] { MetadataReference.CreateFromFile( peFilePath ) } );
            using var domain2 = new UnloadableCompileTimeDomain();
            var pipeline2 = new CompileTimeAspectPipeline( testContext2.ServiceProvider, true, domain2 );
            DiagnosticBag diagnosticBag = new();
            var pipelineResult2 = await pipeline2.ExecuteAsync( diagnosticBag, compilation2, ImmutableArray<ManagedResource>.Empty, CancellationToken.None );

            Assert.True( pipelineResult2.IsSuccess );
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

        [Fact]
        public void PreprocessorDirectivesAreRemoved()
        {
            using var testContext = this.CreateTestContext( new TestProjectOptions( formatCompileTimeCode: true ) );

            var code = @"
#region Namespaces
using System;
using Metalama.Framework.Aspects;
using StrippedNamespace;
#endregion

#region Using Attributes
#if SYMBOL
[assembly: MyRunTimeAttribute]
#else
[assembly: MyCompileTimeAttribute]
#endif
#endregion

[CompileTime]
public class MyCompileTimeAttribute : Attribute {}

#region Defining MyRunTimeAttribute
public class MyRunTimeAttribute : Attribute 
{
#region BadRegion
}
#endregion
#endregion

#region StrippedNamespace
namespace StrippedNamespace {
#region InsideNamespace A
#endregion
class C {
#region InsideClass
#endregion
}
#region InsideNamespace B
#endregion
}
#endregion
";

            var compileTimeCode = GetCompileTimeCode( testContext, code );

            var expected = @"
using System;
using Metalama.Framework.Aspects;

[CompileTime]
public class MyCompileTimeAttribute : Attribute { }
";

            Assert.Equal( expected, compileTimeCode );
        }
    }
}