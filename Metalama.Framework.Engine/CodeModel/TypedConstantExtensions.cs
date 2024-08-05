// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using RoslynTypedConstant = Microsoft.CodeAnalysis.TypedConstant;
using RoslynTypedConstantKind = Microsoft.CodeAnalysis.TypedConstantKind;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel;

internal static class TypedConstantExtensions
{
    public static TypedConstant ToOurTypedConstant( this RoslynTypedConstant constant, CompilationModel compilation )
    {
        var type = compilation.Factory.GetIType( constant.Type.AssertSymbolNotNull() );

        var value = constant.Kind switch
        {
            RoslynTypedConstantKind.Primitive or RoslynTypedConstantKind.Enum => constant.Value,
            RoslynTypedConstantKind.Type => constant.Value == null ? null : compilation.Factory.GetIType( (ITypeSymbol) constant.Value ),
            RoslynTypedConstantKind.Array => constant.Values.Select( x => ToOurTypedConstant( x, compilation ) ).ToImmutableArray(),
            _ => throw new ArgumentException( nameof(constant) )
        };

        return TypedConstant.CreateUnchecked( value, type );
    }

    public static TypedConstantRef ToOurTypedConstantRef( this RoslynTypedConstant constant, CompilationContext compilationContext )
    {
        var type = Ref.FromSymbol<IType>( constant.Type.AssertSymbolNotNull(), compilationContext );

        return constant.Kind switch
        {
            RoslynTypedConstantKind.Enum or TypedConstantKind.Enum => new TypedConstantRef( constant.Value, type ),
            RoslynTypedConstantKind.Primitive => new TypedConstantRef( constant.Value, default ),
            RoslynTypedConstantKind.Type => new TypedConstantRef(
                Ref.FromSymbol<IType>( (ITypeSymbol) constant.Value.AssertNotNull(), compilationContext ),
                type ),
            TypedConstantKind.Array => new TypedConstantRef(
                constant.Values.Select( x => ToOurTypedConstantRef( x, compilationContext ) ).ToImmutableArray(),
                type ),
            _ => throw new ArgumentException( nameof(constant) )
        };
    }

    public static TypedConstantRef ToRef( this TypedConstant constant )
    {
        if ( !constant.IsInitialized )
        {
            return default;
        }
        else if ( constant.Value == null )
        {
            return new TypedConstantRef( null, constant.Type.ToValueTypedRef() );
        }
        else if ( constant.IsArray )
        {
            return new TypedConstantRef( constant.Values.SelectAsImmutableArray( x => x.ToRef() ), constant.Type.ToValueTypedRef() );
        }
        else
        {
            return new TypedConstantRef( constant.Value, constant.Type.ToValueTypedRef() );
        }
    }
}