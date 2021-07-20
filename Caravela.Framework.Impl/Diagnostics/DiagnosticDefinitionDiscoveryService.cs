// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal class DiagnosticDefinitionDiscoveryService : IService
    {
        private readonly UserCodeInvoker _userCodeInvoker;

        public DiagnosticDefinitionDiscoveryService()
        {
            this._userCodeInvoker = new UserCodeInvoker( null );
        }

        public DiagnosticDefinitionDiscoveryService( IServiceProvider serviceProvider )
        {
            this._userCodeInvoker = serviceProvider.GetService<UserCodeInvoker>();
        }

        public IEnumerable<IDiagnosticDefinition> GetDiagnosticDefinitions( params Type[] type )
            => type.Select( this.GetDefinitions<IDiagnosticDefinition> ).SelectMany( d => d );

        public IEnumerable<SuppressionDefinition> GetSuppressionDefinitions( params Type[] type )
            => type.Select( this.GetDefinitions<SuppressionDefinition> ).SelectMany( d => d );

        private IEnumerable<T> GetDefinitions<T>( Type declaringTypes )
            => declaringTypes.GetFields( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )
                .Where( f => typeof(T).IsAssignableFrom( f.FieldType ) )
                .Select( f => (T) this._userCodeInvoker.Invoke( () => f.GetValue( null ) ) )
                .Concat(
                    declaringTypes.GetProperties( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )
                        .Where( p => typeof(T).IsAssignableFrom( p.PropertyType ) )
                        .Select( p => (T) this._userCodeInvoker.Invoke( () => p.GetValue( null ) ) ) );
    }
}