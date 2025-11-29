using System.Windows.Controls;

using Xabbo.Scripter.ViewModel;

namespace Xabbo.Scripter.View.Pages;

public partial class TasksPage : Page
{
    public TasksPage(TasksViewManager manager)
    {
        DataContext = manager;
        InitializeComponent();
    }
}
