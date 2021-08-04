﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

// ReSharper disable RedundantUsingDirective

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Impl.Templating
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
        /// The <see cref="meta.Comment"/> method.
        /// </summary>
        Comment,

        /// <summary>
        /// The <see cref="meta.This"/> property.
        /// </summary>
        This,

        /// <summary>
        /// Any <c>meta.Proceed*</c> method.
        /// </summary>
        Proceed
    }
}