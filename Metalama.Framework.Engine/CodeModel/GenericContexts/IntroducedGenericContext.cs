// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.GenericContexts;

/// <summary>
/// Implements a <see cref="GenericContext"/> that may map parameters to introduced types (instead of just symbols).
/// </summary>
internal class IntroducedGenericContext : GenericContext
{
    private readonly ImmutableArray<IFullRef<IType>> _typeArguments;
    private readonly IntroducedGenericContext? _parentContext;
    private readonly IFullRef<IDeclaration> _definition;

    public IntroducedGenericContext(
        ImmutableArray<IFullRef<IType>> typeArguments,
        IFullRef<IDeclaration> definition,
        IntroducedGenericContext? parentContext )
    {
        this._typeArguments = typeArguments;
        this._parentContext = parentContext;
        this._definition = definition;
    }

    internal override GenericContextKind Kind => GenericContextKind.Introduced;

    internal override ImmutableArray<IFullRef<IType>> TypeArguments => this._typeArguments;

    public override IType Map( ITypeParameter typeParameter )
    {
        if ( typeParameter.ContainingDeclaration.AssertNotNull().GetDefinition().Equals( this._definition.Definition ) )
        {
            return this._typeArguments[typeParameter.Index].GetTarget( typeParameter.Compilation );
        }
        else if ( this._parentContext != null )
        {
            return this._parentContext.Map( typeParameter );
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    public override bool Equals( GenericContext? other )
    {
        if ( other is not IntroducedGenericContext otherIntroducedGenericContext )
        {
            return false;
        }

        if ( !this._definition.Equals( otherIntroducedGenericContext._definition ) )
        {
            return false;
        }

        if ( this._parentContext != null && !this._parentContext.Equals( otherIntroducedGenericContext._parentContext ) )
        {
            return false;
        }

        for ( var i = 0; i < this._typeArguments.Length; i++ )
        {
            if ( !this._typeArguments[i].Equals( otherIntroducedGenericContext._typeArguments[i] ) )
            {
                return false;
            }
        }

        return true;
    }

    protected override int GetHashCodeCore()
    {
        var hashCode = HashCode.Combine( this._parentContext, this._definition.GetHashCode( RefComparison.Default ) );

        foreach ( var arg in this._typeArguments )
        {
            hashCode = HashCode.Combine( hashCode, arg.GetHashCode( RefComparison.Default ) );
        }

        return hashCode;
    }

    public override string ToString()
    {
        string s;

        if ( this._parentContext != null )
        {
            s = this._parentContext + ",";
        }
        else
        {
            s = "";
        }

        s += "[" + string.Join( ", ", this._typeArguments ) + "]";

        return s;
    }
}