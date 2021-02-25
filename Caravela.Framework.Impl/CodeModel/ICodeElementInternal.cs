using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl.CodeModel
{
    internal interface ICodeElementInternal : ISdkCodeElement
    {
        CodeElementLink<ICodeElement> ToLink();
    }
}