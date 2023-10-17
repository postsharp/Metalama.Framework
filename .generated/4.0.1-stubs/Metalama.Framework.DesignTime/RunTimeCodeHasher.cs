#pragma warning disable CS8669 // Nullability
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using K4os.Hash.xxHash;

namespace Metalama.Framework.DesignTime.Pipeline.Diff
{
	public class RunTimeCodeHasher : BaseCodeHasher
	{
		public RunTimeCodeHasher(XXH64 hasher) : base(hasher) {}
	}
}
