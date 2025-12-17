using TypingTest.Core;

namespace TypingTest.MVVM.ViewModel.AuthorizationVM;

public class AuthorizationViewModel:ObservableObject
{
    private LoginViewModel LoginVm { get; set; }
    private RegistrationViewModel RegistrationVm { get; set; }
    
    private object _currentView;

    public object CurrentAutView
    {
        get { return _currentView; }
        set
        {
            _currentView = value;
            OnPropertyChanged();
        }
    }

    public AuthorizationViewModel()
    {
        LoginVm = new LoginViewModel();
        RegistrationVm = new RegistrationViewModel();
        
        RegistrationVm.RequestLoginNavigation += NavigateToLogin;
        LoginVm.RequestRegisterNavigation += NavigateToRegister;
        
        CurrentAutView = RegistrationVm;
    }

    private void NavigateToLogin()
    {
        CurrentAutView = LoginVm;
    }
    
    private void NavigateToRegister()
    {
        CurrentAutView = RegistrationVm;
    }
    
}