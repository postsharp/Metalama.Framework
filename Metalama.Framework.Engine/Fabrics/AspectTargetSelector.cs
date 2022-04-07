// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Fabrics;

internal class AspectTargetSelector<T> : IAspectTargetSelector<T>
    where T : class, IDeclaration
{
    private readonly CompilationModelVersion _version;
    private readonly Ref<T> _targetDeclaration;
    private readonly ICompilationSelectorParent _parent;

    public AspectTargetSelector( Ref<T> targetDeclaration, ICompilationSelectorParent parent ) : this(
        CompilationModelVersion.Current,
        targetDeclaration,
        parent ) { }

    internal AspectTargetSelector( CompilationModelVersion version, Ref<T> targetDeclaration, ICompilationSelectorParent parent )
    {
        this._version = version;
        this._targetDeclaration = targetDeclaration;
        this._parent = parent;
    }

    public IAspectReceiver<TMember> WithTargetMembers<TMember>( Func<T, IEnumerable<TMember>> selector )
        where TMember : class, IDeclaration
    {
        var executionContext = UserCodeExecutionContext.Current;

        return new CompilationSelection<TMember>(
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

    IValidatorReceiver<T> IValidatorTargetSelector<T>.WithTarget() => this.WithTarget();

    IValidatorReceiver<TMember> IValidatorTargetSelector<T>.WithTargetMembers<TMember>( Func<T, IEnumerable<TMember>> selector )
        => this.WithTargetMembers( selector );

    public IAspectReceiver<T> WithTarget() => this.WithTargetMembers<T>( x => new[] { x } );

    public IValidatorTargetSelector<T> AfterAllAspects()
        => this._version == CompilationModelVersion.Final
            ? this
            : new AspectTargetSelector<T>( CompilationModelVersion.Final, this._targetDeclaration, this._parent );

    public IValidatorTargetSelector<T> BeforeAnyAspect()
        => this._version == CompilationModelVersion.Initial
            ? this
            : new AspectTargetSelector<T>( CompilationModelVersion.Initial, this._targetDeclaration, this._parent );
}