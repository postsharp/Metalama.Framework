// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.CodeFixes;

/// <summary>
/// Represents a code fix.
/// </summary>
internal abstract class CodeActionBase
{
    public Task ExecuteAsync( CodeActionContext context )
    {
        if ( !context.ComputingPreview )
        {
            throw new NotImplementedException( "TODO: check licensing" );
        }

        return this.ExecuteImplAsync( context );
    }

    protected abstract Task ExecuteImplAsync( CodeActionContext context );
}