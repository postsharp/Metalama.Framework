// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Immutable;
using Newtonsoft.Json.Serialization;
using StreamJsonRpc.Protocol;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace Metalama.Framework.DesignTime.Rpc;

/// <summary>
/// An implementation of <see cref="ISerializationBinder"/> that strips version numbers from non-Metalama assemblies. 
/// </summary>
internal sealed class JsonSerializationBinder : DefaultSerializationBinder
{
    private readonly ConcurrentDictionary<string, Assembly> _assemblies;
    private static readonly char[] _tokens = new[] { ',', ']' };

    public static JsonSerializationBinder Instance { get; } = new();

    private JsonSerializationBinder()
    {
        this._assemblies = new ConcurrentDictionary<string, Assembly>();

        void AddAssemblyOfType( Type t )
        {
            this._assemblies.TryAdd( t.Assembly.GetName().Name, t.Assembly );
        }

        AddAssemblyOfType( typeof(ProjectKey) );
        AddAssemblyOfType( typeof(SerializableDeclarationId) );
        AddAssemblyOfType( typeof(ImmutableArray<>) );
        AddAssemblyOfType( typeof(CommonErrorData) );

        void AddAssemblyWithSameVersionThanType( Type t, string assemblyName )
        {
            var newAssemblyName = new AssemblyName( t.Assembly.FullName.Replace( t.Assembly.GetName().Name, assemblyName ) );

            var assembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where( a => AssemblyName.ReferenceMatchesDefinition( newAssemblyName, a.GetName() ) )
                .OrderByDescending( a => a.GetName().Version )
                .FirstOrDefault();

            try
            {
                if ( assembly == null )
                {
                    assembly = Assembly.Load( newAssemblyName );
                }

                this._assemblies.TryAdd( assemblyName, assembly );
            }
            catch ( FileNotFoundException )
            {
                // This happens in tests for assemblies of the other version of Roslyn than the one the test project is compiled for.
            }
        }

        AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.DesignTime.4.4.0" );
        AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.DesignTime.4.0.1" );
        AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.DesignTime.VisualStudio.4.4.0" );
        AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.DesignTime.VisualStudio.4.0.1" );
        AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.Engine.4.4.0" );
        AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.Engine.4.0.1" );

        void AddSystemLibrary( string name )
        {
            this._assemblies.TryAdd( name, typeof(int).Assembly );
        }

        AddSystemLibrary( "System.Private.CoreLib" );
        AddSystemLibrary( "mscorlib" );
    }

    public override Type BindToType( string? assemblyName, string typeName )
    {
        if ( !this._assemblies.TryGetValue( assemblyName, out var assembly ) )
        {
            assembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where( a => a.GetName().Name == assemblyName )
                .OrderByDescending( a => a.GetName().Version )
                .FirstOrDefault();

            if ( assembly == null )
            {
                throw new InvalidOperationException( $"The assembly '{assemblyName}' is not yet loaded in the AppDomain." );
            }

            this._assemblies.TryAdd( assemblyName, assembly );
        }

        var type = assembly.GetType( typeName );

        return type;
    }

    internal static string QualifyAssemblies( string fullyQualifiedTypeName, Dictionary<string, string> assemblyQualifiedNames )
    {
        // This code is copied from Newtonsoft codebase and is adapted to remove assembly details for non-Metalama assemblies only.
        var builder = new StringBuilder();

        // loop through the type name and filter out qualified assembly details from nested type names
        var writingAssemblyName = false;
        var skippingAssemblyDetails = false;
        var followBrackets = false;

        for ( var i = 0; i < fullyQualifiedTypeName.Length; i++ )
        {
            var current = fullyQualifiedTypeName[i];

            switch ( current )
            {
                case '[':
                    writingAssemblyName = false;
                    skippingAssemblyDetails = false;
                    followBrackets = false;
                    builder.Append( current );

                    break;

                case ']':
                    writingAssemblyName = false;
                    skippingAssemblyDetails = false;
                    followBrackets = true;
                    builder.Append( current );

                    break;

                case ',':
                    if ( followBrackets )
                    {
                        builder.Append( current );
                    }
                    else if ( !writingAssemblyName )
                    {
                        writingAssemblyName = true;
                        builder.Append( current );
                        builder.Append( ' ' );

                        var nextToken = fullyQualifiedTypeName.IndexOfAny( _tokens, i + 1 );

                        var assemblyName = nextToken > 0
                            ? fullyQualifiedTypeName.Substring( i + 1, nextToken - i - 1 ).Trim()
                            : fullyQualifiedTypeName.Substring( i + 1 ).Trim();

                        if ( !assemblyQualifiedNames.TryGetValue( assemblyName, out var fullyQualifiedAssemblyName ) )
                        {
                            throw new InvalidOperationException( $"Assembly not known as serializable: '{assemblyName}'." );
                        }

                        builder.Append( fullyQualifiedAssemblyName );
                        skippingAssemblyDetails = true;
                    }

                    break;

                default:
                    followBrackets = false;

                    if ( !skippingAssemblyDetails )
                    {
                        builder.Append( current );
                    }

                    break;
            }
        }

        return builder.ToString();
    }

    internal static string RemoveAssemblyDetailsFromAssemblyName( string assemblyName )
    {
        var indexOfComma = assemblyName.IndexOf( ',' );

        if ( indexOfComma > 0 )
        {
            return assemblyName.Substring( 0, indexOfComma ).TrimStart();
        }
        else
        {
            return assemblyName;
        }
    }

    internal static string RemoveAssemblyDetailsFromTypeName( string fullyQualifiedTypeName )
    {
        // This code is copied from Newtonsoft codebase and is adapted to remove assembly details for non-Metalama assemblies only.
        var builder = new StringBuilder();

        // loop through the type name and filter out qualified assembly details from nested type names
        var writingAssemblyName = false;
        var writingVersionNeutralAssemblyName = false;
        var skippingAssemblyDetails = false;
        var followBrackets = false;

        foreach ( var current in fullyQualifiedTypeName )
        {
            switch ( current )
            {
                case '[':
                    writingAssemblyName = false;
                    skippingAssemblyDetails = false;
                    followBrackets = true;
                    builder.Append( current );

                    break;

                case ']':
                    writingAssemblyName = false;
                    skippingAssemblyDetails = false;
                    followBrackets = false;
                    builder.Append( current );

                    break;

                case ',':
                    if ( followBrackets )
                    {
                        builder.Append( current );
                    }
                    else if ( !writingAssemblyName )
                    {
                        writingAssemblyName = true;
                        writingVersionNeutralAssemblyName = true;
                        builder.Append( current );
                    }
                    else if ( writingVersionNeutralAssemblyName )
                    {
                        skippingAssemblyDetails = true;
                    }
                    else
                    {
                        builder.Append( current );
                    }

                    break;

                default:
                    followBrackets = false;

                    if ( !skippingAssemblyDetails )
                    {
                        builder.Append( current );
                    }

                    break;
            }
        }

        return builder.ToString();
    }
}