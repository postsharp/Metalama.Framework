// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Project;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Fabrics;

internal abstract partial class FabricDriver
{
    protected abstract class BaseAmender<T> : IAmender<T>, IDeclarationSelectorInternal
        where T : class, IDeclaration
    {
        // The Target property is protected (and not exposed to the API) because
        private readonly FabricInstance _fabricInstance;
        private readonly Ref<T> _targetDeclaration;
        private readonly FabricManager _fabricManager;

        protected BaseAmender(
            IProject project,
            FabricManager fabricManager,
            FabricInstance fabricInstance,
            in Ref<T> targetDeclaration )
        {
            this._fabricInstance = fabricInstance;
            this._targetDeclaration = targetDeclaration;
            this._fabricManager = fabricManager;
            this.Project = project;
        }

        public IProject Project { get; }

        protected abstract void AddAspectSource( IAspectSource aspectSource );

        protected abstract void AddValidatorSource( ProgrammaticValidatorSource validatorSource );

        public IDeclarationSelection<TChild> WithTargetMembers<TChild>( Func<T, IEnumerable<TChild>> selector )
            where TChild : class, IDeclaration
        {
            var executionContext = UserCodeExecutionContext.Current;

            return new DeclarationSelection<TChild>(
                this._targetDeclaration,
                this,
                this.AddAspectSource,
                this.AddValidatorSource,
                ( compilation, diagnostics ) =>
                {
                    var targetDeclaration = this._targetDeclaration.GetTarget( compilation ).AssertNotNull();

                    if ( !this._fabricManager.UserCodeInvoker.TryInvokeEnumerable(
                            () => selector( targetDeclaration ),
                            executionContext.WithDiagnosticAdder( diagnostics ),
                            out var targets ) )
                    {
                        return Enumerable.Empty<TChild>();
                    }
                    else
                    {
                        return targets;
                    }
                },
                this._fabricManager.AspectClasses,
                this._fabricManager.ServiceProvider );
        }

        public IDeclarationSelection<T> WithTarget() => this.WithTargetMembers( declaration => new[] { declaration } );

        [Obsolete( "Not implemented." )]
        public void AddAnnotation<TTarget, TAspect, TAnnotation>( Func<TTarget, TAnnotation?> provider )
            where TTarget : class, IDeclaration
            where TAspect : IAspect
            where TAnnotation : IAnnotation<TTarget, TAspect>
            => throw new NotImplementedException();

        public AspectPredecessor AspectPredecessor => new( AspectPredecessorKind.Fabric, this._fabricInstance );

        public ValidatorDriver<TContext> GetValidatorDriver<TContext>( string name ) => throw new NotImplementedException();
    }
}