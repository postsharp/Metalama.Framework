using System.Windows;

namespace Caravela.Patterns.Costura.WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += (sender, args) => { this.Close(); };
        }
    }
}
