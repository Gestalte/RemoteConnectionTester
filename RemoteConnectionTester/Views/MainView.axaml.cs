using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using RemoteConnectionTester.ViewModels;

namespace RemoteConnectionTester.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    public MainView(MainViewModel vm)
    {
        DataContext = vm;
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if(DataContext is MainViewModel mainViewModel)
        {
            mainViewModel.ListBox = this.LB;
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            var insetsManager = TopLevel.GetTopLevel(this)?.InsetsManager;

            if (insetsManager is not null)
            {
                insetsManager.DisplayEdgeToEdge = false;
                insetsManager.IsSystemBarVisible = true;
                insetsManager.SystemBarColor = Color.FromRgb(0, 0, 0);
            }
        });
    }
}