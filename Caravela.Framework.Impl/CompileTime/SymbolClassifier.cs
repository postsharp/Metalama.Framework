using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    internal class SymbolClassifier : ISymbolClassifier
    {
        private readonly Compilation _compilation;
        private readonly INamedTypeSymbol _compileTimeAttribute;
        private readonly INamedTypeSymbol _templateAttribute;
        private readonly Dictionary<ISymbol, SymbolDeclarationScope> _cache = new Dictionary<ISymbol, SymbolDeclarationScope>( SymbolEqualityComparer.Default );

        public SymbolClassifier( Compilation compilation )
        {
            this._compilation = compilation;
            this._compileTimeAttribute = this._compilation.GetTypeByMetadataName( typeof( CompileTimeAttribute ).FullName ).AssertNotNull();
            this._templateAttribute = this._compilation.GetTypeByMetadataName( typeof( TemplateAttribute ).FullName ).AssertNotNull();
        }

        protected virtual SymbolDeclarationScope GetAttributeScope( AttributeData attribute )
        {
            if ( this._compilation.HasImplicitConversion( attribute.AttributeClass, this._compileTimeAttribute ) )
            {
                return SymbolDeclarationScope.CompileTimeOnly;
            }
            else if ( this._compilation.HasImplicitConversion( attribute.AttributeClass, this._templateAttribute ) )
            {
                return SymbolDeclarationScope.Template;
            }
            else
            {
                return SymbolDeclarationScope.Default;
            }
        }

        protected virtual SymbolDeclarationScope GetAssemblyScope( IAssemblySymbol? assembly )
        {
            if ( assembly == null )
            {
                return SymbolDeclarationScope.Default;
            }

            // TODO: be more strict with .NET Standard.
            if ( assembly.Name.StartsWith( "System" ) || assembly.Name == "netstandard" || assembly.Name == "Caravela.Reactive" )
            {
                return SymbolDeclarationScope.Default;
            }

            var scopeFromAttributes = assembly.GetAttributes().Concat( assembly.Modules.First().GetAttributes() )
                .Select( this.GetAttributeScope ).FirstOrDefault( s => s != SymbolDeclarationScope.Default );
            if ( scopeFromAttributes != SymbolDeclarationScope.Default )
            {
                return scopeFromAttributes;
            }

            if ( SymbolEqualityComparer.Default.Equals( assembly, this._compilation.Assembly ) )
            {
                return SymbolDeclarationScope.Default;
            }

            return SymbolDeclarationScope.RunTimeOnly;
        }

        public SymbolDeclarationScope GetSymbolDeclarationScope( ISymbol symbol )
        {
            SymbolDeclarationScope AddToCache( SymbolDeclarationScope scope )
            {
                this._cache[symbol] = scope;
                return scope;
            }

            // From cache.
            if ( this._cache.TryGetValue( symbol, out var scopeFromCache ) )
            {
                return scopeFromCache;
            }

            // Add the symbol being processed to the cache temporarily to avoid an infinite recursion.
            AddToCache( SymbolDeclarationScope.Default );

            // From attributes.
            var scopeFromAttributes = symbol.GetAttributes().Select( this.GetAttributeScope ).FirstOrDefault( s => s != SymbolDeclarationScope.Default );
            if ( scopeFromAttributes != SymbolDeclarationScope.Default )
            {
                return AddToCache( scopeFromAttributes );
            }

            // From overridden method.
            if ( symbol is IMethodSymbol { OverriddenMethod: { } overriddenMethod } )
            {
                var scopeFromOverriddenMethod = this.GetSymbolDeclarationScope( overriddenMethod );
                if ( scopeFromOverriddenMethod != SymbolDeclarationScope.Default )
                {
                    return AddToCache( scopeFromOverriddenMethod );
                }
            }

            // From declaring type.
            if ( symbol.ContainingType != null )
            {
                var scopeFromContainingType = this.GetSymbolDeclarationScope( symbol.ContainingType );

                if ( scopeFromContainingType != SymbolDeclarationScope.Default )
                {
                    return AddToCache( scopeFromContainingType );
                }
            }

            switch ( symbol )
            {
                case ITypeSymbol type when type.Name == "dynamic":
                    return AddToCache( SymbolDeclarationScope.RunTimeOnly );

                case ITypeSymbol type:
                    {
                        if ( symbol is INamedTypeSymbol namedType )
                        {
                            // Note: Type with [CompileTime] on a base type or an interface should be considered compile-time,
                            // even if it has a generic argument from an external assembly (which makes it run-time). So generic arguments should come last.

                            // From base type.
                            if ( type.BaseType != null )
                            {
                                var scopeFromBaseType = this.GetSymbolDeclarationScope( type.BaseType );

                                if ( scopeFromBaseType != SymbolDeclarationScope.Default )
                                {
                                    return AddToCache( scopeFromBaseType );
                                }
                            }

                            // From interfaces.
                            foreach ( var iface in type.AllInterfaces )
                            {
                                var scopeFromInterface = this.GetSymbolDeclarationScope( iface );

                                if ( scopeFromInterface != SymbolDeclarationScope.Default )
                                {
                                    return AddToCache( scopeFromInterface );
                                }
                            }

                            // From generic arguments.
                            foreach ( var genericArgument in namedType.TypeArguments )
                            {
                                var scopeFromGenericArgument = this.GetSymbolDeclarationScope( genericArgument );

                                if ( scopeFromGenericArgument != SymbolDeclarationScope.Default )
                                {
                                    return AddToCache( scopeFromGenericArgument );
                                }
                            }
                        }

                        break;
                    }

                case INamespaceSymbol namespaceSymbol:
                    // Namespace can be either runtime, buildtime or both. We don't do more now but we may have TODO it based on assemblies defining the namespace.
                    return AddToCache( SymbolDeclarationScope.Default );
                    break;
            }

            var scopeFromAssembly = this.GetAssemblyScope( symbol.ContainingAssembly );
            if ( scopeFromAssembly != SymbolDeclarationScope.Default )
            {
                return AddToCache( scopeFromAssembly );
            }

            return AddToCache( SymbolDeclarationScope.Default );
        }
    }
}