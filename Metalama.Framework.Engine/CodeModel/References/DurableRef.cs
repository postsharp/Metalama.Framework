﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using System;

namespace Metalama.Framework.Engine.CodeModel.References;

internal abstract class DurableRef<T> : BaseRef<T>, IDurableRef<T>
    where T : class, ICompilationElement
{
    public string Id { get; }

    public abstract IFullRef ToFullRef( CompilationContext compilationContext );

    protected DurableRef( string id )
    {
        this.Id = id;
    }

    public override IDurableRef<T> ToDurable() => this;

    public override bool IsDurable => true;

    public override bool Equals( IRef? other, RefComparison comparison )
    {
        if ( other == null )
        {
            return false;
        }

        if ( other is not IDurableRef stringRef )
        {
            if ( comparison is RefComparison.Structural or RefComparison.StructuralIncludeNullability )
            {
                return this.Equals( other.ToDurable(), comparison );
            }
            else
            {
                return false;
            }
        }

        // String comparisons are always portable and null-sensitive, so we ignore all flags.

        return stringRef.Id == this.Id;
    }

    public override int GetHashCode( RefComparison comparison )
    {
#if NET5_0_OR_GREATER
        return this.Id.GetHashCode( StringComparison.Ordinal );
#else
        return this.Id.GetHashCode();
#endif
    }

    public override string ToString() => this.Id;
}