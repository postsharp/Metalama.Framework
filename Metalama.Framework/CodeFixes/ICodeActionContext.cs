// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Validation;
using System;
using System.Threading;

namespace Metalama.Framework.CodeFixes;

/// <summary>
/// Gets the context in which a code action executes. Exposed by <see cref="ICodeActionBuilder"/>.
/// If you implement your own code action using the Metalama SDK, you should cast this interface to <c>ISdkCodeActionContext</c>.
/// </summary>
[InternalImplement]
public interface ICodeActionContext
{
    /// <summary>
    /// Gets the cancellation token.
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets the service provider.
    /// </summary>
    IServiceProvider ServiceProvider { get; }
}