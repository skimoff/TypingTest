using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using TypingTest.Core;
using TypingTest.MVVM.Model;

namespace TypingTest.MVVM.ViewModel.MainWindowVM;

public class TestViewModel : INotifyPropertyChanged
{
    private const int TEST_DURATION_SECONDS = 60;
    private const int WORDS_COUNT = 50;
    private DispatcherTimer _timer;
    private string _textToType;
    private string _userInput;
    private int _remainingTime;
    private double _wpm;
    private double _accuracy;
    private bool _isTestRunning;
    private DateTime _startTime;
    private List<string> _allAvailableWords;
    private int _totalCorrectCharsAccumulated = 0;
    private int _totalErrors = 0;
    public ObservableCollection<CharDisplayModel> CurrentWordChars { get; set; } = new();
    private List<string> _wordsList;
    private int _wordIndex;
    private string _currentWord;
    
    public ICommand StartTestCommand { get; }

    private bool CanStartTest(object obj) => true;
    public string TimeDisplay => $"{RemainingTime / 60:D2}:{RemainingTime % 60:D2}";

    public TestViewModel()
    {
        LoadWordsFromFile();
        InitializeTest();

        StartTestCommand = new RelayCommand(StartTest, CanStartTest);

        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
    }
    
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    public string CurrentWord
    {
        get => _currentWord;
        set
        {
            _currentWord = value;
            CurrentWordChars.Clear();
            foreach (var c in _currentWord) 
                CurrentWordChars.Add(new CharDisplayModel { CharValue = c });
            OnPropertyChanged();
        }
    }
    
    public string TextToType
    {
        get => _textToType;
        set
        {
            _textToType = value;
            OnPropertyChanged();
        }
    }

    public string UserInput
    {
        get => _userInput;
        set
        {
            if (value == null) return;

            if (_userInput != null && value.Length < _userInput.Length)
            {
                OnPropertyChanged();
                return;
            }

            if (value.EndsWith(" "))
            {
                if (value.Trim() == CurrentWord)
                {
                    CheckWord(value.Trim());
                    _userInput = string.Empty;
                }
                OnPropertyChanged();
                return;
            }
            
            if (_userInput == CurrentWord)
            {
                OnPropertyChanged();
                return;
            }

            if (!_isTestRunning && !string.IsNullOrEmpty(value))
            {
                StartTimerOnly();
            }

            if (value.Length > (_userInput?.Length ?? 0))
            {
                int index = value.Length - 1;
                if (index < CurrentWord.Length)
                {
                    if (value[index] == CurrentWord[index])
                    {
                        CurrentWordChars[index].UnderlineColor = "#00FF00";
                    }
                    else
                    {
                        CurrentWordChars[index].UnderlineColor = "#FF0000";
                        _totalErrors++;
                        CalculateMetrics();
                        return; 
                    }
                }
            }

            _userInput = value;
            OnPropertyChanged();
            CalculateMetrics();
        }
    }
    
    public int RemainingTime
    {
        get => _remainingTime;
        set
        {
            _remainingTime = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TimeDisplay));
        }
    }
    public double WPM
    {
        get => _wpm;
        set
        {
            _wpm = value;
            OnPropertyChanged();
        }
    }

    public double Accuracy
    {
        get => _accuracy;
        set
        {
            _accuracy = value;
            OnPropertyChanged();
        }
    }

    public bool IsTestRunning
    {
        get => _isTestRunning;
        set
        {
            _isTestRunning = value;
            OnPropertyChanged();
        }
    }
    
    private void LoadWordsFromFile()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string lang = SettingsManager.Language;
        string fileName = lang == "ukr" ? "words_ukr.txt" : "words_eng.txt";

        string path1 = Path.Combine(baseDir, "Resources", "Text", fileName);
        string path2 = Path.Combine(baseDir, "Text", fileName);
        string path3 = Path.Combine(baseDir, fileName);

        string foundPath = null;
        if (File.Exists(path1)) foundPath = path1;
        else if (File.Exists(path2)) foundPath = path2;
        else if (File.Exists(path3)) foundPath = path3;

        if (foundPath != null)
        {
            try
            {
                _allAvailableWords = File.ReadAllLines(foundPath)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim().ToLower())
                    .ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Ошибка при чтении файла: " + ex.Message);
            }
        }
        else
        {
            _allAvailableWords = lang == "ukr"
                ? new List<string> { "помилка", "файл", "відсутній" }
                : new List<string> { "error", "no", "file" };
        }
    }

    private string GenerateRandomText()
    {
        if (_allAvailableWords == null || _allAvailableWords.Count == 0)
        {
            return "Не удалось загрузить словарь.";
        }

        Random random = new Random();
        List<string> testWords = new List<string>();

        for (int i = 0; i < WORDS_COUNT; i++)
        {
            int index = random.Next(_allAvailableWords.Count);
            testWords.Add(_allAvailableWords[index]);
        }

        return string.Join(" ", testWords);
    }
    
    private void InitializeTest()
    {
        LoadWordsFromFile(); 
        TextToType = GenerateRandomText(); 
        _wordsList = TextToType.Split(' ').ToList();
        _wordIndex = 0;
        CurrentWord = _wordsList[0];

        _userInput = string.Empty;
        RemainingTime = TEST_DURATION_SECONDS;
        WPM = 0;
        Accuracy = 100;
        _totalCorrectCharsAccumulated = 0;
        _totalErrors = 0;
        _wordIndex = 0;
        _isTestRunning = false;

        OnPropertyChanged(nameof(UserInput));
        OnPropertyChanged(nameof(IsTestRunning));
    }
    
    private void CheckWord(string typedWord)
    {
        _totalCorrectCharsAccumulated += typedWord.Length;

        _wordIndex++;
        if (_wordIndex < _wordsList.Count)
            CurrentWord = _wordsList[_wordIndex];
        else
            EndTest(true);
    }

    private void StartTimerOnly()
    {
        _isTestRunning = true;
        _startTime = DateTime.Now;
        _timer.Start();
        OnPropertyChanged(nameof(IsTestRunning));
    }
    
    private void StartTest(object obj = null)
    {
        if (IsTestRunning)
        {
            EndTest(saveToStats: false);
        }
        else
        {
            InitializeTest();
        }
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        RemainingTime--;
        if (RemainingTime <= 0)
        {
            EndTest(saveToStats: true);
        }
        else
        {
            CalculateMetrics(); 
        }
    }
    
    private void EndTest(bool saveToStats = true)
    {
        _timer.Stop();
        IsTestRunning = false;

        if (saveToStats)
        {
            var finalResult = new TestResult
            {
                WPM = (int)this.WPM,
                Accuracy = this.Accuracy,
                TotalTime = TimeSpan.FromSeconds(60 - RemainingTime)
            };
            StatisticsManager.UpdateBestStats(finalResult);
            System.Windows.MessageBox.Show($"Тест завершен!\nСкорость: {finalResult.WPM} WPM\nТочность: {finalResult.Accuracy}%");
        }
        else
        {
            System.Windows.MessageBox.Show("Тест отменен. Статистика не сохранена.");
        }

        _currentWord = string.Empty;
        _userInput = string.Empty;
        _textToType = "Тест окончен.";
    
        OnPropertyChanged(nameof(CurrentWord));
        OnPropertyChanged(nameof(UserInput));
        OnPropertyChanged(nameof(TextToType));
    }

    private void CalculateMetrics()
    {
        if (!_isTestRunning) return;

        double minutesElapsed = (DateTime.Now - _startTime).TotalMinutes;
        if (minutesElapsed < 0.01) minutesElapsed = 0.01;

        int totalCorrectOverall = _totalCorrectCharsAccumulated + (_userInput?.Length ?? 0);

        WPM = Math.Round((totalCorrectOverall / 5.0) / minutesElapsed, 0);

        int totalTypedAttempts = totalCorrectOverall + _totalErrors;

        if (totalTypedAttempts > 0)
            Accuracy = Math.Round(((double)totalCorrectOverall / totalTypedAttempts) * 100, 2);
        else
            Accuracy = 100;
    }
}