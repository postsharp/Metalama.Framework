using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Caravela.AspectWorkbench.ViewModels;
using PostSharp;

namespace Caravela.AspectWorkbench.Views
{
    public partial class MainWindow
    {
        private readonly MainViewModel viewModel;

        public MainWindow()
        {
            this.InitializeComponent();
            
            var newViewModel = new MainViewModel();
            this.viewModel = newViewModel;
            this.DataContext = newViewModel;
            Post.Cast<MainViewModel, INotifyPropertyChanged>( newViewModel ).PropertyChanged += this.ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            // TODO RichTextBox doesn't support data binding out of the box.
            switch ( e.PropertyName )
            {
                case nameof( MainViewModel.ColoredTemplateDocument ):
                    this.highlightedSourceRichBox.Document = this.viewModel.ColoredTemplateDocument ?? new FlowDocument();
                    break;
                case nameof( MainViewModel.CompiledTemplateDocument ):
                    this.compiledTemplateRichBox.Document = this.viewModel.CompiledTemplateDocument ?? new FlowDocument();
                    break;
                case nameof( MainViewModel.TransformedTargetDocument ):
                    this.transformedCodeRichBox.Document = this.viewModel.TransformedTargetDocument ?? new FlowDocument();
                    break;
            }
        }

        private void UpdateViewModel()
        {
            // Alternatively set the UpdateSourceTrigger property of the TextBox binding to PropertyChanged.
            this.sourceTextBox.GetBindingExpression( TextBox.TextProperty ).UpdateSource();
            this.expectedOutputTextBox.GetBindingExpression( TextBox.TextProperty ).UpdateSource();
            this.targetSourceTextBox.GetBindingExpression( TextBox.TextProperty ).UpdateSource();
        }

        private void NewButton_Click( object sender, RoutedEventArgs e )
        {
            this.viewModel.NewTest();
        }

        private async void OpenButton_Click( object sender, RoutedEventArgs e )
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".cs";
            dlg.Filter = "C# Files (*.cs)|*.cs";
            dlg.InitialDirectory = @"c:\src\Caravela\Tests\Caravela.Templating.UnitTests\";

            if ( dlg.ShowDialog() == true )
            {
                await this.viewModel.LoadTestAsync( dlg.FileName );
            }
        }

        private async void SaveButton_Click( object sender, RoutedEventArgs e )
        {
            this.UpdateViewModel();
            await this.viewModel.SaveTestAsync( null );
        }

        private async void RunButton_Click( object sender, RoutedEventArgs e )
        {
            this.UpdateViewModel();
            await this.viewModel.RunTestAsync();
        }
    }
}