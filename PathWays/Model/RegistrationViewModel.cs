using System.ComponentModel;
using System.Runtime.CompilerServices;
using PathWays.Model;
using Microsoft.Maui.Controls;

public class RegistrationViewModel : INotifyPropertyChanged
{
    private Person _person;

    public RegistrationViewModel()
    {
        _person = new Person();
    }

    public Person Person
    {
        get => _person;
        set
        {
            if (_person != value)
            {
                _person = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
