// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Templating
{
    /// <summary>
    /// Exposes a method that allows to resolve the original <see cref="Location"/> of a transformed node based on
    /// annotations.
    /// </summary>
    public interface ILocationAnnotationMap
    {
        Location? GetLocation( SyntaxNodeOrToken node );
    }
}