// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;

namespace Metalama.Framework.Engine.CodeModel;

/// <summary>
/// Visitor for <see cref="IType"/>.
/// </summary>
internal abstract class TypeVisitor<T>
{
    public virtual T Visit( IType type )
        => type switch
        {
            IArrayType arrayType => this.VisitArrayType( arrayType ),
            IDynamicType dynamicType => this.VisitDynamicType( dynamicType ),
            INamedType namedType => this.VisitNamedType( namedType ),
            IPointerType pointerType => this.VisitPointerType( pointerType ),
            IFunctionPointerType functionPointerType => this.VisitFunctionPointerType( functionPointerType ),
            ITypeParameter typeParameter => this.VisitTypeParameter( typeParameter ),
            IFunctionPointerType or _ => throw new AssertionFailedException( $"Unexpected type: {type.GetType()}" ),
        };

    public abstract T DefaultVisit( IType type );

    public virtual T VisitArrayType( IArrayType arrayType ) => this.DefaultVisit( arrayType );

    public virtual T VisitDynamicType( IDynamicType dynamicType ) => this.DefaultVisit( dynamicType );

    public virtual T VisitNamedType( INamedType namedType ) => this.DefaultVisit( namedType );

    public virtual T VisitPointerType( IPointerType pointerType ) => this.DefaultVisit( pointerType );

    public virtual T VisitFunctionPointerType( IFunctionPointerType functionPointerType ) => this.DefaultVisit( functionPointerType );

    public virtual T VisitTypeParameter( ITypeParameter typeParameter ) => this.DefaultVisit( typeParameter );
}