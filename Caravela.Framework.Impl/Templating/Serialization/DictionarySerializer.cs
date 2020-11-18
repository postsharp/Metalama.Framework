using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    public class DictionarySerializer : ObjectSerializer
    {
        private readonly ObjectSerializers _serializers;

        public DictionarySerializer( ObjectSerializers serializers )
        {
            this._serializers = serializers;
        }
        public override ExpressionSyntax Serialize( object o ) 
        {
            return null;
        }
    }
}