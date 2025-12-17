using System.Windows.Input;
using TypingTest.Core;

namespace TypingTest.MVVM.ViewModel.AuthorizationVM;

public class RegistrationViewModel:ObservableObject
{
    public event Action RequestLoginNavigation;
    
    public ICommand NavigateToLoginCommand { get; }

    public RegistrationViewModel()
    {
        NavigateToLoginCommand = new RelayCommand(o =>
        {
            RequestLoginNavigation?.Invoke();
        });
    }
}