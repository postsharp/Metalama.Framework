// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Fabrics
{
    /// <summary>
    /// An implementation of <see cref="TemplateClass"/> that represents a fabric class.
    /// </summary>
    internal class FabricTemplateClass : TemplateClass
    {
        public FabricDriver Driver { get; }

        public FabricTemplateClass(
            FabricDriver fabricDriver,
            IServiceProvider serviceProvider,
            Compilation compilation,
            IDiagnosticAdder diagnosticAdder,
            TemplateClass? baseClass,
            CompileTimeProject project ) :
            base( serviceProvider, compilation, fabricDriver.FabricSymbol, diagnosticAdder, baseClass, fabricDriver.FabricSymbol.Name )
        {
            this.Driver = fabricDriver;
            this.Project = project;
        }

        public override Type Type => this.Driver.Fabric.GetType();

        internal override CompileTimeProject? Project { get; }

        public override string FullName => this.Driver.FabricSymbol.GetReflectionName().AssertNotNull();
    }
}