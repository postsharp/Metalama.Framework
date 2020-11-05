using Caravela.Framework.Project;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Aspects
{
    [CompileTime]
    public static class TemplateContext
    {
        [field: ThreadStatic]
        internal static object? ProceedImpl { get; set; }

        private static InvalidOperationException NewInvalidOperationException() =>
            new InvalidOperationException( "Code calling this method has to be compiled using Caravela." );

#pragma warning disable IDE1006 // Naming Styles
        [field: ThreadStatic] [AllowNull]
        public static ITemplateContext target { get; internal set; }

        [Proceed]
        public static dynamic proceed() => ProceedImpl ?? throw NewInvalidOperationException();
#pragma warning restore IDE1006 // Naming Styles
    }
}
