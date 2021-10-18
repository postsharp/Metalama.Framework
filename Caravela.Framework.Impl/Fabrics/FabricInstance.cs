// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Fabrics
{
    internal class FabricInstance : IFabricInstance, IAspectPredecessorImpl
    {
        private readonly FabricDriver _driver;

        public IDeclaration? TargetDeclaration { get; }

        public FabricInstance( FabricDriver driver, IDeclaration? targetDeclaration )
        {
            this._driver = driver;
            this.TargetDeclaration = targetDeclaration;
        }

        public IFabric Fabric => this._driver.Fabric;

        public FormattableString FormatPredecessor() => this._driver.FormatPredecessor();

        public Location? GetDiagnosticLocation( Compilation compilation ) => this._driver.GetDiagnosticLocation();
    }
}