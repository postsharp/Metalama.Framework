using Caravela.Framework.Code;

namespace Caravela.Framework.Sdk
{
    public static class CodeModelExtensions
    {
        public static ISdkCodeElement ToSdkCodeElement( this ICodeElement codeElement ) => (ISdkCodeElement) codeElement;

        public static ISdkType ToSdkType( this IType type ) => (ISdkType) type;
    }
}