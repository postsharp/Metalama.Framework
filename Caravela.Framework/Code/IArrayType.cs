namespace Caravela.Framework.Code
{
    public interface IArrayType : IType
    {
        IType ElementType { get; }
        int Rank { get; }
    }
}