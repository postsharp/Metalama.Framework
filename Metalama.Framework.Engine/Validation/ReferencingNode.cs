// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Validation;

internal record struct ReferencingNode( SyntaxNodeOrToken Syntax, ReferenceKinds ReferenceKind );