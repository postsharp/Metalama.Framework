// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Code.Syntax
{
    /// <summary>
    /// Compile-time object that allows to build a run-time array.
    /// </summary>
    [CompileTimeOnly]
    public sealed class ArrayBuilder : ISyntaxBuilder
    {
        private readonly List<object?> _items = new();

        internal IType ItemType { get; }

        internal IReadOnlyList<object?> Items => this._items;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayBuilder"/> class where the item type is a given <see cref="IType"/>.
        /// </summary>
        public ArrayBuilder( IType itemType )
        {
            this.ItemType = itemType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayBuilder"/> class where the item type is a given <see cref="Type"/>.
        /// </summary>
        public ArrayBuilder( Type itemType ) : this( meta.Target.Compilation.TypeFactory.GetTypeByReflectionType( itemType ) ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayBuilder"/> class where the item type is a <see cref="object"/>.
        /// </summary>
        public ArrayBuilder() : this( meta.Target.Compilation.TypeFactory.GetSpecialType( SpecialType.Object ) ) { }

        private ArrayBuilder( ArrayBuilder prototype )
        {
            this._items.AddRange( prototype._items );
            this.ItemType = prototype.ItemType;
        }

        /// <summary>
        /// Adds an item to the array.
        /// </summary>
        public void Add( dynamic? expression ) => this._items.Add( (object?) expression );

        /// <summary>
        /// Converts the current <see cref="ArrayBuilder"/> to syntax that represents the array.
        /// </summary>
        /// <returns></returns>
        public dynamic ToArray() => meta.CurrentContext.CodeBuilder.BuildArray( this );

        ISyntax ISyntaxBuilder.ToSyntax() => (ISyntax) meta.CurrentContext.CodeBuilder.BuildArray( this );

        public ArrayBuilder Clone() => new( this );
    }
}