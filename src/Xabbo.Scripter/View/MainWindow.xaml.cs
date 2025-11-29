using System;
using System.Windows;
using System.Windows.Controls;

using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

using Xabbo.Scripter.ViewModel;

namespace Xabbo.Scripter.View;

public partial class MainWindow : UiWindow, INavigationWindow
{
    private readonly INavigationService _nav;
    private bool _firstActivation = true;

    public MainWindow(MainViewManager manager,
        INavigationService nav,
        IPageService pageService)
    {
        _nav = nav;
        DataContext = manager;

        InitializeComponent();

        _nav.SetNavigationControl(RootNavigation);
        SetPageService(pageService);

        Activated += MainWindow_Activated;

        RootFrame.Navigating += RootFrame_Navigating;
    }

    private void RootFrame_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
    {
        if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Refresh)
            e.Cancel = true;
    }

    private void ButtonPin_Click(object sender, RoutedEventArgs e) => Topmost = !Topmost;

    private void MainWindow_Activated(object? sender, EventArgs e)
    {
        if (!_firstActivation) return;
        _firstActivation = false;

        Navigate(typeof(Pages.LogPage));

        Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Light, updateAccent: false, forceBackground: true);

        Wpf.Ui.Appearance.Theme.Apply(Wpf.Ui.Appearance.ThemeType.Dark, updateAccent: false, forceBackground: true);
    }


    #region - INavigationWindow -
    public void CloseWindow() => Close();

    public Frame GetFrame() => RootFrame;

    public INavigation GetNavigation() => RootNavigation;

    public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

    public void SetPageService(IPageService pageService) => RootNavigation.PageService = pageService;

    public void ShowWindow() => Show();
    #endregion
}
