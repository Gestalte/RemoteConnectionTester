using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Server.ViewModels;
using Server.Views;
using System;

namespace Server;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownRequested += Desktop_ShutdownRequested;

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CA2211 // Non-constant fields should not be visible
    /// <summary>
    /// The provided action will be invoked before the application exits. When
    /// the ShutdownRequested event fires.
    /// </summary>
    public static Action? ShutDownAction;
#pragma warning restore CA2211 // Non-constant fields should not be visible
#pragma warning restore IDE0079 // Remove unnecessary suppression

    private void Desktop_ShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        ShutDownAction?.Invoke();
    }
}
