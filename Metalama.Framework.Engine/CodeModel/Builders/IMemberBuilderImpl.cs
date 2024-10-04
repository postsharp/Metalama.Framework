using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Abstractions;

namespace Metalama.Framework.Engine.CodeModel.Builders;

internal interface IMemberBuilderImpl : IMemberBuilder, IMemberOrNamedTypeBuilderImpl, IMemberImpl;