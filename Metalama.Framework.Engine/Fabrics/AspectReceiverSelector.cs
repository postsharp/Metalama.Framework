// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Templating.MetaModel;
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
    private readonly CompilationModelVersion _version;

    internal AspectReceiverSelector( Ref<T> targetDeclaration, IAspectReceiverParent parent, CompilationModelVersion version )
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
                var targetDeclaration = this._targetDeclaration.GetTarget( compilation ).AssertNotNull();

                using var syntaxBuilder = SyntaxBuilder.WithImplementation( new SyntaxBuilderImpl( compilation, OurSyntaxGenerator.Default ) );

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
            this._version,
            ( compilation, _ ) => new[] { selector( this._targetDeclaration.GetTarget( compilation ) ) } );

    IValidatorReceiver<TMember> IValidatorReceiverSelector<T>.With<TMember>( Func<T, TMember> selector ) => this.With( selector );

    IValidatorReceiver<TMember> IValidatorReceiverSelector<T>.With<TMember>( Func<T, IEnumerable<TMember>> selector ) => this.With( selector );
}