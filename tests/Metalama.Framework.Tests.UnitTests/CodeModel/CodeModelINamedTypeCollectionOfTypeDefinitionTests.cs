﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Comparers;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Testing.UnitTesting;
using System;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel
{
    public class CodeModelINamedTypeCollectionOfTypeDefinitionTests : UnitTestClass
    {
        [Fact]
        public void NonGeneric()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class C
{
}
class D
{
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types.Single( t => t.Name == "C" );

            var types = compilation.Types.OfTypeDefinition( type ).OrderBy( x => x.GetSymbol(), StructuralSymbolComparer.Default ).ToArray();

            Assert.Equal( new[] { type }, types );
        }

        [Fact]
        public void Generic()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class C<T>
{
}

class D
{
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var type = compilation.Types.Single( t => t.Name == "C" );

            var types = compilation.Types.OfTypeDefinition( type ).OrderBy( x => x.GetSymbol(), StructuralSymbolComparer.Default ).ToArray();

            Assert.Equal( new[] { type }, types );
        }

        [Fact]
        public void GenericBase()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class C<T>
{
}

class D : C<int>
{
}

class E
{
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var baseType = compilation.Types.Single( t => t.Name == "C" );
            var type = compilation.Types.Single( t => t.Name == "D" );

            var types = compilation.Types.OfTypeDefinition( baseType ).OrderBy( x => x.GetSymbol(), StructuralSymbolComparer.Default ).ToArray();

            Assert.Equal( new[] { baseType, type }, types );
        }

        [Fact]
        public void GenericBaseBase()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"

class B<T>
{
}

class C : B<int>
{
}

class D : C
{
}

class E
{
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var baseType = compilation.Types.Single( t => t.Name == "C" );
            var type = compilation.Types.Single( t => t.Name == "D" );

            var types = compilation.Types.OfTypeDefinition( baseType ).OrderBy( x => x.GetSymbol(), StructuralSymbolComparer.Default ).ToArray();

            Assert.Equal( new[] { baseType, type }, types );
        }

        [Fact]
        public void GenericGenericBase()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class C<T>
{
}

class D<T,U> : C<U>
{
}

class E
{
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var baseType = compilation.Types.Single( t => t.Name == "C" );
            var type = compilation.Types.Single( t => t.Name == "D" );

            var types = compilation.Types.OfTypeDefinition( baseType ).OrderBy( x => x.GetSymbol(), StructuralSymbolComparer.Default ).ToArray();

            Assert.Equal( new[] { baseType, type }, types );
        }

        [Fact]
        public void Interface()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
interface I
{
}

class D
{
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var interfaceType = compilation.Types.Single( t => t.Name == "I" );
            var type = compilation.Types.Single( t => t.Name == "D" );

            var types = compilation.Types.OfTypeDefinition( interfaceType ).OrderBy( x => x.GetSymbol(), StructuralSymbolComparer.Default ).ToArray();

            Assert.Equal( new[] {  interfaceType }, types );
        }

        [Fact]
        public void ImplementedInterface()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
interface I
{
}

class D : I
{
}

class E
{
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var interfaceType = compilation.Types.Single( t => t.Name == "I" );
            var type = compilation.Types.Single( t => t.Name == "D" );

            var types = compilation.Types.OfTypeDefinition( interfaceType ).OrderBy( x => x.GetSymbol(), StructuralSymbolComparer.Default ).ToArray();

            Assert.Equal( new[] { type, interfaceType }, types );
        }

        [Fact]
        public void GenericInterface()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
interface I<T>
{
}

class D : I<int>
{
}

class E
{
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var interfaceType = compilation.Types.Single( t => t.Name == "I" );
            var type = compilation.Types.Single( t => t.Name == "D" );

            var types = compilation.Types.OfTypeDefinition( interfaceType ).OrderBy( x => x.GetSymbol(), StructuralSymbolComparer.Default ).ToArray();

            Assert.Equal( new[] { type, interfaceType }, types );
        }

        [Fact]
        public void GenericBaseInterface()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
interface I<T>
{
}

interface J : I<int>
{
}

class D : J
{
}

class E
{
}
";

            var compilation = testContext.CreateCompilationModel( code );
            var interfaceBaseType = compilation.Types.Single( t => t.Name == "I" );
            var interfaceType = compilation.Types.Single( t => t.Name == "J" );
            var type = compilation.Types.Single( t => t.Name == "D" );

            var types = compilation.Types.OfTypeDefinition( interfaceBaseType ).OrderBy( x => x.GetSymbol(), StructuralSymbolComparer.Default ).ToArray();

            Assert.Equal( new[] { type, interfaceBaseType, interfaceType }, types );
        }

        [Fact]
        public void UnboundGenericError()
        {
            using var testContext = this.CreateTestContext();

            const string code = @"
class C<T>
{
}
class D
{
}
";

            var compilation = testContext.CreateCompilationModel( code );

            using var userCodeContext = UserCodeExecutionContext.WithContext( testContext.ServiceProvider, compilation );

            var type = compilation.Types.Single( t => t.Name == "C" ).WithTypeArguments( typeof(int) );

            Assert.Throws<ArgumentException>(
                () => _ = compilation.Types.OfTypeDefinition( type ).OrderBy( x => x.GetSymbol(), StructuralSymbolComparer.Default ).ToArray() );
        }
    }
}