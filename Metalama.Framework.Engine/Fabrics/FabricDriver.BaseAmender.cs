// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Project;
using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Metalama.Framework.Engine.Fabrics;

internal abstract partial class FabricDriver
{
    protected abstract class BaseAmender<T> : IAmender<T>, IAspectReceiverParent
        where T : class, IDeclaration
    {
        // The Target property is protected (and not exposed to the API) because
        private readonly FabricInstance _fabricInstance;
        private readonly Ref<T> _targetDeclaration;
        private readonly FabricManager _fabricManager;
        private AspectReceiverSelector<T>? _declarationSelector;

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

        private AspectReceiverSelector<T> GetAspectTargetSelector()
            => this._declarationSelector ??= new AspectReceiverSelector<T>( this._targetDeclaration, this, CompilationModelVersion.Initial );

        public IProject Project { get; }

        public abstract void AddAspectSource( IAspectSource aspectSource );

        public abstract void AddValidatorSource( IValidatorSource validatorSource );

        public IAspectReceiver<TChild> With<TChild>( Func<T, IEnumerable<TChild>> selector )
            where TChild : class, IDeclaration
            => this.GetAspectTargetSelector().With( selector );

        IAspectReceiver<TMember> IAspectReceiverSelector<T>.With<TMember>( Func<T, TMember> selector ) => this.GetAspectTargetSelector().With( selector );

        IValidatorReceiver<TMember> IValidatorReceiverSelector<T>.With<TMember>( Func<T, TMember> selector ) => this.GetAspectTargetSelector().With( selector );

        IValidatorReceiver<TMember> IValidatorReceiverSelector<T>.With<TMember>( Func<T, IEnumerable<TMember>> selector ) => this.With( selector );

        IServiceProvider IAspectReceiverParent.ServiceProvider => this._fabricManager.ServiceProvider;

        BoundAspectClassCollection IAspectReceiverParent.AspectClasses => this._fabricManager.AspectClasses;

        UserCodeInvoker IAspectReceiverParent.UserCodeInvoker => this._fabricManager.UserCodeInvoker;

        public AspectPredecessor AspectPredecessor => new( AspectPredecessorKind.Fabric, this._fabricInstance );

        Type IAspectReceiverParent.Type => this._fabricInstance.Fabric.GetType();

        ReferenceValidatorDriver IValidatorDriverFactory.GetReferenceValidatorDriver( MethodInfo validateMethod )
            => this._fabricInstance.ValidatorDriverFactory.GetReferenceValidatorDriver( validateMethod );

        DeclarationValidatorDriver IValidatorDriverFactory.GetDeclarationValidatorDriver( ValidatorDelegate<DeclarationValidationContext> validate )
            => this._fabricInstance.ValidatorDriverFactory.GetDeclarationValidatorDriver( validate );
    }
}