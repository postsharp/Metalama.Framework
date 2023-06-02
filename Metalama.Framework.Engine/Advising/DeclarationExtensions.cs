// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using System.Linq;

namespace Metalama.Framework.Engine.Advising
{
    internal static class DeclarationExtensions
    {
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
        /// Determines whether the property's signature is equal to the signature of the given property.
        /// </summary>
        public static bool SignatureEquals( this IProperty property, IProperty other )
        {
            return property.Name == other.Name;
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