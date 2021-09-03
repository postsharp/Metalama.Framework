// Copyright (c) SharpCrafters s.r.o. All rights reserved.

using System.Xml.Linq;

namespace PostSharp.Engineering.BuildTools.Coverage
{
    public partial class WarnCommand
    {
        private class SequencePoint
        {
            public int StartLine { get; }

            public int EndLine { get; }

            public int FileId { get; }

            public int TotalBranchCount { get; }

            public int CoveredBranchCount { get; }

            public SequencePoint( XElement element )
            {
                // Coverlet does not provide any meaningful value for columns, so we don't read them.

                this.StartLine = int.Parse( element.Attribute( "sl" )!.Value ) - 1;
                this.EndLine = int.Parse( element.Attribute( "el" )!.Value ) - 1;
                this.FileId = int.Parse( element.Attribute( "fileid" )!.Value );
                this.TotalBranchCount = int.Parse( element.Attribute( "bec" )!.Value );
                this.CoveredBranchCount = int.Parse( element.Attribute( "bev" )!.Value );
            }
        }
    }
}