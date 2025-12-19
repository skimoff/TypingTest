using System.Windows.Input;
using TypingTest.Core;

namespace TypingTest.MVVM.ViewModel.MainWindowVM;

public class MainViewModel:ObservableObject
{
    public RelayCommand ProfileViewCommand { get; set; }
    public RelayCommand SettingViewCommand { get; set; }
    public RelayCommand TestViewCommand { get; set; }
    public RelayCommand GuideViewCommand { get; set; }

    private ProfileViewModel ProfileVm { get; set; }
    private SettingViewModel SettingVm { get; set; }
    private TestViewModel TestVm { get; set; }
    private GuideViewModel GuideVm { get; set; }
    
    private object _currentView;
    
    private double _blurRadius;
    private bool _isRegistrationVisible;
    private string _newUserName;

    public object CurrentView
    {
        get{return _currentView;}
        set
        {
            _currentView = value;
            OnPropertyChanged();
        }
    }

    public MainViewModel()
    {
        ProfileVm = new ProfileViewModel();
        SettingVm = new SettingViewModel();
        TestVm = new TestViewModel();
        GuideVm = new GuideViewModel();
        
        SettingsManager.Load(); 
        
        if (string.IsNullOrEmpty(SettingsManager.UserName))
        {
            BlurRadius = 15; 
            IsRegistrationVisible = true; 
        }
        else
        {
            BlurRadius = 0;
            IsRegistrationVisible = false;
            ProfileVm.LoadData();
        }

        SaveNameCommand = new RelayCommand(o =>
        {
            if (!string.IsNullOrWhiteSpace(NewUserName))
            {
                SettingsManager.UserName = NewUserName;
                SettingsManager.Save();

                ProfileVm.LoadData();

                BlurRadius = 0;
                IsRegistrationVisible = false;
            }
        });

        CurrentView = ProfileVm;
    
        ProfileViewCommand = new RelayCommand(o => CurrentView = ProfileVm);
        SettingViewCommand = new RelayCommand(o => CurrentView = SettingVm);
        TestViewCommand = new RelayCommand(o => CurrentView = TestVm);
        GuideViewCommand = new RelayCommand(o => CurrentView = GuideVm);
    }
    public double BlurRadius 
    { 
        get => _blurRadius; 
        set { _blurRadius = value; OnPropertyChanged(); } 
    }

    public bool IsRegistrationVisible 
    { 
        get => _isRegistrationVisible; 
        set { _isRegistrationVisible = value; OnPropertyChanged(); } 
    }

    public string NewUserName 
    { 
        get => _newUserName; 
        set { _newUserName = value; OnPropertyChanged(); } 
    }

    public ICommand SaveNameCommand { get; }
}