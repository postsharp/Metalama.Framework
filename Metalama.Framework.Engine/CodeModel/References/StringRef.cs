// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel.References;

internal abstract class StringRef<T> : BaseRef<T>, IStringRef, IDurableRef<T>
    where T : class, ICompilationElement
{
    public string Id { get; }

    protected StringRef( string id )
    {
        this.Id = id;
    }

    public override string? Name => null;

    public override IDurableRef<T> ToDurable() => this;

    public override bool IsDurable => true;
    
    public sealed override IRef? ContainingDeclaration => throw new NotSupportedException();


    public override ISymbol GetClosestContainingSymbol( CompilationContext compilationContext )
    {
        // TODO: Handle references to builders.
        return this.GetSymbol( compilationContext );
    }

    public override bool Equals( IRef? other, RefComparison comparison )
    {
        if ( other == null )
        {
            return false;
        }

        if ( other is not IStringRef stringRef )
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

    public sealed override DeclarationKind DeclarationKind => DeclarationKind.None;
}