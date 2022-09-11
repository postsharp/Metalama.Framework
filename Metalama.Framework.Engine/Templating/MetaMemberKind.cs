// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

// ReSharper disable RedundantUsingDirective

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Engine.Templating
{
    internal enum MetaMemberKind
    {
        /// <summary>
        /// Not a member of the <see cref="meta"/> class.
        /// </summary>
        None,

        /// <summary>
        /// A default member.
        /// </summary>
        Default,

        /// <summary>
        /// The <see cref="meta.InsertComment"/> method.
        /// </summary>
        InsertComment,

        /// <summary>
        /// The <see cref="meta.InsertStatement(Metalama.Framework.Code.SyntaxBuilders.IStatement)"/> method (or another overload).
        /// </summary>
        InsertStatement,

        /// <summary>
        /// The <see cref="meta.This"/> property.
        /// </summary>
        This,

        Proceed,

        ProceedAsync,

        ProceedEnumerable,

        ProceedEnumerator,

        ProceedAsyncEnumerable,

        ProceedAsyncEnumerator,

        Expression
    }
}