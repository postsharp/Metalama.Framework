using System.Collections.Generic;

#nullable enable

namespace Caravela.Framework.Impl.AspectOrdering
{
    /// <summary>
    /// Minimalist implementation of a one-direction linked list.
    /// </summary>
    /// <typeparam name="T">Type of values.</typeparam>
    /// <remarks>There is no node implementing the <i>list</i>. Everything
    /// is a <i>node</i>. When using the <see cref="IEnumerable{T}"/> interface,
    /// you get an enumeration of the current node and all the next nodes.</remarks>
    internal sealed class SimpleLinkedListNode<T>
    {

        /// <summary>
        /// Inserts a value at the beginning of a list.
        /// </summary>
        /// <param name="node">Reference to the head node. May safely be a reference to a <c>null</c> node.</param>
        /// <param name="value">Value.</param>
        public static void Insert( ref SimpleLinkedListNode<T>? node, T value ) => node = new SimpleLinkedListNode<T>( value, node );

        /// <summary>
        /// Appends a value at the end of a list.
        /// </summary>
        /// <param name="node">Reference to a node of the list. May safely be a reference to a <c>null</c> node.</param>
        /// <param name="value">Value.</param>
        public static void Append( ref SimpleLinkedListNode<T>? node, T value )
        {
            if ( node == null )
            {
                node = new SimpleLinkedListNode<T>( value, null );
            }
            else
            {
                var last = node.GetLast();
                last.Next = new SimpleLinkedListNode<T>( value, null );
            }
        }

        /// <summary>
        /// Appends a list at the end of another one.
        /// </summary>
        /// <param name="node">Reference of a node of the 'left' list.
        /// May safely be a reference to a <c>null</c> node.</param>
        /// <param name="list">Reference to the head of the 'right' list.
        /// May safely be a <c>null</c> node.</param>
        /// <remarks>
        /// The 'right' list (<paramref name="list"/>) is cloned, so
        /// nodes are never shared between lists.
        /// </remarks>
        public static void Append( ref SimpleLinkedListNode<T>? node, SimpleLinkedListNode<T>? list )
        {
            if ( list == null )
            {
                return;
            }

            if ( node == null )
            {
                node = list.Clone();
            }
            else
            {
                var last = node.GetLast();
                last.Next = list.Clone();
            }
        }

        /// <summary>
        /// Finds a node in a list and removes it.
        /// </summary>
        /// <param name="node">Reference to the head node. May safely be a reference to a <c>null</c> node.</param>
        /// <param name="value">The value to remove.</param>
        /// <returns><c>true</c> if the node was found and removed, otherwise <c>false</c>.</returns>
        public static bool Remove( ref SimpleLinkedListNode<T>? node, T value )
        {
            if ( node == null )
            {
                return false;
            }

            if ( (value == null && node.Value == null) || Equals( node.Value, value ) )
            {
                node = node.Next;
                return true;
            }

            var previousNode = node;

            for ( var cursor = node.Next; cursor != null; cursor = cursor.Next )
            {
                if ( (value == null && cursor.Value == null) || Equals( cursor.Value, value ) )
                {
                    previousNode.Next = cursor.Next;
                    return true;
                }

                previousNode = cursor;
            }

            return false;
        }
        
        private SimpleLinkedListNode()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLinkedListNode{T}"/> class.
        /// </summary>
        /// <param name="value">Node value.</param>
        /// <param name="next">Next node.</param>
        public SimpleLinkedListNode( T value, SimpleLinkedListNode<T>? next )
        {
            this.Value = value;
            this.Next = next;
        }

        /// <summary>
        /// Gets or sets the node value.
        /// </summary>
        public T? Value { get; set; }

        /// <summary>
        /// Gets or sets the next node.
        /// </summary>
        public SimpleLinkedListNode<T>? Next { get; internal set; }

        /// <summary>
        /// Gets the last node in the list.
        /// </summary>
        /// <returns>The last node in the list.</returns>
        public SimpleLinkedListNode<T> GetLast()
        {
            var cursor = this;
            var last = this;

            while ( cursor != null )
            {
                last = cursor;
                cursor = cursor.Next;
            }

            return last;
        }

        /// <summary>
        /// Clone the current node (but not the value).
        /// </summary>
        /// <returns>A copy of the current node.</returns>
        private SimpleLinkedListNode<T> Clone()
        {
            SimpleLinkedListNode<T> clonedRoot = new();

            var cursor = this;
            var clonedCursor = clonedRoot;

            while ( true )
            {
                clonedRoot.Value = cursor.Value;

                if ( cursor.Next != null )
                {
                    clonedCursor.Next = new SimpleLinkedListNode<T>();
                    clonedCursor = clonedCursor.Next;
                    cursor = cursor.Next;
                }
                else
                {
                    break;
                }
            }

            return clonedRoot;
        }
    }
}