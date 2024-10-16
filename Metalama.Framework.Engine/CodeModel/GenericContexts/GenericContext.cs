// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using OurSpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.CodeModel.GenericContexts;

internal abstract partial class GenericContext : IEquatable<GenericContext?>, IGenericContext
{
    public static GenericContext Empty { get; } = new NullGenericContext();

    public bool IsEmptyOrIdentity => this.Kind == GenericContextKind.Null;

    public abstract GenericContextKind Kind { get; }

    [Memo]
    private TypeMapper TypeMapperInstance => new( this );

    public abstract IType Map( ITypeParameter typeParameter );

    [return: NotNullIfNotNull( nameof(type) )]
    public IType? Map( IType? type )
    {
        if ( this.IsEmptyOrIdentity )
        {
            return type;
        }

        return type switch
        {
            null => null,
            ITypeParameter typeParameter => this.Map( typeParameter ),
            _ when type.SpecialType != OurSpecialType.None || !TypeVisitor.Instance.Visit( type ) => type, // Fast path
            _ => this.TypeMapperInstance.Visit( type )
        };
    }

    public abstract bool Equals( GenericContext? other );

    protected abstract int GetHashCodeCore();

    public override int GetHashCode() => this.GetHashCodeCore();

    public sealed override bool Equals( object? obj ) => obj is GenericContext genericMap && this.Equals( genericMap );
}