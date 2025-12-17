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

    // Свойства (оставляем как есть)
    public UserModel CurrentUser { get => _currentUser; set { _currentUser = value; OnPropertyChanged(); OnPropertyChanged(nameof(UserDisplayName)); } }
    public string UserPhotoPath { get => _userPhotoPath; set { _userPhotoPath = value; OnPropertyChanged(); } }
    public TestResult BestStats { get => _bestStats; set { _bestStats = value; OnPropertyChanged(); OnPropertyChanged(nameof(BestWPM)); OnPropertyChanged(nameof(BestAccuracy)); } }

    // Текстовые свойства для View
    public string UserDisplayName => CurrentUser?.Username ?? "Гость";
    public string BestWPM => $"Лучшая скорость: {BestStats?.WPM ?? 0} СЛОВ/МИН";
    public string BestAccuracy => $"Лучшая точность: {BestStats?.Accuracy.ToString("F2") ?? "0.00"} %";
    public string TotalTests => $"Всего тестов пройдено: {StatisticsManager.GetTotalTestsCount()}";

    public ICommand SelectPhotoCommand { get; }
    public ICommand LoadDataCommand { get; }

    public ProfileViewModel()
    {
        SelectPhotoCommand = new RelayCommand(param => ExecuteSelectPhoto());
        LoadDataCommand = new RelayCommand(param => LoadData());

        // Подписываемся на обновление статистики
        StatisticsManager.OnStatisticsChanged += Refresh; 
        
        LoadData();
    }

    public void LoadData()
    {
        // Инициализируем юзера, если его нет
        if (CurrentUser == null) 
        {
            CurrentUser = new UserModel { Username = "Skimoff" }; 
        }

        // Берем свежие данные из менеджера (который загрузил их из JSON)
        BestStats = StatisticsManager.GetBestStats(); 
        
        // ПРИНУДИТЕЛЬНО уведомляем UI, что количество тестов могло измениться
        OnPropertyChanged(nameof(TotalTests));
        OnPropertyChanged(nameof(BestWPM));
        OnPropertyChanged(nameof(BestAccuracy));
    }

    public void Refresh()
    {
        // Вызывается автоматически, когда StatisticsManager сохраняет новый результат
        LoadData(); 
    }

    private void ExecuteSelectPhoto()
    {
        if (string.IsNullOrEmpty(UserPhotoPath))
            UserPhotoPath = "Resources/Images/default_avatar.png"; 
        else
            UserPhotoPath = null;
    }
}