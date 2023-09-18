using Metalama.Framework.Engine.HierarchicalOptions;
using Metalama.Framework.Options;

namespace Metalama.Framework.DesignTime.Pipeline;

internal record InheritableOptionsInstance( HierarchicalOptionsKey Key, IHierarchicalOptions Options );