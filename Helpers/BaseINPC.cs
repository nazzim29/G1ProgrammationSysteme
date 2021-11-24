using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySave.Helpers
{
    //We have created a BaseINPCclass (for “INotifyPropertyChanged”) which has a method to make it easier to trigger the property change event.
    public abstract class BaseINPC : INotifyPropertyChanged
    {
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // interface implemetation
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
