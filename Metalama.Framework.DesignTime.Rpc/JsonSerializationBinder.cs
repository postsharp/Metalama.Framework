// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Immutable;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace Metalama.Framework.DesignTime.Rpc;

/// <summary>
/// An implementation of <see cref="ISerializationBinder"/> that strips version numbers from non-Metalama assemblies. 
/// </summary>
internal class JsonSerializationBinder : DefaultSerializationBinder
{
    private static readonly Dictionary<string, string> _assemblyQualifiedNames;
    private static readonly char[] _tokens = new[] { ',', ']' };

    static JsonSerializationBinder()
    {
        _assemblyQualifiedNames = new Dictionary<string, string>();

        void AddAssemblyOfType( Type t )
        {
            _assemblyQualifiedNames.Add( t.Assembly.GetName().Name, t.Assembly.FullName );
        }

        AddAssemblyOfType( typeof(ProjectKey) );
        AddAssemblyOfType( typeof(SerializableDeclarationId) );
        AddAssemblyOfType( typeof(ImmutableArray<>) );

        void AddAssemblyWithSameVersionThanType( Type t, string assemblyName )
        {
            _assemblyQualifiedNames.Add( assemblyName, t.Assembly.FullName.Replace( t.Assembly.GetName().Name, assemblyName ) );
        }

        AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.DesignTime.4.0.1" );
        AddAssemblyWithSameVersionThanType( typeof(ProjectKey), "Metalama.Framework.DesignTime.VisualStudio.4.0.1" );

        void AddSystemLibrary( string name )
        {
            _assemblyQualifiedNames.Add( name, typeof(int).Assembly.FullName );
        }

        AddSystemLibrary( "System.Private.CoreLib" );
        AddSystemLibrary( "mscorlib" );
    }

    public override Type BindToType( string? assemblyName, string typeName )
    {
        if ( !_assemblyQualifiedNames.TryGetValue( assemblyName, out var fullAssemblyName ) )
        {
            // We assume the assembly name is already fully qualified.
            fullAssemblyName = assemblyName;
        }

        var fullTypeName = QualifyAssemblies( typeName, _assemblyQualifiedNames ) + ", " + fullAssemblyName;

        return Type.GetType( fullTypeName, true )!;
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