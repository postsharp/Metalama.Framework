#pragma warning disable CS8669 // Nullability
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.Templating;

internal enum RoslynApiVersion 
{
	V4_0_1 = 0,
	V4_4_0 = 1,
	V4_8_0 = 2,
	Current = V4_4_0,
	Lowest = V4_0_1,
	Highest = V4_8_0
}
