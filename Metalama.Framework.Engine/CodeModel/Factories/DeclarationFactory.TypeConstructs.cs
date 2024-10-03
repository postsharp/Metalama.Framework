// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using Microsoft.CodeAnalysis;
using SpecialType = Microsoft.CodeAnalysis.SpecialType;

namespace Metalama.Framework.Engine.CodeModel.Factories;

public partial class DeclarationFactory
{
    IArrayType IDeclarationFactory.ConstructArrayType( IType elementType, int rank )
        => (IArrayType) this.GetIType(
            this.RoslynCompilation.CreateArrayTypeSymbol(
                ((ISdkType) elementType).TypeSymbol.AssertSymbolNullNotImplemented( UnsupportedFeatures.ConstructedIntroducedTypes ),
                rank ) );

    IPointerType IDeclarationFactory.ConstructPointerType( IType pointedType )
        => (IPointerType) this.GetIType(
            this.RoslynCompilation.CreatePointerTypeSymbol(
                ((ISdkType) pointedType).TypeSymbol.AssertSymbolNullNotImplemented( UnsupportedFeatures.ConstructedIntroducedTypes ) ) );

    public IType ConstructNullable( IType type, bool isNullable )
    {
        if ( type.IsNullable == isNullable )
        {
            return type;
        }

        var typeSymbol = ((ISdkType) type).TypeSymbol.AssertSymbolNullNotImplemented( UnsupportedFeatures.ConstructedIntroducedTypes );
        ITypeSymbol newTypeSymbol;

        if ( type.IsReferenceType ?? true )
        {
            newTypeSymbol = typeSymbol
                .WithNullableAnnotation( isNullable ? NullableAnnotation.Annotated : NullableAnnotation.NotAnnotated );
        }
        else
        {
            if ( isNullable )
            {
                newTypeSymbol = this._compilationModel.RoslynCompilation.GetSpecialType( SpecialType.System_Nullable_T )
                    .Construct( typeSymbol );
            }
            else
            {
                return ((INamedType) type).TypeArguments[0];
            }
        }

        return this.GetIType( newTypeSymbol );
    }
}