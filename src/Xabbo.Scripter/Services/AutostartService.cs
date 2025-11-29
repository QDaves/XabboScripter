using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using GalaSoft.MvvmLight;

using Xabbo.Extension;

using Xabbo.Scripter.Configuration;
using Xabbo.Scripter.Engine;
using Xabbo.Scripter.ViewModel;

namespace Xabbo.Scripter.Services;

public class AutostartService : ObservableObject
{
    private readonly ScriptEngine _engine;
    private readonly IScriptHost _host;
    private readonly IUiContext _uiContext;
    private readonly AutostartConfig _config;

    private ScriptsViewManager? _scriptsManager;

    public ObservableCollection<AutostartTaskViewModel> Tasks { get; } = new();

    public AutostartService(
        ScriptEngine engine,
        IScriptHost host,
        IUiContext uiContext,
        IRemoteExtension extension)
    {
        _engine = engine;
        _host = host;
        _uiContext = uiContext;
        _config = AutostartConfig.Load();

        extension.Connected += OnConnected;
    }

    public void Initialize(ScriptsViewManager scriptsManager)
    {
        _scriptsManager = scriptsManager;
        _config.CleanupMissing(_engine.ScriptDirectory);
        RefreshTasks();
    }

    public void RefreshTasks()
    {
        Tasks.Clear();
        foreach (var entry in _config.Entries)
        {
            var script = FindScript(entry.FileName);
            Tasks.Add(new AutostartTaskViewModel(this, entry, script));
        }
    }

    private ScriptViewModel? FindScript(string fileName)
    {
        if (_scriptsManager == null) return null;

        foreach (ScriptViewModel script in _scriptsManager.Scripts.SourceCollection)
        {
            if (script.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                return script;
        }
        return null;
    }

    public bool IsAutostart(string fileName)
    {
        return _config.Contains(fileName);
    }

    public void SetAutostart(string fileName, bool enabled)
    {
        if (enabled)
        {
            _config.Add(fileName);
            var script = FindScript(fileName);
            Tasks.Add(new AutostartTaskViewModel(this, new AutostartEntry { FileName = fileName, AddedAt = DateTime.Now }, script));
        }
        else
        {
            _config.Remove(fileName);
            var task = Tasks.FirstOrDefault(t => t.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if (task != null) Tasks.Remove(task);
        }
    }

    public void RemoveTask(AutostartTaskViewModel task)
    {
        _config.Remove(task.FileName);
        Tasks.Remove(task);
        task.Script?.RaisePropertyChanged(nameof(ScriptViewModel.IsAutostart));
    }

    public void StopTask(AutostartTaskViewModel task)
    {
        task.Script?.CancelCommand.Execute(null);
    }

    public void RestartTask(AutostartTaskViewModel task)
    {
        if (task.Script == null) return;

        if (task.Script.IsWorking)
        {
            task.Script.CancelCommand.Execute(null);
        }

        Task.Delay(100).ContinueWith(_ =>
        {
            if (_host.CanExecute && task.Script.IsSavedToDisk)
            {
                task.Script.ExecuteCommand.Execute(null);
            }
        });
    }

    private async void OnConnected(object? sender, GameConnectedEventArgs e)
    {
        await Task.Delay(500);

        _config.CleanupMissing(_engine.ScriptDirectory);

        await _uiContext.InvokeAsync(() => RefreshTasks());

        foreach (var task in Tasks.ToList())
        {
            if (task.Script != null && task.Script.IsSavedToDisk && !task.Script.IsWorking)
            {
                if (!task.Script.IsLoaded)
                {
                    try
                    {
                        task.Script.Load();
                    }
                    catch { continue; }
                }

                task.Script.ExecuteCommand.Execute(null);
                await Task.Delay(100);
            }
        }
    }
}

public class AutostartTaskViewModel : ObservableObject
{
    private readonly AutostartService _service;

    public AutostartEntry Entry { get; }
    public ScriptViewModel? Script { get; private set; }

    public string FileName => Entry.FileName;
    public string Name => Script?.Name ?? System.IO.Path.GetFileNameWithoutExtension(FileName);
    public DateTime AddedAt => Entry.AddedAt;
    public bool IsValid => Script != null && Script.IsSavedToDisk;
    public bool IsRunning => Script?.IsRunning == true;

    public string Status
    {
        get
        {
            if (Script == null) return "file missing";
            if (Script.IsRunning) return "running...";
            if (Script.IsCompiling) return "compiling...";
            if (Script.Status == Scripting.ScriptStatus.None) return "waiting";
            return Script.StatusText;
        }
    }

    public AutostartTaskViewModel(AutostartService service, AutostartEntry entry, ScriptViewModel? script)
    {
        _service = service;
        Entry = entry;
        Script = script;

        if (script != null)
        {
            script.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ScriptViewModel.Status) ||
                    e.PropertyName == nameof(ScriptViewModel.StatusText) ||
                    e.PropertyName == nameof(ScriptViewModel.IsRunning) ||
                    e.PropertyName == nameof(ScriptViewModel.IsCompiling))
                {
                    RaisePropertyChanged(nameof(Status));
                    RaisePropertyChanged(nameof(IsRunning));
                }
            };
        }
    }

    public void Remove() => _service.RemoveTask(this);
    public void Stop() => _service.StopTask(this);
    public void Restart() => _service.RestartTask(this);
}
