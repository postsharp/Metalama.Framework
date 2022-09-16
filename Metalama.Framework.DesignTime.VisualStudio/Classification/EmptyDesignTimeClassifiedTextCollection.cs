// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using Microsoft.CodeAnalysis.Text;

namespace Metalama.Framework.DesignTime.VisualStudio.Classification;

internal class EmptyDesignTimeClassifiedTextCollection : IDesignTimeClassifiedTextCollection
{
    public static EmptyDesignTimeClassifiedTextCollection Instance { get; } = new();

    private EmptyDesignTimeClassifiedTextCollection() { }

    public IEnumerable<DesignTimeClassifiedTextSpan> GetClassifiedTextSpans() => Enumerable.Empty<DesignTimeClassifiedTextSpan>();

    public IEnumerable<DesignTimeClassifiedTextSpan> GetClassifiedTextSpans( TextSpan textSpan ) => Enumerable.Empty<DesignTimeClassifiedTextSpan>();
}