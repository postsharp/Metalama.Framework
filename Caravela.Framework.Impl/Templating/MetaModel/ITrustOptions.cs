namespace Caravela.Framework.Impl.Templating.MetaModel
{
    /// <summary>
    /// Exposes a property <see cref="IsTrusted"/> that determines whether the source code is trusted by the compiler.
    /// TryCaravela implements this and returns <c>false</c>.
    /// </summary>
    public interface ITrustOptions : IService
    {
        bool IsTrusted { get; }
    }

    internal class DefaultTrustOptions : ITrustOptions
    {
        public bool IsTrusted => true;
    }
}