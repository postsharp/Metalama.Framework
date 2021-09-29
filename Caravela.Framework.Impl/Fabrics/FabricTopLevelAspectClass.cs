// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Sdk;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Threading;

namespace Caravela.Framework.Impl.Fabrics
{
    /// <summary>
    /// The top-level aspect class integrating the fabrics feature in the aspect pipeline. It is used as an 'identity'
    /// class. The real class is <see cref="FabricAggregateAspectClass"/>, which is instantiated in the middle of the pipeline,
    /// while <see cref="FabricTopLevelAspectClass"/> must exist while the pipeline is being instantiated.
    /// </summary>
    internal class FabricTopLevelAspectClass : IBoundAspectClass, IAspectClassImpl
    {
        public const string FabricAspectName = "<Fabric>";
        

        
        public AspectLayer Layer { get; }

        string IAspectClass.FullName => FabricAspectName;

        string IAspectClass.DisplayName => FabricAspectName;

        string? IAspectClass.Description => null;

        bool IAspectClass.IsAbstract => false;

        public FabricTopLevelAspectClass( IServiceProvider serviceProvider, Compilation compilation, CompileTimeProject project )
        {
            this.Layer = new AspectLayer( this, null );
            this.AspectDriver = new AspectDriver( serviceProvider, this, compilation );
            this.Project = project;
        }

        public IAspectDriver AspectDriver { get; }

        public Location? DiagnosticLocation => null;

        public CompileTimeProject? Project { get; }

        ImmutableArray<TemplateClass> IAspectClassImpl.TemplateClasses => ImmutableArray<TemplateClass>.Empty;
    }
    
  

   
}