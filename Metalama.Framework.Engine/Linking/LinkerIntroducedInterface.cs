using Metalama.Framework.Engine.Aspects;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking;

internal record struct LinkerIntroducedInterface( AspectLayerId AspectLayerId, BaseTypeSyntax Syntax );