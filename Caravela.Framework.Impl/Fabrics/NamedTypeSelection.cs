// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Fabrics
{
    internal class NamedTypeSelection : DeclarationSelection<INamedType>, INamedTypeSelection
    {
        private IDeclarationSelection<T> WithMembers<T>( Func<INamedType, IEnumerable<T>> selector )
            where T : class, IMember
            => new DeclarationSelection<T>(
                this.RegisterAspectSource,
                compilation
                    => this.Selector( compilation )
                        .SelectMany( t => this.UserCodeInvoker.Wrap( this.UserCodeInvoker.Invoke( () => selector( t ) ) ) ),
                this.ServiceProvider,
                this.AspectClasses );

        public NamedTypeSelection(
            Action<IAspectSource> registerAspectSource,
            Func<CompilationModel, IEnumerable<INamedType>> selectTargets,
            IServiceProvider serviceProvider,
            AspectClassRegistry aspectClasses ) : base( registerAspectSource, selectTargets, serviceProvider, aspectClasses ) { }

        public IDeclarationSelection<IMethod> WithMethods( Func<INamedType, IEnumerable<IMethod>> selector ) => this.WithMembers( selector );

        public IDeclarationSelection<IProperty> WithProperties( Func<INamedType, IEnumerable<IProperty>> selector ) => this.WithMembers( selector );

        public IDeclarationSelection<IEvent> WithEvents( Func<INamedType, IEnumerable<IEvent>> selector ) => this.WithMembers( selector );

        public IDeclarationSelection<IField> WithFields( Func<INamedType, IEnumerable<IField>> selector ) => this.WithMembers( selector );

        public IDeclarationSelection<IConstructor> WithConstructors( Func<INamedType, IEnumerable<IConstructor>> selector ) => this.WithMembers( selector );
    }
}