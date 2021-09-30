// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// An exception thrown by <see cref="IAdviceFactory"/> when compile-time code attempts to add a template
    /// to a target declaration and the template is not compatible with the target declaration.
    /// </summary>
    [CompileTimeOnly]
    public sealed class InvalidAdviceTargetException : Exception
    {
        internal InvalidAdviceTargetException( string message ) : base( message ) { }
    }
}