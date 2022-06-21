// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using System.Linq;

namespace Metalama.Framework.Engine.Advising
{
    internal static class DeclarationExtensions
    {
        public static bool SignatureEquals( this IMethod method, IMethod other )
        {
            return method.Name == other.Name
                   && method.TypeParameters.Count == other.TypeParameters.Count
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

        public static bool SignatureEquals( this IProperty property, IProperty other )
        {
            return property.Name == other.Name;
        }

        public static bool SignatureEquals( this IEvent @event, IEvent other )
        {
            return @event.Name == other.Name;
        }

        public static bool SignatureEquals( this IIndexer property, IIndexer other )
        {
            return property.Name == other.Name
                   && property.Parameters.Count == other.Parameters.Count
                   && property.Parameters
                       .Select( ( p, i ) => (p, i) )
                       .All(
                           app =>
                               SignatureTypeSymbolComparer.Instance.Equals(
                                   app.p.Type.GetSymbol().AssertNotNull(),
                                   other.Parameters[app.i].Type.GetSymbol().AssertNotNull() )
                               && app.p.RefKind == other.Parameters[app.i].RefKind );
        }
    }
}