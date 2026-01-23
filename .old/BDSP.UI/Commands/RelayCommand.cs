using System;
using System.Windows.Input;

namespace BDSP.UI.Commands;
public sealed class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;

    public RelayCommand(Action<T> execute)
    {
        _execute = execute;
    }

    public bool CanExecute(object? parameter)
        => parameter is T;

    public void Execute(object? parameter)
    {
        if (parameter is T value)
            _execute(value);
    }

    public event EventHandler? CanExecuteChanged;
}
