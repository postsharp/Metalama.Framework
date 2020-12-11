using Caravela.Framework.Impl.CompileTime;
using System;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests
{
    public class ReferenceAssemblyLocatorTests
    {
        [Fact]
        public void CommandLineParserTest()
        {
            var lines = @"Microsoft (R) Build Engine version 16.8.0+126527ff1 for .NET
Copyright (C) Microsoft Corporation. All rights reserved.

  Determining projects to restore...
  Restored C:\code\tmp\hwapp\hwapp.csproj (in 427 ms).
  1 of 2 projects are up-to-date for restore.
  lib->C:\code\tmp\class lib\bin\Debug\netstandard2.0\lib.dll
C:\Program Files\dotnet\dotnet.exe exec ""C:\Program Files\dotnet\sdk\5.0.100\Roslyn\bincore\csc.dll"" /noconfig /unsafe- /checked- /nowarn:1701,1702,1701,1702 /fullpaths /nostdlib+ /errorreport:prompt /warn:5 /define:TRACE;DEBUG;NET;NET5_0;NETCOREAPP /highentropyva+ /reference:C:\Users\PetrOnderka\.nuget\packages\microsoft.codeanalysis.common\3.8.0\lib\netcoreapp3.1\Microsoft.CodeAnalysis.dll /reference:""C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\5.0.0\ref\net5.0\Microsoft.CSharp.dll"" /reference:""C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\5.0.0\ref\net5.0\netstandard.dll"" /debug+ /debug:portable /filealign:512 /optimize- /out:obj\Debug\net5.0\hwapp.dll /refout:obj\Debug\net5.0\ref\hwapp.dll /target:exe /warnaserror- /utf8output /deterministic+ /langversion:9 /embed /analyzerconfig:obj\Debug\net5.0\hwapp.GeneratedMSBuildEditorConfig.editorconfig /analyzerconfig:""C:\Program Files\dotnet\sdk\5.0.100\Sdks\Microsoft.NET.Sdk\analyzers\build\config\AnalysisLevel_5_Default.editorconfig"" /analyzer:""C:\Program Files\dotnet\sdk\5.0.100\Sdks\Microsoft.NET.Sdk\targets\..\analyzers\Microsoft.CodeAnalysis.CSharp.NetAnalyzers.dll"" /analyzer:""C:\Program Files\dotnet\sdk\5.0.100\Sdks\Microsoft.NET.Sdk\targets\..\analyzers\Microsoft.CodeAnalysis.NetAnalyzers.dll"" /analyzer:C:\Users\PetrOnderka\.nuget\packages\microsoft.codeanalysis.analyzers\3.0.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.Analyzers.dll /analyzer:C:\Users\PetrOnderka\.nuget\packages\microsoft.codeanalysis.analyzers\3.0.0\analyzers\dotnet\cs\Microsoft.CodeAnalysis.CSharp.Analyzers.dll Program.cs ""obj\Debug\net5.0\.NETCoreApp,Version=v5.0.AssemblyAttributes.cs"" obj\Debug\net5.0\hwapp.AssemblyInfo.cs /warnaserror+:NU1605
hwapp -> C:\code\tmp\hwapp\bin\Debug\net5.0\hwapp.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:04.09".Split( Environment.NewLine ).ToList();

            var actual = ReferenceAssemblyLocator.ReadReferencesFromBuildOutput( lines );

            var expected = new[]
            {
                @"C:\Users\PetrOnderka\.nuget\packages\microsoft.codeanalysis.common\3.8.0\lib\netcoreapp3.1\Microsoft.CodeAnalysis.dll",
                @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\5.0.0\ref\net5.0\Microsoft.CSharp.dll",
                @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\5.0.0\ref\net5.0\netstandard.dll"
            };

            Assert.Equal( expected, actual );
        }
    }
}
