// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Caravela.Framework.Sdk
{
    public interface IPartialCompilation
    {
        Compilation Compilation { get; }

        IReadOnlyCollection<SyntaxTree> SyntaxTrees { get; }

        bool IsPartial { get; }

        public IPartialCompilation UpdateSyntaxTrees( IEnumerable<(SyntaxTree OldTree, SyntaxTree NewTree)> replacements, IEnumerable<SyntaxTree> addedTrees );
    }
}