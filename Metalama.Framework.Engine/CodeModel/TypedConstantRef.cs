﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Collections.Immutable;
using System.Linq;
using TypedConstant = Metalama.Framework.Code.TypedConstant;

namespace Metalama.Framework.Engine.CodeModel;

/// <summary>
/// A compilation-independent version of <see cref="Code.TypedConstant"/>.
/// </summary>
internal readonly struct TypedConstantRef
{
    public object? RawValue { get; }

    // This property may be null if the type can be assumed from the value.
    public Ref<IType> Type { get; }

    public TypedConstantRef( object? value, Ref<IType> type )
    {
        if ( value is Array )
        {
            throw new ArgumentOutOfRangeException( nameof(value), "An ImmutableArray<TypedConstantRef> was expected." );
        }

        this.RawValue = value;
        this.Type = type;
    }

    public TypedConstant Resolve( CompilationModel compilation )
    {
        var type = this.Type.GetTargetOrNull( compilation );

        if ( this.RawValue == null && type == null )
        {
            return default;
        }

        return this.RawValue switch
        {
            null => TypedConstant.Default( type! ),
            ImmutableArray<TypedConstantRef> array => TypedConstant.Create( array.SelectAsImmutableArray( x => x.Resolve( compilation ) ), type! ),
            Ref<IType> valueAsType => TypedConstant.Create( valueAsType.GetTarget( compilation ), type! ),
            _ => TypedConstant.Create( this.RawValue, type! )
        };
    }
}