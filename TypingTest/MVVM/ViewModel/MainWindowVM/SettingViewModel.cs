using TypingTest.Core;

namespace TypingTest.MVVM.ViewModel.MainWindowVM;

public class SettingViewModel:ObservableObject
{
    public bool IsUkrSelected
    {
        get => StatisticsManager.CurrentLanguage == "ukr";
        set 
        { 
            if (value) 
            {
                StatisticsManager.CurrentLanguage = "ukr"; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEngSelected)); // Уведомляем вторую кнопку
            }
        }
    }

    public bool IsEngSelected
    {
        get => StatisticsManager.CurrentLanguage == "eng";
        set 
        { 
            if (value) 
            {
                StatisticsManager.CurrentLanguage = "eng"; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsUkrSelected)); // Уведомляем вторую кнопку
            }
        }
    }
}