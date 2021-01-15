namespace Caravela.Framework.Code
{
    public interface IPointerType : IType
    {
        IType PointedAtType { get; }
    }
}