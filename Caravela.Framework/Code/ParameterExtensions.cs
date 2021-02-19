namespace Caravela.Framework.Code
{
    /// <summary>
    /// Provides extension methods for <see cref="IParameter"/>.
    /// </summary>
    public static class ParameterExtensions
    {
        public static bool IsByRef( this IParameter parameter ) => parameter.RefKind != RefKind.None;

        public static bool IsRef( this IParameter parameter ) => parameter.RefKind == RefKind.Ref;

        public static bool IsOut( this IParameter parameter ) => parameter.RefKind == RefKind.Out;
    }
}