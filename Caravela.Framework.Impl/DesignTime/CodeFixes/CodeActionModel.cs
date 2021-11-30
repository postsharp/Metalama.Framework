// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Impl.DesignTime.CodeFixes
{
    /// <summary>
    /// Represent a leaf in a code action menu.
    /// </summary>
    internal class CodeActionModel : CodeActionBaseModel
    {
        public Func<CancellationToken, Task<Solution>> Action { get; }

        public CodeActionModel( string title, Func<CancellationToken, Task<Solution>> action ) : base( title )
        {
            this.Action = action;
        }

        public override ImmutableArray<CodeAction> ToCodeActions( bool supportsHierarchicalItems, string titlePrefix = "" )
        {
            var title = titlePrefix + this.Title;

            return ImmutableArray.Create( CodeAction.Create( title, this.Action ) );
        }
    }
}