// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.HierarchicalOptions;
using Metalama.Framework.Engine.Licensing;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Project;
using Metalama.Framework.Validation;
using System;
using System.Reflection;

namespace Metalama.Framework.Engine.Fabrics;

internal abstract partial class FabricDriver
{
    protected abstract class BaseAmender<T> : AspectReceiver<T, int>, IAmender<T>, IAspectReceiverParent
        where T : class, IDeclaration
    {
        // The Target property is protected (and not exposed to the API) because
        private readonly FabricInstance _fabricInstance;

        private Ref<T> TargetDeclaration { get; }

        private readonly FabricManager _fabricManager;
        private readonly IProject _project;

        protected BaseAmender(
            IProject project,
            FabricManager fabricManager,
            FabricInstance fabricInstance,
            Ref<T> targetDeclaration ) : base(
            fabricManager.ServiceProvider,
            targetDeclaration,
            CompilationModelVersion.Final,
            ( action, context ) => action( targetDeclaration.GetTarget( context.Compilation ), 0, context ) )
        {
            this._project = project;
            this._fabricInstance = fabricInstance;
            this.TargetDeclaration = targetDeclaration;
            this._fabricManager = fabricManager;
            this.LicenseVerifier = this._fabricManager.ServiceProvider.GetService<LicenseVerifier>();
        }

        protected override bool ShouldCache => false;

        protected override IAspectReceiverParent Parent => this;

        IProject IAspectReceiverParent.Project => this._project;

        public abstract string? Namespace { get; }

        public LicenseVerifier? LicenseVerifier { get; }

        public abstract void AddAspectSource( IAspectSource aspectSource );

        public abstract void AddValidatorSource( IValidatorSource validatorSource );

        public abstract void AddOptionsSource( IHierarchicalOptionsSource hierarchicalOptionsSource );

        ProjectServiceProvider IAspectReceiverParent.ServiceProvider => this._fabricManager.ServiceProvider;

        BoundAspectClassCollection IAspectReceiverParent.AspectClasses => this._fabricManager.AspectClasses;

        UserCodeInvoker IAspectReceiverParent.UserCodeInvoker => this._fabricManager.UserCodeInvoker;

        public AspectPredecessor AspectPredecessor => new( AspectPredecessorKind.Fabric, this._fabricInstance );

        Type IAspectReceiverParent.Type => this._fabricInstance.Fabric.GetType();

        MethodBasedReferenceValidatorDriver IValidatorDriverFactory.GetReferenceValidatorDriver( MethodInfo validateMethod )
            => this._fabricInstance.ValidatorDriverFactory.GetReferenceValidatorDriver( validateMethod );

        ClassBasedReferenceValidatorDriver IValidatorDriverFactory.GetReferenceValidatorDriver( Type type )
            => this._fabricInstance.ValidatorDriverFactory.GetReferenceValidatorDriver( type );

        DeclarationValidatorDriver IValidatorDriverFactory.GetDeclarationValidatorDriver( ValidatorDelegate<DeclarationValidationContext> validate )
            => this._fabricInstance.ValidatorDriverFactory.GetDeclarationValidatorDriver( validate );

        [Memo]
        public IAspectReceiver<T> Outbound
            => new RootAspectReceiver<T>(
                this.TargetDeclaration,
                this,
                CompilationModelVersion.Final );

        string IDiagnosticSource.DiagnosticSourceDescription => $"fabric {this._fabricInstance.Fabric.GetType().FullName}";
    }
}