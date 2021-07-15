namespace Caravela.Framework.Impl.CompileTime
{
    public interface ICompileTimeDomainFactory : IService
    {
        CompileTimeDomain GetDomain();
    }
}