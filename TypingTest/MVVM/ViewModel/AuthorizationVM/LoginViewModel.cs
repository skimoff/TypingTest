using System.Windows.Input;
using TypingTest.Core;

namespace TypingTest.MVVM.ViewModel.AuthorizationVM;

public class LoginViewModel:ObservableObject
{
    public event Action RequestRegisterNavigation;
    public ICommand NavigateToRegisterCommand { get; }
    
    public LoginViewModel()
    {
        NavigateToRegisterCommand = new RelayCommand(o =>
        {
            RequestRegisterNavigation?.Invoke();
        });
    }
}