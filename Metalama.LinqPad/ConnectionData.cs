// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using LINQPad.Extensibility.DataContext;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Metalama.LinqPad
{
    /// <summary>
    /// Represents a connection, i.e. contains information about the loaded project or solution.
    /// </summary>
    internal sealed class ConnectionData : INotifyPropertyChanged
    {
        private string? _project;
        private string? _displayName;
        private bool _persist;

        public ConnectionData( IConnectionInfo connectionInfo )
        {
            if ( connectionInfo.DriverData != null )
            {
                this.Project = connectionInfo.DriverData.Element( "Project" )?.Value;
            }

            this.DisplayName = connectionInfo.DisplayName;
            this.Persist = connectionInfo.Persist;
        }

        public string? Project
        {
            get => this._project;
            set
            {
                this._project = value;
                this.OnPropertyChanged();
            }
        }

        public string DisplayName
        {
            get => string.IsNullOrWhiteSpace( this._displayName ) ? Path.GetFileName( this.Project )! : this._displayName;

            set
            {
                this._displayName = value;
                this.OnPropertyChanged();
            }
        }

        public bool Persist
        {
            get => this._persist;
            set
            {
                this._persist = value;
                this.OnPropertyChanged();
            }
        }

        public void Save( IConnectionInfo connectionInfo )
        {
            connectionInfo.DisplayName = this.DisplayName;
            connectionInfo.DriverData = new XElement( "MetalamaConnection", new XElement( "Project", this.Project ) );
            connectionInfo.Persist = this.Persist;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged( [CallerMemberName] string? propertyName = null )
        {
            this.PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}