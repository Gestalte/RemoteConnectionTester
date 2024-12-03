using Avalonia.Controls;
using Avalonia.Interactivity;
using Server.ViewModels;

namespace Server.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (DataContext is MainViewModel mainViewModel)
        {
            mainViewModel.ListBox = this.LB;

            var toplevel = TopLevel.GetTopLevel(this.LB);
            mainViewModel.Clipboard = toplevel?.Clipboard;
        }
    }
}
