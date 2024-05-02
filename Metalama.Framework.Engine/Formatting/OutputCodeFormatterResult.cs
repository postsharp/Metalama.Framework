// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Formatting;

public sealed record OutputCodeFormatterResult( Solution Solution, Dictionary<DocumentId, Document> Documents );