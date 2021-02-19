namespace Caravela.Framework.Code
{
    /// <summary>
    /// Accessibility of types and members for instance <see cref="Private"/> or <see cref="Public"/>.
    /// </summary>
    public enum Accessibility
    {
        Private,
        ProtectedOrInternal,
        Protected,
        ProtectedAndInternal,
        Internal,
        Public
    }
}