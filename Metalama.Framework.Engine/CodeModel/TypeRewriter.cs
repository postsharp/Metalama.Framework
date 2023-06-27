// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel
{
    /// <summary>
    /// A rewriter that allows to rewrite instances of the <see cref="IType"/> interface.
    /// </summary>
    internal abstract class TypeRewriter
    {
        protected static TypeRewriter Null { get; } = new NullRewriter();

        internal virtual ITypeImpl Visit( ArrayType arrayType ) => arrayType.WithElementType( this.Visit( arrayType.ElementType ) );

        public virtual ITypeImpl Visit( IType elementType ) => ((ITypeImpl) elementType).Accept( this );

        internal virtual ITypeImpl Visit( DynamicType dynamicType ) => dynamicType;

        internal virtual ITypeImpl Visit( PointerType pointerType ) => pointerType.WithPointedAtType( this.Visit( pointerType.PointedAtType ) );

        internal virtual ITypeImpl Visit( FunctionPointerType functionPointerType )
        {
            throw new NotImplementedException( "Function pointers are not fully supported." );
        }

        internal virtual ITypeImpl Visit( NamedType namedType )
        {
            if ( namedType.TypeArguments.Count == 0 )
            {
                return namedType;
            }
            else
            {
                var typeArguments = ImmutableArray.CreateBuilder<IType>( namedType.TypeArguments.Count );

                foreach ( var t in namedType.TypeArguments )
                {
                    typeArguments.Add( this.Visit( t ) );
                }

                return namedType.WithTypeArguments( typeArguments.MoveToImmutable() );
            }
        }

        internal virtual ITypeImpl Visit( TypeParameter typeParameter ) => typeParameter;

        private sealed class NullRewriter : TypeRewriter
        {
            public override ITypeImpl Visit( IType elementType ) => (ITypeImpl) elementType;

            internal override ITypeImpl Visit( ArrayType arrayType ) => arrayType;

            internal override ITypeImpl Visit( DynamicType dynamicType ) => dynamicType;

            internal override ITypeImpl Visit( PointerType pointerType ) => pointerType;

            internal override ITypeImpl Visit( NamedType namedType ) => namedType;

            internal override ITypeImpl Visit( TypeParameter typeParameter ) => typeParameter;

            internal override ITypeImpl Visit( FunctionPointerType functionPointerType ) => functionPointerType;
        }
    }
}