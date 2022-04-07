// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Threading.Tasks;

namespace Metalama.Framework.Engine.CodeFixes;

/// <summary>
/// Unifies the implementation of code actions. This interface is not completely useful because the abstraction is unused, but it helps splitting every code
/// implementation into a class with a unified interface.
/// </summary>
internal interface ICodeAction
{
    Task ExecuteAsync( CodeActionContext context );
}