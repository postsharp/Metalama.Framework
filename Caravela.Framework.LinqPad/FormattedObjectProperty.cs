using System.Reflection;

namespace Caravela.Framework.LinqPad
{
    internal record FormattedObjectProperty( string PropertyName, MethodInfo Getter, bool IsLazy );
}