using System;
using System.ComponentModel;
using System.Windows.Input;
using TypingTest.Core;
using TypingTest.MVVM.Model;

namespace TypingTest.MVVM.ViewModel.MainWindowVM;

public class ProfileViewModel:ObservableObject
{
    private UserModel _currentUser;
    private string _userPhotoPath;
    private TestResult _bestStats;

    public UserModel CurrentUser { get => _currentUser; set { _currentUser = value; OnPropertyChanged(); OnPropertyChanged(nameof(UserDisplayName)); } }
    public string UserPhotoPath { get => _userPhotoPath; set { _userPhotoPath = value; OnPropertyChanged(); } }
    public TestResult BestStats { get => _bestStats; set { _bestStats = value; OnPropertyChanged(); OnPropertyChanged(nameof(BestWPM)); OnPropertyChanged(nameof(BestAccuracy)); } }

    public string UserDisplayName => CurrentUser?.Username ?? "Гость";
    public string BestWPM => $"Найкраща швидкість: {BestStats?.WPM ?? 0} СЛОВ/ХВ";
    public string BestAccuracy => $"Найкраща точність: {BestStats?.Accuracy.ToString("F2") ?? "0.00"} %";
    public string TotalTests => $"Всього тестів пройдено: {StatisticsManager.GetTotalTestsCount()}";

    
    public ICommand LoadDataCommand { get; }

    public ProfileViewModel()
    {
        LoadDataCommand = new RelayCommand(param => LoadData());

        StatisticsManager.OnStatisticsChanged += Refresh; 
        
        LoadData();
    }

    public void LoadData()
    {
        UserPhotoPath = "pack://application:,,,/Resources/Images/profileImages.png";
        SettingsManager.Load();

        if (CurrentUser == null) 
        {
            CurrentUser = new UserModel(); 
        }

        CurrentUser.Username = !string.IsNullOrEmpty(SettingsManager.UserName) 
            ? SettingsManager.UserName 
            : "Гість";

        OnPropertyChanged(nameof(UserDisplayName));

        BestStats = StatisticsManager.GetBestStats(); 
    
        OnPropertyChanged(nameof(TotalTests));
        OnPropertyChanged(nameof(BestWPM));
        OnPropertyChanged(nameof(BestAccuracy));
    }

    public void Refresh()
    {
        LoadData(); 
    }
    
}