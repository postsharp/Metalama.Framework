// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Tests.UnitTests.Utilities
{
    internal static class CodeModelExtensions
    {
        public static ImmutableArray<T> OrderBySource<T>( this IEnumerable<T> items )
            where T : IDeclaration
            => items.Select( item => (Item: item, Declaration: item.GetPrimaryDeclaration()) )
                .OrderBy( item => item.Declaration?.SyntaxTree.FilePath )
                .ThenBy( item => item.Declaration?.SpanStart )
                .Select( item => item.Item )
                .ToImmutableArray();

    }
}