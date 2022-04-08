﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
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
    private readonly CompilationModelVersion _version;
    private readonly Ref<T> _targetDeclaration;
    private readonly IAspectReceiverParent _parent;

    public AspectReceiverSelector( Ref<T> targetDeclaration, IAspectReceiverParent parent ) : this(
        CompilationModelVersion.Current,
        targetDeclaration,
        parent ) { }

    internal AspectReceiverSelector( CompilationModelVersion version, Ref<T> targetDeclaration, IAspectReceiverParent parent )
    {
        this._version = version;
        this._targetDeclaration = targetDeclaration;
        this._parent = parent;
    }

    public IAspectReceiver<TMember> WithTargetMembers<TMember>( Func<T, IEnumerable<TMember>> selector )
        where TMember : class, IDeclaration
    {
        var executionContext = UserCodeExecutionContext.Current;

        return new AspectReceiver<TMember>(
            this._targetDeclaration,
            this._parent,
            this._version,
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

    IValidatorReceiver<T> IValidatorReceiverSelector<T>.WithTarget() => this.WithTarget();

    IValidatorReceiver<TMember> IValidatorReceiverSelector<T>.WithTargetMembers<TMember>( Func<T, IEnumerable<TMember>> selector )
        => this.WithTargetMembers( selector );

    public IAspectReceiver<T> WithTarget() => this.WithTargetMembers( x => new[] { x } );

    public IValidatorReceiverSelector<T> AfterAllAspects()
        => this._version == CompilationModelVersion.Final
            ? this
            : new AspectReceiverSelector<T>( CompilationModelVersion.Final, this._targetDeclaration, this._parent );

    public IValidatorReceiverSelector<T> BeforeAnyAspect()
        => this._version == CompilationModelVersion.Initial
            ? this
            : new AspectReceiverSelector<T>( CompilationModelVersion.Initial, this._targetDeclaration, this._parent );
}