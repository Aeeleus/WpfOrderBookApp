using System.Windows;
using WpfOrderBookApp.ViewModels;

namespace WpfOrderBookApp
{
    public partial class MainWindow : Window
    {
        private MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            _vm = new MainViewModel();
            DataContext = _vm;

            Loaded += async (s, e) => await _vm.StartAsync();
        }
    }
}