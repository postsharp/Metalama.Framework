using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CompileTime
{
    internal sealed class CompileTimeAssemblyLoader : IDisposable
    {
        private readonly CSharpCompilation _compilation;
        private readonly CompileTimeAssemblyBuilder _compileTimeAssemblyBuilder;

        private readonly Dictionary<IAssemblySymbol, Assembly> _assemblyMap = new ();
        private readonly Dictionary<string, byte[]?> _assemblyBytesMap = new ();

        public CompileTimeAssemblyLoader( CSharpCompilation compilation, CompileTimeAssemblyBuilder compileTimeAssemblyBuilder )
        {
            this._compilation = compilation;
            this._compileTimeAssemblyBuilder = compileTimeAssemblyBuilder;

            // TODO: this is probably not enough
            this._assemblyMap.Add( compilation.ObjectType.ContainingAssembly, typeof( object ).Assembly );

            AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomain_AssemblyResolve;
        }

        // https://stackoverflow.com/a/27106959/41071
        private static string GetFullMetadataName( ISymbol? s )
        {
            if ( s == null || IsRootNamespace( s ) )
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            var first = true;

            do
            {
                if ( !first )
                {
                    if ( s is ITypeSymbol )
                    {
                        sb.Insert( 0, '+' );
                    }
                    else if ( s is INamespaceSymbol )
                    {
                        sb.Insert( 0, '.' );
                    }
                }

                first = false;

                sb.Insert( 0, s.MetadataName );

                s = s.ContainingSymbol;
            }
            while ( !IsRootNamespace( s ) );

            return sb.ToString();
        }

        private static IEnumerable<ITypeSymbol> CollectTypeArguments( INamedTypeSymbol? s )
        {
            var typeArguments = new List<ITypeSymbol>();

            while ( s != null )
            {
                typeArguments.InsertRange( 0, s.TypeArguments );

                s = s.ContainingSymbol as INamedTypeSymbol;
            }

            return typeArguments;
        }

        private static bool IsRootNamespace( ISymbol symbol ) => symbol is INamespaceSymbol ns && ns.IsGlobalNamespace;

        private static Assembly Load( byte[] assembly )
        {
            // TODO: use AssemblyLoadContext on .Net Core? (requires multi-targetting)
            return Assembly.Load( assembly );
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= this.CurrentDomain_AssemblyResolve;
        }

        // TODO: perf
        private Assembly? CurrentDomain_AssemblyResolve( object sender, ResolveEventArgs args )
        {
            if ( !this._assemblyMap.ContainsValue( args.RequestingAssembly ) )
            {
                return null;
            }

            var reference = this._compilation.References.SingleOrDefault(
                r => r is PortableExecutableReference { FilePath: string path } && Path.GetFileNameWithoutExtension( path ) == new AssemblyName( args.Name ).Name );
            if ( reference == null )
            {
                return null;
            }

            if ( this._compilation.GetAssemblyOrModuleSymbol( reference ) is not IAssemblySymbol symbol )
            {
                return null;
            }

            return this.LoadCompileTimeAssembly( symbol );
        }

        public object CreateAttributeInstance( Code.IAttribute attribute )
        {
            // TODO: Exception handling and recovery should be better. Don't throw an exception but return false and emit a diagnostic.

            var constructorSymbol = attribute.Constructor.GetSymbol();
            var constructor = this.GetCompileTimeConstructor( constructorSymbol );

            if ( constructor == null )
            {
                throw new InvalidOperationException( $"Could not load type {constructorSymbol.ContainingType}." );
            }

            var parameters = attribute.ConstructorArguments.Select(
                ( a, i ) => this.TranslateAttributeArgument( a, constructor.GetParameters()[i].ParameterType ) ).ToArray();
            var result = constructor.Invoke( parameters );

            var type = constructor.DeclaringType!;

            foreach ( var (name, value) in attribute.NamedArguments )
            {
                PropertyInfo? property;
                FieldInfo? field;

                if ( (property = type.GetProperty( name )) != null )
                {
                    property.SetValue( result, this.TranslateAttributeArgument( value, property.PropertyType ) );
                }
                else if ( (field = type.GetField( name )) != null )
                {
                    field.SetValue( result, this.TranslateAttributeArgument( value, field.FieldType ) );
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Cannot find a field or property {name} in type {constructorSymbol.ContainingType.ToDisplayString()}" );
                }
            }

            return result;
        }

        private ConstructorInfo? GetCompileTimeConstructor( IMethodSymbol constructorSymbol )
        {
            var type = this.GetCompileTimeType( constructorSymbol.ContainingType );
            return type?.GetConstructors().Single( c => this.ParametersMatch( c.GetParameters(), constructorSymbol.Parameters ) );
        }

        private Type? GetCompileTimeType( ITypeSymbol typeSymbol )
        {
            if ( typeSymbol is IArrayTypeSymbol arrayType )
            {
                var elementType = this.GetCompileTimeType( arrayType.ElementType );

                if ( arrayType.IsSZArray )
                {
                    return elementType?.MakeArrayType();
                }

                return elementType?.MakeArrayType( arrayType.Rank );
            }

            var assemblySymbol = typeSymbol.ContainingAssembly;
            var assembly = this.LoadCompileTimeAssembly( assemblySymbol );
            var result = assembly.GetType( GetFullMetadataName( typeSymbol ) );

            if ( typeSymbol is INamedTypeSymbol { IsGenericType: true, IsUnboundGenericType: false } namedTypeSymbol )
            {
                var typeArguments = CollectTypeArguments( namedTypeSymbol );

                result = result.MakeGenericType( typeArguments.Select( this.GetCompileTimeType ).ToArray() );
            }

            return result;
        }

        private object? TranslateAttributeArgument( object? roslynArgument, Type targetType )
        {
            if ( roslynArgument == null )
            {
                return null;
            }

            switch ( roslynArgument )
            {
                case Code.IType type:
                    if ( !targetType.IsAssignableFrom( typeof( Type ) ) )
                    {
                        throw new InvalidOperationException( $"System.Type can't be assigned to {targetType}" );
                    }

                    var translatedType = this.GetCompileTimeType( type.GetSymbol() );
                    if ( translatedType == null )
                    {
                        throw new InvalidOperationException( $"Could not load type {type}." );
                    }

                    return translatedType;

                case IReadOnlyList<object?> list:
                    if ( !targetType.IsArray )
                    {
                        throw new InvalidOperationException( $"Array can't be assigned to {targetType}" );
                    }

                    var array = Array.CreateInstance( targetType.GetElementType()!, list.Count );

                    for ( var i = 0; i < list.Count; i++ )
                    {
                        array.SetValue( this.TranslateAttributeArgument( list[i], targetType.GetElementType()! ), i );
                    }

                    return array;

                default:
                    if ( targetType.IsEnum )
                    {
                        return Enum.ToObject( targetType, roslynArgument );
                    }

                    if ( roslynArgument != null && !targetType.IsInstanceOfType( roslynArgument ) )
                    {
                        throw new InvalidOperationException( $"{roslynArgument.GetType()} can't be assigned to {targetType}" );
                    }

                    return roslynArgument;
            }
        }

        private bool ParametersMatch( ParameterInfo[] reflectionParameters, ImmutableArray<IParameterSymbol> roslynParameters )
        {
            if ( reflectionParameters.Length != roslynParameters.Length )
            {
                return false;
            }

            for ( var i = 0; i < reflectionParameters.Length; i++ )
            {
                if ( reflectionParameters[i].ParameterType != this.GetCompileTimeType( roslynParameters[i].Type ) )
                {
                    return false;
                }
            }

            return true;
        }

        private byte[]? GetResourceBytes( string assemblyPath, string resourceName )
        {
            var resolver = new PathAssemblyResolver( new[] { typeof( object ).Assembly.Location } );
            using var mlc = new MetadataLoadContext( resolver, typeof( object ).Assembly.GetName().Name );

            // LoadFromAssemblyPath throws for mscorlib
            if ( Path.GetFileNameWithoutExtension( assemblyPath ) == typeof( object ).Assembly.GetName().Name )
            {
                return null;
            }

            var runtimeAssembly = mlc.LoadFromAssemblyPath( assemblyPath );

            if ( runtimeAssembly.GetManifestResourceNames().Contains( resourceName ) )
            {
                using var resourceStream = runtimeAssembly.GetManifestResourceStream( resourceName );

                if ( resourceName == null )
                {
                    throw new FileNotFoundException();
                }

                var memoryStream = new MemoryStream( (int) resourceStream!.Length );
                resourceStream.CopyTo( memoryStream );
                return memoryStream.ToArray();
            }
            else
            {
                return null;
            }
        }

        public byte[]? GetCompileTimeAssembly( string path )
        {
            if ( this._assemblyBytesMap.TryGetValue( path, out var assemblyBytes ) )
            {
                return assemblyBytes;
            }

            assemblyBytes = this.GetResourceBytes( path, this._compileTimeAssemblyBuilder.GetResourceName() );

            this._assemblyBytesMap.Add( path, assemblyBytes );
            return assemblyBytes;
        }

        private byte[] GetCompileTimeAssembly( IAssemblySymbol assemblySymbol )
        {
            MemoryStream? assemblyStream;

            if ( assemblySymbol is ISourceAssemblySymbol sourceAssemblySymbol )
            {
                assemblyStream = this._compileTimeAssemblyBuilder.EmitCompileTimeAssembly( sourceAssemblySymbol.Compilation );
            }
            else
            {
                if ( this._compilation.GetMetadataReference( assemblySymbol ) is not { } reference )
                {
                    throw new InvalidOperationException( $"Could not find reference for assembly {assemblySymbol}." );
                }

                if ( reference is CompilationReference compilationReference )
                {
                    // note: this should only happen in Try Caravela
                    var compilation = compilationReference.Compilation;
                    assemblyStream = new CompileTimeAssemblyBuilder( compilation ).EmitCompileTimeAssembly( compilation );
                }
                else
                {
                    if ( reference is not PortableExecutableReference peReference )
                    {
                        throw new InvalidOperationException( $"The assembly {assemblySymbol} does not correspond to a known kind of reference." );
                    }

                    if ( peReference.FilePath is not { } path )
                    {
                        throw new InvalidOperationException( $"Could not access path for the assembly {assemblySymbol}." );
                    }

                    if ( this.GetCompileTimeAssembly( path ) is not { } assemblyBytes )
                    {
                        throw new InvalidOperationException( $"Runtime assembly {assemblySymbol} does not contain a compile-time assembly resource." );
                    }

                    return assemblyBytes;
                }
            }

            if ( assemblyStream == null )
            {
                throw new InvalidOperationException( $"Could not create compile-time assembly for {assemblySymbol}." );
            }

            return assemblyStream.ToArray();
        }

        private Assembly LoadCompileTimeAssembly( IAssemblySymbol assemblySymbol )
        {
            if ( !this._assemblyMap.TryGetValue( assemblySymbol, out var assembly ) )
            {
                assembly = Load( this.GetCompileTimeAssembly( assemblySymbol ) );
                this._assemblyMap[assemblySymbol] = assembly;
            }

            return assembly;
        }
    }
}
