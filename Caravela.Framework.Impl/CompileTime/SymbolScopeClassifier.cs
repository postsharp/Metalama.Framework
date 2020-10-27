using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace PostSharp.Caravela.AspectWorkbench
{
    public class SymbolScopeClassifier
    {
        private readonly IAssemblySymbol _assembly;
        private readonly Dictionary<ISymbol,SymbolScope> _cache = new Dictionary<ISymbol, SymbolScope>(SymbolEqualityComparer.Default);

        public SymbolScopeClassifier(IAssemblySymbol assembly)
        {
            this._assembly = assembly;
        }

        protected virtual SymbolScope GetAttributeScope(AttributeData attribute)
        {
            if (attribute.AttributeClass.Name == nameof(BuildTimeOnlyAttribute))
            {
                return SymbolScope.CompileTimeOnly;
            }
            else
            {
                return SymbolScope.Default;
            }
        }

        protected virtual SymbolScope GetAssemblyScope(IAssemblySymbol assembly)
        {
            if (assembly == null)
            {
                return SymbolScope.Default;
            }
            
            // TODO: be more strict with .NET Standard.
            if (SymbolEqualityComparer.Default.Equals(assembly, this._assembly))
            {
                return SymbolScope.Default;
            }
            else if (assembly.Name.StartsWith("System") || assembly.Name == "netstandard")
            {
                return SymbolScope.Default;
            }
            else
            {
                return SymbolScope.RunTimeOnly;
            }
        }
        
        public SymbolScope GetSymbolScope(ISymbol symbol)
        {
            SymbolScope AddToCache(SymbolScope scope)
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
            var scopeFromAttributes = symbol.GetAttributes().Select(this.GetAttributeScope).FirstOrDefault(s => s != SymbolScope.Default);
            if (scopeFromAttributes != SymbolScope.Default)
            {
                return AddToCache(scopeFromAttributes);
            }
            
            // From declaring type.
            if (symbol.ContainingType != null)
            {
                var scopeFromContainingType = this.GetSymbolScope(symbol.ContainingType);
                
                if (scopeFromContainingType != SymbolScope.Default )
                {
                    return AddToCache(scopeFromContainingType);
                }
            }
            
            // From base type.
            if (symbol is ITypeSymbol type )
            {
                if (type.Name == "dynamic")
                {
                    return SymbolScope.RunTimeOnly;
                }
                
                if (type.BaseType != null)
                {
                    var scopeFromBaseType = this.GetSymbolScope(type.BaseType);

                    if (scopeFromBaseType != SymbolScope.Default)
                    {
                        return AddToCache(scopeFromBaseType);
                    }
                }
            }
            
            // From assemblies.
            var scopeFromAssembly = this.GetAssemblyScope(symbol.ContainingAssembly);
            if (scopeFromAssembly != SymbolScope.Default)
            {
                return AddToCache(scopeFromAssembly);
            }

            return SymbolScope.Default;


        }
    }
}