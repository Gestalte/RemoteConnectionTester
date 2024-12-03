using Avalonia.Controls;
using RemoteConnectionTester.ViewModels;

namespace RemoteConnectionTester.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(ViewModelBase viewModelBase)
    {
        DataContext = viewModelBase;
        InitializeComponent();
    }
}
