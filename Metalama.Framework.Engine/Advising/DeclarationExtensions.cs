// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using System.Linq;

namespace Metalama.Framework.Engine.Advising
{
    internal static class DeclarationExtensions
    {
        public static bool SignatureEquals( this IMember declaration, IMember other )
        {
            return (declaration, other) switch
            {
                (IMethod x, IMethod y ) => SignatureEquals( x, y ),
                (IIndexer x, IIndexer y ) => SignatureEquals( x, y ),
                (IField x, IField y ) => SignatureEquals( x, y ),
                (IProperty x, IProperty y ) => SignatureEquals( x, y ),
                (IEvent x, IEvent y ) => SignatureEquals( x, y ),
                _ => throw new AssertionFailedException( $"Not expected ({declaration.DeclarationKind}, {other.DeclarationKind})" )
            };
        }

        /// <summary>
        /// Determines whether the method's signature is equal to the signature of the given method.
        /// </summary>
        public static bool SignatureEquals( this IMethod method, IMethod other )
        {
            // TODO: Custom modifiers.
            return method.TypeParameters.Count == other.TypeParameters.Count
                   && method.Parameters.Count == other.Parameters.Count
                   && method.Parameters
                       .Select( ( p, i ) => (p, i) )
                       .All(
                           amp =>
                               SignatureTypeSymbolComparer.Instance.Equals(
                                   amp.p.Type.GetSymbol().AssertNotNull(),
                                   other.Parameters[amp.i].Type.GetSymbol().AssertNotNull() )
                               && amp.p.RefKind == other.Parameters[amp.i].RefKind );
        }

        /// <summary>
        /// Determines whether the field's signature is equal to the signature of the given property.
        /// </summary>
        public static bool SignatureEquals( this IField field, IField other )
        {
            return field.Name == other.Name;
        }

        /// <summary>
        /// Determines whether the property's signature is equal to the signature of the given property.
        /// </summary>
        public static bool SignatureEquals( this IProperty property, IProperty other )
        {
            return property.Name == other.Name;
        }

        /// <summary>
        /// Determines whether the method's signature is equal to the signature of the given method.
        /// </summary>
        public static bool SignatureEquals( this IIndexer indexer, IIndexer other )
        {
            // TODO: Custom modifiers.
            return indexer.Parameters.Count == other.Parameters.Count
                   && indexer.Parameters
                       .Select( ( p, i ) => (p, i) )
                       .All(
                           amp =>
                               SignatureTypeSymbolComparer.Instance.Equals(
                                   amp.p.Type.GetSymbol().AssertNotNull(),
                                   other.Parameters[amp.i].Type.GetSymbol().AssertNotNull() )
                               && amp.p.RefKind == other.Parameters[amp.i].RefKind );
        }

        /// <summary>
        /// Determines whether the event's signature is equal to the signature of the given event.
        /// </summary>
        public static bool SignatureEquals( this IEvent @event, IEvent other )
        {
            return @event.Name == other.Name;
        }
    }
}