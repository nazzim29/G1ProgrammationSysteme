using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Helpers
{
    public interface Command
    {
        void Execute(Object parameter);
        bool CanExecute(Object parameter);
        event EventHandler CanExecuteChanged;
    }
}
