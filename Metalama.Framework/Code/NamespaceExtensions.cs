// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Code;

public static class NamespaceExtensions
{
    /// <summary>
    /// Gets a value indicating whether the current namespace is the ancestor of another given namespace.
    /// This method returns <c>false</c> when both namespaces are equal.
    /// </summary>
    public static bool IsAncestorOf( this INamespace a, INamespace b )
    {
        if ( a.IsGlobalNamespace )
        {
            return !b.IsGlobalNamespace;
        }

        var aFullName = a.FullName;
        var bFullName = b.FullName;

        return bFullName.StartsWith( aFullName, StringComparison.Ordinal ) && bFullName.Length > aFullName.Length && bFullName[aFullName.Length] == '.';
    }

    /// <summary>
    /// Gets a value indicating whether the current namespace is the descendant of another given namespace.
    /// This method returns <c>false</c> when both namespaces are equal.
    /// </summary>
    public static bool IsDescendantOf( this INamespace a, INamespace b ) => b.IsAncestorOf( a );
}