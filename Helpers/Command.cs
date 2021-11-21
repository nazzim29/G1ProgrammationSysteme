using System;

namespace EasySave.Helpers
{
    public interface Command
    {
        void Execute(Object parameter);
        bool CanExecute(Object parameter);
        event EventHandler CanExecuteChanged;
    }
}
