using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    public class NullableSerializer : ObjectSerializer
    {
        private readonly ObjectSerializers _serializers;

        public NullableSerializer( ObjectSerializers serializers )
        {
            this._serializers = serializers;
        }
        public override ExpressionSyntax Serialize( object o )
        {
            return null;
        }
    }
}