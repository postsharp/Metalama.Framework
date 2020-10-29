using System;

namespace Caravela.Framework.Aspects
{
    public static class TemplateContext
    {
        private static InvalidOperationException NewInvalidOperationException() =>
            new InvalidOperationException( "Code calling this method has to be compiled using Caravela." );

#pragma warning disable IDE1006 // Naming Styles
        public static dynamic proceed() => throw NewInvalidOperationException();
#pragma warning restore IDE1006 // Naming Styles
    }
}
