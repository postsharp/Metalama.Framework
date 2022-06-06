// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel
{
    /// <summary>
    /// A rewriter that allows to rewrite instances of the <see cref="IType"/> interface.
    /// </summary>
    internal abstract class TypeRewriter
    {
        public static TypeRewriter Null { get; } = new NullRewriter();

        internal virtual ITypeInternal Visit( ArrayType arrayType ) => arrayType.WithElementType( this.Visit( arrayType.ElementType ) );

        public virtual ITypeInternal Visit( IType elementType ) => ((ITypeInternal) elementType).Accept( this );

        internal virtual ITypeInternal Visit( DynamicType dynamicType ) => dynamicType;

        internal virtual ITypeInternal Visit( PointerType pointerType ) => pointerType.WithPointedAtType( this.Visit( pointerType.PointedAtType ) );

        internal virtual ITypeInternal Visit( NamedType namedType )
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

        internal virtual ITypeInternal Visit( TypeParameter typeParameter ) => typeParameter;

        private class NullRewriter : TypeRewriter
        {
            public override ITypeInternal Visit( IType elementType ) => (ITypeInternal) elementType;

            internal override ITypeInternal Visit( ArrayType arrayType ) => arrayType;

            internal override ITypeInternal Visit( DynamicType dynamicType ) => dynamicType;

            internal override ITypeInternal Visit( PointerType pointerType ) => pointerType;

            internal override ITypeInternal Visit( NamedType namedType ) => namedType;

            internal override ITypeInternal Visit( TypeParameter typeParameter ) => typeParameter;
        }
    }
}