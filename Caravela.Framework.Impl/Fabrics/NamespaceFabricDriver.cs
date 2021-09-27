// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Utilities;
using System;

namespace Caravela.Framework.Impl.Fabrics
{
    internal class NamespaceFabricDriver : FabricDriver
    {
        private readonly string _ns;

        public NamespaceFabricDriver( IServiceProvider serviceProvider, AspectClassRegistry aspectClasses, IFabric fabric ) :
            base( serviceProvider, aspectClasses, fabric )
        {
            this._ns = NamespaceHelper.GetNamespace( this.Fabric.GetType().FullName );
        }

        public override FabricResult Execute( IProject project )
        {
            var builder = new Builder( this.ServiceProvider, project, this.AspectClasses, this._ns );
            ((INamespaceFabric) this.Fabric).BuildFabric( builder );

            return new FabricResult( builder );
        }

        public override FabricKind Kind => FabricKind.Namespace;

        public override string OrderingKey => this._ns;

        private class Builder : BaseBuilder<INamespace>, INamespaceFabricBuilder
        {
            private readonly string _ns;

            public Builder( IServiceProvider serviceProvider, IProject project, AspectClassRegistry aspectClasses, string ns ) : base(
                serviceProvider,
                project,
                aspectClasses )
            {
                this._ns = ns;
            }

            protected override INamespace GetTargetDeclaration( CompilationModel compilation ) => compilation.GetNamespace( this._ns ).AssertNotNull();
        }
    }
}