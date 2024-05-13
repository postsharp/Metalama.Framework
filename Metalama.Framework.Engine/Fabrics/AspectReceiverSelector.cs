// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Fabrics;

internal sealed class AspectReceiverSelector<T> : IAspectReceiverSelector<T>
    where T : class, IDeclaration
{
    private readonly Ref<T> _targetDeclaration;
    private readonly IAspectReceiverParent _parent;
    private readonly CompilationModelVersion _version;

    internal AspectReceiverSelector( in Ref<T> targetDeclaration, IAspectReceiverParent parent, CompilationModelVersion version )
    {
        this._targetDeclaration = targetDeclaration;
        this._parent = parent;
        this._version = version;
    }

    public IAspectReceiver<TMember> With<TMember>( Func<T, IEnumerable<TMember>> selector )
        where TMember : class, IDeclaration
    {
        var executionContext = UserCodeExecutionContext.Current;

        return new AspectReceiver<TMember>(
            this._targetDeclaration,
            this._parent,
            this._version,
            ( compilation, diagnostics ) =>
            {
                var targetDeclaration = this._targetDeclaration.GetTargetOrNull( compilation );

                if ( targetDeclaration == null )
                {
                    // This happens at design time during a background rebuild, but we don't want to fail because of this.
                    return Enumerable.Empty<TMember>();
                }

                if ( !this._parent.UserCodeInvoker.TryInvokeEnumerable(
                        () => selector( targetDeclaration ),
                        executionContext.WithCompilationAndDiagnosticAdder( compilation, diagnostics ),
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
            this._version,
            ( compilation, _ ) => new[] { selector( this._targetDeclaration.GetTarget( compilation ) ) } );

    IValidatorReceiver<TMember> IValidatorReceiverSelector<T>.With<TMember>( Func<T, TMember> selector ) => this.With( selector );

    IValidatorReceiver<TMember> IValidatorReceiverSelector<T>.With<TMember>( Func<T, IEnumerable<TMember>> selector ) => this.With( selector );
}