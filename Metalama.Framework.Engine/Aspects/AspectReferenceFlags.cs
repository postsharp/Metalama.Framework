// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Engine.Aspects;

/// <summary>
/// Provides flags on aspect reference, guiding the aspect linker.
/// </summary>
[Flags]
internal enum AspectReferenceFlags
{
    /// <summary>
    /// No flags are active on the aspect reference.
    /// </summary>
    None = 0,

    /// <summary>
    /// The reference is inlineable.
    /// </summary>
    Inlineable = 1,

    /// <summary>
    /// The reference has a custom receiver (i.e. it cannot be substituted with <c>this</c> or <c>base</c>).
    /// </summary>
    CustomReceiver = 2,

    /// <summary>
    /// The reference represents an implicitly inlineable invocation, i.e. sequence of arguments exactly matches the sequence of parameters of the containing method.
    /// For example "Foo_Override(int x, int y) => Foo(x,y)" would be inlineable implicitly and "Foo_Override(int x, int y) => Foo(y,x)" would not be.
    /// This flag is used for meta.Proceed, which never reorders parameters.
    /// </summary>
    ImplicitlyInlineableInvocation = 4
}