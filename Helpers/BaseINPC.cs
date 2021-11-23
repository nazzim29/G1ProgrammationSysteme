using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySave.Helpers
{
    public abstract class BaseINPC : INotifyPropertyChanged
    {
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // interface implemetation
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
