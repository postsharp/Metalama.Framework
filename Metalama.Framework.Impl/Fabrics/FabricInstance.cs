// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Impl.Aspects;
using Metalama.Framework.Impl.CodeModel.References;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Impl.Fabrics
{
    internal class FabricInstance : IFabricInstance, IAspectPredecessorImpl
    {
        private readonly FabricDriver _driver;

        IRef<IDeclaration> IFabricInstance.TargetDeclaration => this.TargetDeclaration;

        public Ref<IDeclaration> TargetDeclaration { get; }

        public FabricInstance( FabricDriver driver, in Ref<IDeclaration> targetDeclaration )
        {
            this._driver = driver;
            this.TargetDeclaration = targetDeclaration;
        }

        public Fabric Fabric => this._driver.Fabric;

        public FormattableString FormatPredecessor( ICompilation compilation ) => this._driver.FormatPredecessor();

        public Location? GetDiagnosticLocation( Compilation compilation ) => this._driver.GetDiagnosticLocation();
    }
}