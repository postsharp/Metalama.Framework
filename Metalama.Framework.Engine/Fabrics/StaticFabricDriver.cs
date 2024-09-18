// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.HierarchicalOptions;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.Fabrics
{
    /// <summary>
    /// The base class for <see cref="ProjectFabricDriver"/> and <see cref="NamespaceFabricDriver"/>,
    /// which are executed when building the project configuration, not when executing the pipeline.
    /// </summary>
    internal abstract class StaticFabricDriver : FabricDriver
    {
        protected StaticFabricDriver( CreationData creationData ) :
            base( creationData ) { }

        public abstract bool TryExecute(
            IProject project,
            CompilationModel compilation,
            IDiagnosticAdder diagnosticAdder,
            [NotNullWhen( true )] out StaticFabricResult? result );

        protected class StaticAmender<T> : BaseAmender<T>
            where T : class, IDeclaration
        {
            private readonly List<IAspectSource> _aspectSources = new();
            private readonly List<IValidatorSource> _validatorSources = new();
            private readonly List<IHierarchicalOptionsSource> _optionsSources = new();

            protected StaticAmender( IProject project, FabricManager fabricManager, FabricInstance fabricInstance, IRef<T> targetDeclaration, string? ns ) :
                base( project, fabricManager, fabricInstance, targetDeclaration )
            {
                this.Namespace = ns;
            }

            public override string? Namespace { get; }

            public sealed override void AddAspectSource( IAspectSource aspectSource ) => this._aspectSources.Add( aspectSource );

            public override void AddValidatorSource( IValidatorSource validatorSource ) => this._validatorSources.Add( validatorSource );

            public override void AddOptionsSource( IHierarchicalOptionsSource hierarchicalOptionsSource )
                => this._optionsSources.Add( hierarchicalOptionsSource );

            public StaticFabricResult ToResult()
                => new( this._aspectSources.ToImmutableArray(), this._validatorSources.ToImmutableArray(), this._optionsSources.ToImmutableArray() );
        }
    }
}