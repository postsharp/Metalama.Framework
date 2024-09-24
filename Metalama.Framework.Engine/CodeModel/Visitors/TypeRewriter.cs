// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Types;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel.Visitors
{
    /// <summary>
    /// A rewriter that allows to rewrite instances of the <see cref="IType"/> interface.
    /// </summary>
    internal abstract class TypeRewriter
    {
        protected static TypeRewriter Null { get; } = new NullRewriter();

        internal virtual IType Visit( IArrayType arrayType )
        {
            var elementType = this.Visit( arrayType.ElementType );

            if ( elementType == arrayType.ElementType )
            {
                return arrayType;
            }
            else if ( arrayType is ArrayType sourceArrayType )
            {
                return sourceArrayType.WithElementType( elementType );
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public virtual IType Visit( IType elementType ) => ((ITypeImpl) elementType).Accept( this );

        internal virtual IType Visit( IDynamicType dynamicType ) => dynamicType;

        internal virtual IType Visit( IPointerType pointerType )
        {
            var pointedAtType = this.Visit( pointerType.PointedAtType );

            if ( pointedAtType == pointerType.PointedAtType )
            {
                return pointerType;
            }
            else if ( pointerType is PointerType sourcePointerType )
            {
                return sourcePointerType.WithPointedAtType( pointedAtType );
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        internal virtual IType Visit( IFunctionPointerType functionPointerType )
        {
            throw new NotImplementedException( "Function pointers are not fully supported." );
        }

        internal virtual IType Visit( INamedType namedType )
        {
            if ( namedType.TypeArguments.Count == 0 )
            {
                return namedType;
            }
            else
            {
                var typeArguments = ImmutableArray.CreateBuilder<IType>( namedType.TypeArguments.Count );

                var hasChange = false;

                foreach ( var t in namedType.TypeArguments )
                {
                    var argumentType = this.Visit( t );
                    hasChange |= argumentType != t;
                    typeArguments.Add( argumentType );
                }

                if ( hasChange )
                {
                    if ( namedType is NamedType sourceNamedType )
                    {
                        return sourceNamedType.WithTypeArguments( typeArguments.MoveToImmutable() );
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    return namedType;
                }
            }
        }

        internal virtual IType Visit( ITypeParameter typeParameter ) => typeParameter;

        private sealed class NullRewriter : TypeRewriter
        {
            public override IType Visit( IType elementType ) => elementType;

            internal override IType Visit( IArrayType arrayType ) => arrayType;

            internal override IType Visit( IDynamicType dynamicType ) => dynamicType;

            internal override IType Visit( IPointerType pointerType ) => pointerType;

            internal override IType Visit( INamedType namedType ) => namedType;

            internal override IType Visit( ITypeParameter typeParameter ) => typeParameter;

            internal override IType Visit( IFunctionPointerType functionPointerType ) => functionPointerType;
        }
    }
}