// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Advising;
using System;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// An exception thrown by <see cref="IAdviceFactory"/> when compile-time code attempts to add a template
    /// to a target declaration and the template is not compatible with the advice and the target declaration.
    /// </summary>
    [CompileTime]
    public sealed class InvalidTemplateSignatureException : Exception
    {
        internal InvalidTemplateSignatureException( string message ) : base( message ) { }
    }
}