// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
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
        protected StaticFabricDriver( FabricManager fabricManager, Fabric fabric, Compilation runTimeCompilation ) :
            base(
                fabricManager,
                fabric,
                runTimeCompilation ) { }

        public abstract bool TryExecute( IProject project, IDiagnosticAdder diagnosticAdder, [NotNullWhen( true )] out StaticFabricResult? result );

        protected class StaticAmender<T> : BaseAmender<T>
            where T : class, IDeclaration
        {
            private readonly List<IAspectSource> _aspectSources = new();

            protected StaticAmender( IProject project, FabricManager fabricManager, FabricInstance fabricInstance, in Ref<T> targetDeclaration ) :
                base( project, fabricManager, fabricInstance, targetDeclaration ) { }

            protected sealed override void AddAspectSource( IAspectSource aspectSource ) => this._aspectSources.Add( aspectSource );

            public StaticFabricResult ToResult() => new( this._aspectSources.ToImmutableArray() );
        }
    }
}