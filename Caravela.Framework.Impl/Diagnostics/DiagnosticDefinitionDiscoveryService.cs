// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code.Collections;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal class DiagnosticDefinitionDiscoveryService : IService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly UserCodeInvoker _userCodeInvoker;

        // This constructor is called in a path where no user code is involved
        public DiagnosticDefinitionDiscoveryService() : this( ServiceProvider.Empty.WithServices( new UserCodeInvoker( ServiceProvider.Empty ) ) ) { }

        public DiagnosticDefinitionDiscoveryService( IServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider;
            this._userCodeInvoker = serviceProvider.GetService<UserCodeInvoker>();
        }

        public IEnumerable<IDiagnosticDefinition> GetDiagnosticDefinitions( params Type[] type )
            => type.Select( this.GetDefinitions<IDiagnosticDefinition> ).SelectMany( d => d );

        public IEnumerable<SuppressionDefinition> GetSuppressionDefinitions( params Type[] type )
            => type.Select( this.GetDefinitions<SuppressionDefinition> ).SelectMany( d => d );

        private IEnumerable<T> GetDefinitions<T>( Type declaringTypes )
            where T : class
        {
            T? GetFieldValue( FieldInfo f )
            {
                var executionContext = new UserCodeExecutionContext(
                    this._serviceProvider,
                    NullDiagnosticAdder.Instance,
                    UserCodeMemberInfo.FromMemberInfo( f ) );

                if ( !this._userCodeInvoker.TryInvoke( () => f.GetValue( null ), executionContext, out var value ) )
                {
                    return null;
                }

                return (T?) value;
            }

            T? GetPropertyValue( PropertyInfo p )
            {
                var executionContext = new UserCodeExecutionContext(
                    this._serviceProvider,
                    NullDiagnosticAdder.Instance,
                    UserCodeMemberInfo.FromMemberInfo( p ) );

                if ( !this._userCodeInvoker.TryInvoke( () => p.GetValue( null ), executionContext, out var value ) )
                {
                    return null;
                }

                return (T?) value;
            }

            return declaringTypes.GetFields( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )
                .Where( f => typeof(T).IsAssignableFrom( f.FieldType ) )
                .Select( GetFieldValue )
                .Concat(
                    declaringTypes.GetProperties( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic )
                        .Where( p => typeof(T).IsAssignableFrom( p.PropertyType ) )
                        .Select( GetPropertyValue ) )
                .WhereNotNull();
        }
    }
}