// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Fabrics
{
    internal class FabricInstance : IFabricInstance, IAspectPredecessorImpl
    {
        public FabricDriver Driver { get; }

        IRef<IDeclaration>? IFabricInstance.TargetDeclaration => this.TargetDeclaration.IsDefault ? null : this.TargetDeclaration;

        public Ref<IDeclaration> TargetDeclaration { get; }

        public FabricInstance( FabricDriver driver, in Ref<IDeclaration> targetDeclaration )
        {
            this.Driver = driver;
            this.TargetDeclaration = targetDeclaration;
        }

        public Fabric Fabric => this.Driver.Fabric;

        public FormattableString FormatPredecessor( ICompilation compilation ) => this.Driver.FormatPredecessor();

        public Location? GetDiagnosticLocation( Compilation compilation ) => this.Driver.GetDiagnosticLocation();
    }
}