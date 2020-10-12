using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using PostSharp.Framework.Sdk;

namespace PostSharp.Framework.Impl
{
    class AspectDriverFactory
    {
        private Microsoft.CodeAnalysis.Compilation compilation;

        public AspectDriverFactory(Microsoft.CodeAnalysis.Compilation compilation)
        {
            this.compilation = compilation;
        }

        public IAspectDriver GetAspectDriver(INamedTypeSymbol type)
        {

        }
    }
}
