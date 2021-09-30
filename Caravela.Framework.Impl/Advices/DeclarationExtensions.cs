// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System.Linq;

namespace Caravela.Framework.Impl.Advices
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
                               method.Compilation.InvariantComparer.Equals( amp.p.Type, other.Parameters[amp.i].Type )
                               && amp.p.RefKind == other.Parameters[amp.i].RefKind );
        }

        public static bool SignatureEquals( this IProperty property, IProperty other )
        {
            return property.Name == other.Name
                   && property.Parameters.Count == other.Parameters.Count
                   && property.Parameters
                       .Select( ( p, i ) => (p, i) )
                       .All(
                           app =>
                               property.Compilation.InvariantComparer.Equals( app.p.Type, other.Parameters[app.i].Type )
                               && app.p.RefKind == other.Parameters[app.i].RefKind );
        }
    }
}