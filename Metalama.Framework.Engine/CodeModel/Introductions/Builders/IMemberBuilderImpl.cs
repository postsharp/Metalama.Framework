using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.CodeModel.Abstractions;

namespace Metalama.Framework.Engine.CodeModel.Introductions.Builders;

internal interface IMemberBuilderImpl : IMemberBuilder, IMemberOrNamedTypeBuilderImpl, IMemberImpl;