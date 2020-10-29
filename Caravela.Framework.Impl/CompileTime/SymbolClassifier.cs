using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CompileTime
{
    class SymbolClassifier : ISymbolClassifier
    {
        private readonly CSharpCompilation _compilation;
        private readonly INamedTypeSymbol _compileTimeAttribute;
        private readonly INamedTypeSymbol _templateAttribute;
        private readonly Dictionary<ISymbol, SymbolDeclarationScope> _cache = new Dictionary<ISymbol, SymbolDeclarationScope>( SymbolEqualityComparer.Default );

        public SymbolClassifier( CSharpCompilation compilation )
        {
            this._compilation = compilation;
            this._compileTimeAttribute = this._compilation.GetTypeByMetadataName( typeof( CompileTimeAttribute ).FullName )!;
            this._templateAttribute = this._compilation.GetTypeByMetadataName( typeof( TemplateAttribute ).FullName )!;
        }

        protected virtual SymbolDeclarationScope GetAttributeScope(AttributeData attribute)
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

        protected virtual SymbolDeclarationScope GetAssemblyScope(IAssemblySymbol assembly)
        {
            if (assembly == null)
            {
                return SymbolDeclarationScope.Default;
            }
            
            // TODO: be more strict with .NET Standard.
            if (SymbolEqualityComparer.Default.Equals(assembly, this._compilation.Assembly))
            {
                return SymbolDeclarationScope.Default;
            }
            else if (assembly.Name.StartsWith("System") || assembly.Name == "netstandard")
            {
                return SymbolDeclarationScope.Default;
            }
            else
            {
                return SymbolDeclarationScope.RunTimeOnly;
            }
        }
        
        public SymbolDeclarationScope GetSymbolDeclarationScope(ISymbol symbol)
        {
            SymbolDeclarationScope AddToCache(SymbolDeclarationScope scope)
            {
                this._cache[symbol] = scope;
                return scope;
            }
            
            // From cache.
            if (this._cache.TryGetValue(symbol, out var scopeFromCache))
            {
                return scopeFromCache;
            }
            
            // From attributes.
            var scopeFromAttributes = symbol.GetAttributes().Select(this.GetAttributeScope).FirstOrDefault(s => s != SymbolDeclarationScope.Default);
            if (scopeFromAttributes != SymbolDeclarationScope.Default)
            {
                return AddToCache(scopeFromAttributes);
            }
            
            // From declaring type.
            if (symbol.ContainingType != null)
            {
                var scopeFromContainingType = this.GetSymbolDeclarationScope(symbol.ContainingType);
                
                if (scopeFromContainingType != SymbolDeclarationScope.Default )
                {
                    return AddToCache(scopeFromContainingType);
                }
            }
            
            // From base type.
            if (symbol is ITypeSymbol type )
            {
                if (type.Name == "dynamic")
                {
                    return SymbolDeclarationScope.RunTimeOnly;
                }
                
                if (type.BaseType != null)
                {
                    var scopeFromBaseType = this.GetSymbolDeclarationScope(type.BaseType);

                    if (scopeFromBaseType != SymbolDeclarationScope.Default)
                    {
                        return AddToCache(scopeFromBaseType);
                    }
                }

                foreach (var iface in type.AllInterfaces)
                {
                    var scopeFromInterface = this.GetSymbolDeclarationScope( iface );

                    if ( scopeFromInterface != SymbolDeclarationScope.Default )
                    {
                        return AddToCache( scopeFromInterface );
                    }
                }
            }
            
            // From assemblies.
            var scopeFromAssembly = this.GetAssemblyScope(symbol.ContainingAssembly);
            if (scopeFromAssembly != SymbolDeclarationScope.Default)
            {
                return AddToCache(scopeFromAssembly);
            }

            return AddToCache( SymbolDeclarationScope.Default );
        }
    }
}