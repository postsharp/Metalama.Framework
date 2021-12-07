// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using LINQPad.Extensibility.DataContext;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace Metalama.LinqPad
{
    /// <summary>
    /// Interaction logic for ConnectionDialog.xaml.
    /// </summary>
    public partial class ConnectionDialog
    {
        private readonly IConnectionInfo _connectionInfo;
        private readonly ConnectionData _connectionData;

        public ConnectionDialog( IConnectionInfo cxInfo )
        {
            this._connectionInfo = cxInfo;
            this._connectionData = new ConnectionData( cxInfo );
            this.DataContext = this._connectionData;
            this.InitializeComponent();
        }

        private void OnOkButtonClick( object sender, RoutedEventArgs e )
        {
            this._connectionData.Save( this._connectionInfo );
            this.DialogResult = true;
        }

        private void BrowseProject( object sender, RoutedEventArgs e )
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Choose project or solution",
                DefaultExt = ".csproj",
                CheckFileExists = true,
                Filter = "Project files (*.csproj, *.sln)|*.csproj;*.sln"
            };

            if ( dialog.ShowDialog() == true )
            {
                this._connectionInfo.DisplayName = Path.GetFileName( dialog.FileName );
                this._connectionData.Project = dialog.FileName;
            }
        }
    }
}