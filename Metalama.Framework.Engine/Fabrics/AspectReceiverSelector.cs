// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Fabrics;

internal class AspectReceiverSelector<T> : IAspectReceiverSelector<T>
    where T : class, IDeclaration
{
    private readonly Ref<T> _targetDeclaration;
    private readonly IAspectReceiverParent _parent;

    internal AspectReceiverSelector( Ref<T> targetDeclaration, IAspectReceiverParent parent )
    {
        this._targetDeclaration = targetDeclaration;
        this._parent = parent;
    }

    public IAspectReceiver<TMember> With<TMember>( Func<T, IEnumerable<TMember>> selector )
        where TMember : class, IDeclaration
    {
        var executionContext = UserCodeExecutionContext.Current;

        return new AspectReceiver<TMember>(
            this._targetDeclaration,
            this._parent,
            CompilationModelVersion.Current,
            ( compilation, diagnostics ) =>
            {
                var targetDeclaration = this._targetDeclaration.GetTarget( compilation ).AssertNotNull();

                if ( !this._parent.UserCodeInvoker.TryInvokeEnumerable(
                        () => selector( targetDeclaration ),
                        executionContext.WithDiagnosticAdder( diagnostics ),
                        out var targets ) )
                {
                    return Enumerable.Empty<TMember>();
                }
                else
                {
                    return targets;
                }
            } );
    }

    public IAspectReceiver<TMember> With<TMember>( Func<T, TMember> selector )
        where TMember : class, IDeclaration
        => new AspectReceiver<TMember>(
            this._targetDeclaration,
            this._parent,
            CompilationModelVersion.Current,
            ( compilation, _ ) => new[] { selector( this._targetDeclaration.GetTarget( compilation ) ) } );

    IValidatorReceiver<TMember> IValidatorReceiverSelector<T>.With<TMember>( Func<T, TMember> selector ) => this.With( selector );

    IValidatorReceiver<TMember> IValidatorReceiverSelector<T>.With<TMember>( Func<T, IEnumerable<TMember>> selector ) => this.With( selector );
}