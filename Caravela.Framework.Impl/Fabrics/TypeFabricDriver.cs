// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Fabrics;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using Attribute = System.Attribute;

namespace Caravela.Framework.Impl.Fabrics
{
    internal class TypeFabricAspectSource : IAspectSource
    {
        public AspectSourcePriority Priority => throw new NotImplementedException();

        public IEnumerable<AspectClass> AspectTypes => throw new NotImplementedException();

        public IEnumerable<IDeclaration> GetExclusions( INamedType aspectType ) => throw new NotImplementedException();

        public IEnumerable<AspectInstance> GetAspectInstances(
            CompilationModel compilation,
            AspectClass aspectClass,
            IDiagnosticAdder diagnosticAdder,
            CancellationToken cancellationToken )
            => throw new NotImplementedException();
    }

    internal class TypeFabricDriver : FabricDriver
    {
        private readonly string _targetTypeName;

        public TypeFabricDriver( IServiceProvider serviceProvider, AspectClassRegistry aspectClasses, IFabric fabric ) : base(
            serviceProvider,
            aspectClasses,
            fabric )
        {
            var attribute = this.Fabric.GetType().GetCustomAttribute<FabricAttribute>();
            this._targetTypeName = attribute.TargetTypeName.AssertNotNull();
        }

        public override FabricResult Execute( IProject project )
        {
            var builder = new Builder( this.ServiceProvider, project, this.AspectClasses, this._targetTypeName );
            ((ITypeFabric) this.Fabric).BuildFabric( builder );

            return new FabricResult( builder );
        }

        public override FabricKind Kind => FabricKind.Type;

        public override string OrderingKey => this._targetTypeName;

        private class Builder : BaseBuilder<INamedType>, ITypeFabricBuilder
        {
            private readonly string _targetTypeName;
            private readonly NamedTypeSelection _namedTypeSelection;

            public Builder( IServiceProvider serviceProvider, IProject project, AspectClassRegistry aspectClasses, string targetTypeName ) : base(
                serviceProvider,
                project,
                aspectClasses )
            {
                this._targetTypeName = targetTypeName;

                this._namedTypeSelection = new NamedTypeSelection(
                    this.RegisterAspectSource,
                    compilation => new[] { compilation.Factory.GetTypeByReflectionName( this._targetTypeName ) },
                    serviceProvider,
                    aspectClasses );
            }

            protected override INamedType GetTargetDeclaration( CompilationModel compilation )
                => compilation.Factory.GetTypeByReflectionName( this._targetTypeName );

            public void AddAspect<TAspect>( Func<INamedType, Expression<Func<TAspect>>> createAspect )
                where TAspect : Attribute, IAspect<INamedType>
                => this._namedTypeSelection.AddAspect( createAspect );

            public void AddAspect<TAspect>( Func<INamedType, TAspect> createAspect )
                where TAspect : Attribute, IAspect<INamedType>
                => this._namedTypeSelection.AddAspect( createAspect );

            public void AddAspect<TAspect>()
                where TAspect : Attribute, IAspect<INamedType>, new()
                => this._namedTypeSelection.AddAspect<TAspect>();

            [Obsolete( "Not implemented." )]
            public void RequireAspect<TTarget, TAspect>( TTarget target )
                where TTarget : class, IDeclaration
                where TAspect : IAspect<TTarget>, new()
                => this._namedTypeSelection.RequireAspect<TTarget, TAspect>( target );

            [Obsolete( "Not implemented." )]
            public void AddAnnotation<TAspect, TAnnotation>( Func<INamedType, TAnnotation> getAnnotation )
                where TAspect : IAspect
                where TAnnotation : IAnnotation<INamedType, TAspect>, IEligible<INamedType>
                => this._namedTypeSelection.AddAnnotation<TAspect, TAnnotation>( getAnnotation );

            public IDeclarationSelection<IMethod> WithMethods( Func<INamedType, IEnumerable<IMethod>> selector )
                => this._namedTypeSelection.WithMethods( selector );

            public IDeclarationSelection<IProperty> WithProperties( Func<INamedType, IEnumerable<IProperty>> selector )
                => this._namedTypeSelection.WithProperties( selector );

            public IDeclarationSelection<IEvent> WithEvents( Func<INamedType, IEnumerable<IEvent>> selector )
                => this._namedTypeSelection.WithEvents( selector );

            public IDeclarationSelection<IField> WithFields( Func<INamedType, IEnumerable<IField>> selector )
                => this._namedTypeSelection.WithFields( selector );

            public IDeclarationSelection<IConstructor> WithConstructors( Func<INamedType, IEnumerable<IConstructor>> selector )
                => this._namedTypeSelection.WithConstructors( selector );
        }
    }
}