using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Linking;

internal record struct LinkerIntroducedInterface( ITransformation Transformation, BaseTypeSyntax Syntax );