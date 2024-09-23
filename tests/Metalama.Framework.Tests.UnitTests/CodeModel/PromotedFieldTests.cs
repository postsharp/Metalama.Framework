// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
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

        // Assertions.
        var fieldAfter = field.ForCompilation( compilation );
        Assert.IsType<BuiltField>( fieldAfter );
        Assert.NotNull( fieldAfter.OverridingProperty );

        // Continue assertions.
        Assert.NotNull( fieldAfter.OverridingProperty.OriginalField );
        Assert.Same( fieldAfter, fieldAfter.OverridingProperty.OriginalField );
    }
}