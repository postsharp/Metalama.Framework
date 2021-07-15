namespace Caravela.Framework.Impl.CompileTime
{
    internal class DefaultCompileTimeDomainFactory : ICompileTimeDomainFactory
    {
        public CompileTimeDomain GetDomain() => new CompileTimeDomain();
    }
}