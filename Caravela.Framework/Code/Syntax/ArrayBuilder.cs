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

        internal object ItemType { get; }

        internal IReadOnlyList<object?> Items => this._items;

        private ArrayBuilder( IType itemType )
        {
            this.ItemType = itemType;
        }

        private ArrayBuilder( Type itemType )
        {
            this.ItemType = itemType;
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

        ISyntax ISyntaxBuilder.ToSyntax() => this.ToArray();

        /// <summary>
        /// Creates an <see cref="ArrayBuilder"/> where the item type is a given <see cref="IType"/>.
        /// </summary>
        public static ArrayBuilder Create( IType type ) => new( type );

        /// <summary>
        /// Creates an <see cref="ArrayBuilder"/> where the item type is a given <see cref="Type"/>.
        /// </summary>
        public static ArrayBuilder Create( Type type ) => new( type );

        /// <summary>
        /// Creates an <see cref="ArrayBuilder"/> where the item type is a given as a generic parameter.
        /// </summary>
        public static ArrayBuilder Create<T>() => new( typeof(T) );

        /// <summary>
        /// Creates an <see cref="ArrayBuilder"/> where the item type is <see cref="object"/>.
        /// </summary>
        public static ArrayBuilder Create() => new( typeof(object) );
    }
}