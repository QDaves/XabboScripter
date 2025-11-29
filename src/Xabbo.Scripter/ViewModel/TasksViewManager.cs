using System.Collections.ObjectModel;
using System.Windows.Input;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Xabbo.Scripter.Services;

namespace Xabbo.Scripter.ViewModel;

public class TasksViewManager : ObservableObject
{
    private readonly AutostartService _autostartService;

    public ObservableCollection<AutostartTaskViewModel> Tasks => _autostartService.Tasks;

    public ICommand RemoveCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand RestartCommand { get; }

    private AutostartTaskViewModel? _selectedTask;
    public AutostartTaskViewModel? SelectedTask
    {
        get => _selectedTask;
        set => Set(ref _selectedTask, value);
    }

    public TasksViewManager(AutostartService autostartService)
    {
        _autostartService = autostartService;

        RemoveCommand = new RelayCommand<AutostartTaskViewModel>(t => t?.Remove());
        StopCommand = new RelayCommand<AutostartTaskViewModel>(t => t?.Stop());
        RestartCommand = new RelayCommand<AutostartTaskViewModel>(t => t?.Restart());
    }
}
