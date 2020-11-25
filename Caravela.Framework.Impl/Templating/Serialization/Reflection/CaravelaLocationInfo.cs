using Caravela.Framework.Impl.CodeModel;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal class CaravelaLocationInfo : LocationInfo
    {
        public Property Property { get; }

        public CaravelaLocationInfo(Property property) : base((PropertyInfo)null)
        {
            this.Property = property;
        }
    }
}