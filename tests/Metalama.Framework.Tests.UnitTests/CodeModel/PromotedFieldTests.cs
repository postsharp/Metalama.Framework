// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Testing.UnitTesting;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CodeModel;

public sealed class PromotedFieldTests : UnitTestClass
{
    [Fact]
    public void NonGenericTest()
    {
        using var testContext = this.CreateTestContext();

        // Create original compilation.
        const string code = @"
class C
{    
 int _f;
}";

        var immutableCompilation = testContext.CreateCompilationModel( code );
        var field = immutableCompilation.Types.Single().Fields.Single();

        // Create a PromotedField.
        var promotedField = PromotedField.Create( testContext.ServiceProvider, field, null!, null! );
        Assert.Same( promotedField.Definition, promotedField );
        Assert.Same( promotedField.OverridingProperty, promotedField );
        Assert.Same( promotedField.OriginalField, promotedField );

        _ = promotedField.PrimarySyntaxTree;

        // Verify that all properties work.
        var objectReader = new ObjectReaderFactory( testContext.ServiceProvider ).GetReader( promotedField );

        foreach ( var property in objectReader.Keys )
        {
            _ = objectReader[property];
        }

        // Add the PromotedField to a compilation.
        var compilation = immutableCompilation.CreateMutableClone();
        compilation.AddTransformation( promotedField.ToTransformation() );

        // Assertions on declarations.
        var fieldAfter = field.ForCompilation( compilation );
        Assert.IsType<BuiltField>( fieldAfter );
        Assert.NotNull( fieldAfter.OverridingProperty );
        Assert.NotNull( fieldAfter.OverridingProperty.OriginalField );
        Assert.Same( fieldAfter, fieldAfter.OverridingProperty.OriginalField );
        Assert.Equal( DeclarationKind.Field, fieldAfter.DeclarationKind );
        Assert.Equal( DeclarationKind.Property, fieldAfter.OverridingProperty.DeclarationKind );

        // Declaration on references.
        Assert.Same( fieldAfter, field.ToRef().GetTarget( compilation ) );
        Assert.True( RefEqualityComparer<IField>.Default.Equals( field.ToRef(), promotedField.ToRef().As<IField>() ) );
        Assert.NotEqual<IRef>( promotedField.ToRef(), promotedField.ToRef().As<IField>() );
    }

    [Fact]
    public void GenericTest()
    {
        using var testContext = this.CreateTestContext();

        // Create original compilation.
        const string code = @"
class C<T>
{    
 int _f;
}";

        var immutableCompilation = testContext.CreateCompilationModel( code );

        using ( testContext.WithExecutionContext( immutableCompilation ) )
        {
            var field = immutableCompilation.Types.Single().Fields.Single();

            // Create a PromotedField.
            var promotedField = PromotedField.Create( testContext.ServiceProvider, field, null!, null! );
            Assert.Same( promotedField.Definition, promotedField );
            Assert.Same( promotedField.OverridingProperty, promotedField );
            Assert.Same( promotedField.OriginalField, promotedField );

            // Add the PromotedField to a compilation.
            var compilation = immutableCompilation.CreateMutableClone();
            compilation.AddTransformation( promotedField.ToTransformation() );
            var builtPromotedField = field.ForCompilation( compilation );

            // Get generic instances.
            var genericField = field.ForTypeInstance( field.DeclaringType.WithTypeArguments( typeof(int) ) );
            var genericPromotedField = builtPromotedField.ForTypeInstance( builtPromotedField.DeclaringType.WithTypeArguments( typeof(int) ) );

            // Assertions on declarations.
            var genericFieldAfter = genericField.ForCompilation( compilation );
            Assert.IsType<BuiltField>( genericFieldAfter );
            Assert.NotNull( genericFieldAfter.OverridingProperty );
            Assert.NotNull( genericFieldAfter.OverridingProperty.OriginalField );
            Assert.Same( genericFieldAfter, genericFieldAfter.OverridingProperty.OriginalField );
            Assert.Equal( DeclarationKind.Field, genericFieldAfter.DeclarationKind );
            Assert.Equal( DeclarationKind.Property, genericFieldAfter.OverridingProperty.DeclarationKind );

            // Declaration on references.
            Assert.Same( genericFieldAfter, genericField.ToRef().GetTarget( compilation ) );
            Assert.True( RefEqualityComparer<IField>.Default.Equals( genericField.ToRef(), genericPromotedField.ToRef().As<IField>() ) );
        }
    }

    [Fact]
    public void Introduced()
    {
        using var testContext = this.CreateTestContext();

        // Create original compilation.
        const string code = @"
class C
{
}";

        var immutableCompilation1 = testContext.CreateCompilationModel( code );

        var introducedField = new FieldBuilder( null!, immutableCompilation1.Types.Single(), "_f", ObjectReader.Empty );
        introducedField.Accessibility = Accessibility.Private;
        introducedField.Type = immutableCompilation1.Factory.GetSpecialType( SpecialType.Int32 );

        // Add the PromotedField to a compilation.
        var compilation1 = immutableCompilation1.CreateMutableClone();
        compilation1.AddTransformation( introducedField.ToTransformation() );

        var immutableCompilation2 = compilation1.CreateImmutableClone();

        var field = immutableCompilation2.Types.Single().Fields.Single();

        // Create a PromotedField.
        var promotedField = PromotedField.Create( testContext.ServiceProvider, field, null!, null! );
        Assert.Same( promotedField.Definition, promotedField );
        Assert.Same( promotedField.OverridingProperty, promotedField );
        Assert.Same( promotedField.OriginalField, promotedField );

        _ = promotedField.PrimarySyntaxTree;

        // Verify that all properties work.
        var objectReader = new ObjectReaderFactory( testContext.ServiceProvider ).GetReader( promotedField );

        foreach ( var property in objectReader.Keys )
        {
            _ = objectReader[property];
        }

        // Add the PromotedField to a compilation.
        var compilation2 = immutableCompilation1.CreateMutableClone();
        compilation2.AddTransformation( promotedField.ToTransformation() );

        // Assertions on declarations.
        var fieldAfter = field.ForCompilation( compilation2 );
        Assert.IsType<BuiltField>( fieldAfter );
        Assert.NotNull( fieldAfter.OverridingProperty );
        Assert.NotNull( fieldAfter.OverridingProperty.OriginalField );
        Assert.Same( fieldAfter, fieldAfter.OverridingProperty.OriginalField );
        Assert.Equal( DeclarationKind.Field, fieldAfter.DeclarationKind );
        Assert.Equal( DeclarationKind.Property, fieldAfter.OverridingProperty.DeclarationKind );

        // Declaration on references.
        Assert.Same( fieldAfter, field.ToRef().GetTarget( compilation2 ) );
        Assert.True( RefEqualityComparer<IField>.Default.Equals( field.ToRef(), promotedField.ToRef().As<IField>() ) );
        Assert.NotEqual<IRef>( promotedField.ToRef(), promotedField.ToRef().As<IField>() );
    }
}