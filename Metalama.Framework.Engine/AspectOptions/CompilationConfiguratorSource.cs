// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Metalama.Framework.Engine.AspectOptions;

internal sealed class CompilationConfiguratorSource : IConfiguratorSource
{
    private readonly IUserCodeAttributeDeserializer _attributeDeserializer;

    private readonly ConcurrentDictionary<Type, Func<IAspectOptionsAttribute, IAspectOptions>> _toOptionsMethods = new();
    private readonly ProjectSpecificCompileTimeTypeResolver _typeResolver;

    public CompilationConfiguratorSource( ProjectServiceProvider serviceProvider )
    {
        this._attributeDeserializer = serviceProvider.GetRequiredService<IUserCodeAttributeDeserializer>();
        this._typeResolver = serviceProvider.GetRequiredService<ProjectSpecificCompileTimeTypeResolver>();
    }

    private Func<IAspectOptionsAttribute, IAspectOptions> GetToOptionsMethod( Type type ) => this._toOptionsMethods.GetOrAdd( type, GetToOptionsMethodCore );

    private static Func<IAspectOptionsAttribute, IAspectOptions> GetToOptionsMethodCore( Type type )
    {
        var parameter = Expression.Parameter( typeof(IAspectOptionsAttribute) );
        var interfaceType = typeof(IAspectOptionsAttribute<>).MakeGenericType( type );
        var cast = Expression.Convert( parameter, interfaceType );

        var methodCall = Expression.Call(
            cast,
            interfaceType.GetMethod( nameof(IAspectOptionsAttribute<IAspectOptions>.ToOptions) ).AssertNotNull() );

        return Expression.Lambda<Func<IAspectOptionsAttribute, IAspectOptions>>( methodCall, parameter ).Compile();
    }

    public IEnumerable<Configurator> GetConfigurators( CompilationModel compilation, IDiagnosticAdder diagnosticAdder )
    {
        var genericIAspectOptionsAttribute = compilation.Factory.GetTypeByReflectionType( typeof(IAspectOptionsAttribute<>) );

        foreach ( var attributeType in compilation.GetDerivedTypes(
                     (INamedType) compilation.Factory.GetTypeByReflectionType( typeof(IAspectOptionsAttribute) ) ) )
        {
            var optionTypes =
                attributeType.AllImplementedInterfaces.Where( i => i.TypeDefinition == genericIAspectOptionsAttribute )
                    .Select( x => (INamedType) x.TypeArguments[0] )
                    .Select( x => this._typeResolver.GetCompileTimeType( x.GetSymbol().AssertNotNull(), false ).AssertNotNull() )
                    .ToReadOnlyList();

            foreach ( var attribute in compilation.GetAllAttributesOfType( attributeType ) )
            {
                if ( !this._attributeDeserializer.TryCreateAttribute( attribute.GetAttributeData(), diagnosticAdder, out var deserializedAttribute ) )
                {
                    continue;
                }

                var optionsAttribute = (IAspectOptionsAttribute) deserializedAttribute;

                foreach ( var optionType in optionTypes )
                {
                    var options = this.GetToOptionsMethod( optionType ).Invoke( optionsAttribute );

                    yield return new Configurator( attribute.ContainingDeclaration, options );
                }
            }
        }
    }
}