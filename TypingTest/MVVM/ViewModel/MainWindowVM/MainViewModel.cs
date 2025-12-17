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
        
        CurrentView = ProfileVm;
        
        ProfileViewCommand = new RelayCommand(o =>
        {
            CurrentView = ProfileVm;
        });
        SettingViewCommand = new RelayCommand(o =>
        {
            CurrentView = SettingVm;
        });
        TestViewCommand = new RelayCommand(o =>
        {
            CurrentView = TestVm;
        });
        GuideViewCommand = new RelayCommand(o =>
        {
            CurrentView = GuideVm;
        });
    }
}