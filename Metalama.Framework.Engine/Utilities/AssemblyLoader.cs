// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Metalama.Framework.Engine.Utilities;

internal sealed class AssemblyLoader : IDisposable
{
    private readonly Func<string, Assembly?> _resolveAssembly;
    private readonly Func<string, Assembly> _loadAssembly;
    private readonly Action _assemblyResolveUnsubscribe;

    public AssemblyLoader( Func<string, Assembly?> resolveAssembly )
    {
        this._resolveAssembly = resolveAssembly;

        var alcType = Type.GetType( "System.Runtime.Loader.AssemblyLoadContext, System.Runtime.Loader" );
        var currentAlc = alcType?.GetMethod( "GetLoadContext" )!.Invoke( null, new object[] { typeof(AssemblyLoader).Assembly } );
        var defaultAlc = alcType?.GetProperty( "Default" )!.GetValue( null );

        if ( currentAlc != null )
        {
            // When we're in RoslynCodeAnalysisService, we need to load extra assemblies using the same ALC as the one that's used by Roslyn for loading this assembly.
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

            // Within Roslyn, the ALC used is DirectoryLoadContext, which does not respond to its Resolving event.
            // Subscribing to the event of the default ALC instead works.
            // In Rider, DirectoryLoadContext is not used, and we have to subscibe to the Resolving event of the current ALC.
            var resolvingAlc = currentAlc.GetType().FullName == "Microsoft.CodeAnalysis.DefaultAnalyzerAssemblyLoader+DirectoryLoadContext" ? defaultAlc : currentAlc;

            var addResolvingMethod = alcType.GetMethod( "add_Resolving" )!;
            addResolvingMethod.Invoke( resolvingAlc, new object[] { alcResolvingDelegate } );

            var removeResolvingMethod = alcType.GetMethod( "remove_Resolving" )!;
            this._assemblyResolveUnsubscribe = () => removeResolvingMethod.Invoke( resolvingAlc, new object[] { alcResolvingDelegate } );
        }
        else
        {
            // On .Net Framework, use the regular Assembly.LoadFile, since ALC does not exist.
            this._loadAssembly = Assembly.LoadFile;

            AppDomain.CurrentDomain.AssemblyResolve += this.OnAssemblyResolve;

            this._assemblyResolveUnsubscribe = () => AppDomain.CurrentDomain.AssemblyResolve -= this.OnAssemblyResolve;
        }
    }

    private Assembly? OnAssemblyResolve( object? sender, ResolveEventArgs args ) => this._resolveAssembly( args.Name );

    public Assembly LoadAssembly( string assemblyPath ) => this._loadAssembly( assemblyPath );

    public void Dispose() => this._assemblyResolveUnsubscribe();
}