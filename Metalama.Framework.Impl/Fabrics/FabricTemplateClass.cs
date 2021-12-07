// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.Aspects;
using Metalama.Framework.Impl.CompileTime;
using Metalama.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Impl.Fabrics
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
            base( serviceProvider, compilation, fabricDriver.FabricSymbol, diagnosticAdder, baseClass )
        {
            this.Driver = fabricDriver;
            this.Project = project;
        }

        public override Type AspectType => this.Driver.Fabric.GetType();

        public override CompileTimeProject? Project { get; }

        public override string FullName => this.Driver.FabricSymbol.GetReflectionName();
    }
}