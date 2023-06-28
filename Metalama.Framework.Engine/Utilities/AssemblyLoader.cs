// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Metalama.Framework.Engine.Utilities;

internal sealed class AssemblyLoader : IDisposable
{
    private static readonly PropertyInfo? _isCollectibleProperty = typeof( Assembly ).GetProperty( "IsCollectible" );

    private readonly Func<string, Assembly?> _resolveAssembly;
    private readonly Func<string, Assembly> _loadAssembly;
    private readonly Action _assemblyResolveUnsubscribe;

    public AssemblyLoader( Func<string, Assembly?> resolveAssembly )
    {
        this._resolveAssembly = resolveAssembly;

        // In most cases (devenv, Rider, OmniSharp), compiler assemblies (including System.Collections.Immutable) are loaded into the default AssemblyLoadContext
        // (or ALCs don't exist), which means loading everything using Assembly.LoadFile works (though it's also a memory leak).
        // But in RoslynCodeAnalysisService, compiler assemblies are loaded into a separate ALC, so we have to do something different.
        // We detect this by checking that DirectoryLoadContext is the current ALC and that its _compilerLoadContext is not Default.

        var alcType = Type.GetType( "System.Runtime.Loader.AssemblyLoadContext, System.Runtime.Loader" );
        var currentAlc = alcType?.GetMethod( "GetLoadContext" )!.Invoke( null, new object[] { typeof(AssemblyLoader).Assembly } );
        var defaultAlc = alcType?.GetProperty( "Default" )!.GetValue( null );

        if ( currentAlc != null )
        {
            // The check for DirectoryLoadContext is written like this, because it can be either Microsoft.CodeAnalysis.DefaultAnalyzerAssemblyLoader+DirectoryLoadContext
            // or Microsoft.CodeAnalysis.AnalyzerAssemblyLoader+DirectoryLoadContext, depending on Roslyn version.
            if ( currentAlc.GetType() is { Namespace: "Microsoft.CodeAnalysis", Name: "DirectoryLoadContext" } directoryLoadContextType )
            {
                var compilerAlc = directoryLoadContextType.GetField( "_compilerLoadContext", BindingFlags.Instance | BindingFlags.NonPublic )
                    ?.GetValue( currentAlc );

                if ( !ReferenceEquals( compilerAlc, defaultAlc ) )
                {
                    // Now we need to set up loading into the current ALC, which is DirectoryLoadContext.
                    // DirectoryLoadContext does not respond to its Resolving event, but it does delegate to its "parent" contexts.
                    // This means subscribing to the Resolving event of the Default ALC works, but only up to .Net 6.
                    // .Net 7 added a new error, which means that loading into an unloadable context (like DirectoryLoadContext) through a non-unloadable context (like Default)
                    // throws an exception.
                    // Which means this code will stop working as soon as RoslynCodeAnalysisService starts using .Net 7 or 8.

#if DEBUG
                    if ( Environment.Version.Major >= 7 )
                    {
                        throw new NotSupportedException( "This hack does not work on .Net 7 and newer." );
                    }
#endif

                    var loadMethod = alcType!.GetMethod( "LoadFromAssemblyPath" )!;
                    this._loadAssembly = (Func<string, Assembly>) Delegate.CreateDelegate( typeof(Func<string, Assembly>), currentAlc, loadMethod );

                    // Use expression trees to create a delegate for the AssemblyLoadContext.Resolving event, because it involves the AssemblyLoadContext type,
                    // which cannot be statically used here.
                    // Using delegate variance instead won't work, because that fails when combining delegates of different types.
                    LambdaExpression simplifiedAlcResolvingExpression = ( AssemblyName assemblyName ) => this._resolveAssembly( assemblyName.FullName );

                    var alcResolvingType = typeof(Func<,,>).MakeGenericType( alcType, typeof(AssemblyName), typeof(Assembly) );

                    var alcResolvingExpression = Expression.Lambda(
                        alcResolvingType,
                        simplifiedAlcResolvingExpression.Body,
                        Expression.Parameter( alcType ),
                        simplifiedAlcResolvingExpression.Parameters.Single() );

                    var alcResolvingDelegate = alcResolvingExpression.Compile();

                    var addResolvingMethod = alcType.GetMethod( "add_Resolving" )!;
                    addResolvingMethod.Invoke( defaultAlc, new object[] { alcResolvingDelegate } );

                    var removeResolvingMethod = alcType.GetMethod( "remove_Resolving" )!;
                    this._assemblyResolveUnsubscribe = () => removeResolvingMethod.Invoke( defaultAlc, new object[] { alcResolvingDelegate } );

                    return;
                }
            }
        }

        this._loadAssembly = Assembly.LoadFile;

        AppDomain.CurrentDomain.AssemblyResolve += this.OnAssemblyResolve;

        this._assemblyResolveUnsubscribe = () => AppDomain.CurrentDomain.AssemblyResolve -= this.OnAssemblyResolve;
    }

    private Assembly? OnAssemblyResolve( object? sender, ResolveEventArgs args ) => this._resolveAssembly( args.Name );

    public Assembly LoadAssembly( string assemblyPath ) => this._loadAssembly( assemblyPath );

    // .NET 5.0 has collectible assemblies, but collectible assemblies cannot be returned to AppDomain.AssemblyResolve.
    internal static bool IsCollectible( Assembly assembly ) => _isCollectibleProperty == null || (bool) _isCollectibleProperty.GetValue( assembly )!;

    public void Dispose() => this._assemblyResolveUnsubscribe();
}