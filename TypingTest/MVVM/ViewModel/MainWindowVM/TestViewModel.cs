using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using TypingTest.Core;
using TypingTest.MVVM.Model;

namespace TypingTest.MVVM.ViewModel.MainWindowVM;

public class TestViewModel:INotifyPropertyChanged
{
    private const int TEST_DURATION_SECONDS = 60; // Длительность теста: 60 секунд
        private const int WORDS_COUNT = 50; // Количество слов для генерации в тесте
        private DispatcherTimer _timer;
        private string _textToType;
        private string _userInput;
        private int _remainingTime;
        private double _wpm;
        private double _accuracy;
        private bool _isTestRunning;
        private DateTime _startTime;
        private List<string> _allAvailableWords; // Список всех слов из файла
        private int _totalCorrectCharsAccumulated = 0;
        private int _totalErrors = 0;
        
        
        private List<string> _wordsList; // Список усіх слів для набору
        private int _wordIndex;         // Індекс поточного слова
        private string _currentWord;
        
        public string CurrentWord
        {
            get => _currentWord;
            set { _currentWord = value; OnPropertyChanged(); }
        }

        public TestViewModel()
        {
            // Сначала загружаем словарь
            LoadWordsFromFile();
            // Затем инициализируем текст теста
            InitializeTest();
            
            StartTestCommand = new RelayCommand(StartTest, CanStartTest);
            
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        // --- Новая логика: Загрузка слов и Генерация текста ---

        private void LoadWordsFromFile()
        {
            // 1. Подготовка путей
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string lang = StatisticsManager.CurrentLanguage;
    
            // ВАЖНО: убедись, что файлы называются именно так (маленькими буквами)
            string fileName = lang == "ukr" ? "words_ukr.txt" : "words_eng.txt";

            // 2. Список мест, где программа будет искать файл
            string path1 = Path.Combine(baseDir, "Resources", "Text", fileName);
            string path2 = Path.Combine(baseDir, "Text", fileName);
            string path3 = Path.Combine(baseDir, fileName);

            string foundPath = null;

            // 3. Проверка существования файла
            if (File.Exists(path1)) foundPath = path1;
            else if (File.Exists(path2)) foundPath = path2;
            else if (File.Exists(path3)) foundPath = path3;

            // 4. Загрузка данных
            if (foundPath != null)
            {
                try 
                {
                    _allAvailableWords = File.ReadAllLines(foundPath)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => s.Trim().ToLower())
                        .ToList();
            
                    System.Diagnostics.Debug.WriteLine($"Успешно загружен словарь: {foundPath}");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Ошибка при чтении файла: " + ex.Message);
                }
            }
            else
            {
                // Если не нашли — показываем окно, чтобы ты мог скопировать путь
                System.Windows.MessageBox.Show(
                    $"ФАЙЛ НЕ НАЙДЕН!\n\n" +
                    $"Проверь название: {fileName}\n" +
                    $"Я искал его здесь:\n{path1}\n\n" +
                    $"Убедись, что в свойствах файла стоит 'Copy if newer'!", 
                    "Ошибка загрузки", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);

                // Заглушка, чтобы программа не вылетала
                _allAvailableWords = new List<string> { "ошибка", "файл", "не", "найден" };
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

            // Генерируем WORDS_COUNT случайных слов
            for (int i = 0; i < WORDS_COUNT; i++)
            {
                int index = random.Next(_allAvailableWords.Count);
                testWords.Add(_allAvailableWords[index]);
            }

            // Объединяем слова в одну строку, разделенную пробелами
            return string.Join(" ", testWords);
        }

        // --- Обновленный метод инициализации ---

        private void InitializeTest()
        {
            LoadWordsFromFile(); // Завантажуємо всі доступні слова (_allAvailableWords)
    
            // 1. Створюємо список слів для тесту
            TextToType = GenerateRandomText(); // Цей метод генерує рядок слів
            _wordsList = TextToType.Split(' ').ToList();

            // 2. Ініціалізуємо стан
            _wordIndex = 0;
    
            // 3. Встановлюємо перше слово
            CurrentWord = _wordsList.Any() ? _wordsList[_wordIndex] : string.Empty;

            // Скидання метрик
            UserInput = string.Empty;
            RemainingTime = TEST_DURATION_SECONDS;
            WPM = 0;
            Accuracy = 0;
            IsTestRunning = false;
        }

        // --- Свойства и остальные методы остаются без изменений ---

        public string TextToType
        {
            get => _textToType;
            set { _textToType = value; OnPropertyChanged(); }
        }

        public string UserInput
        {
            get => _userInput;
            set
            {
                // 1. Если тест не запущен и мы ввели первый символ — запускаем тест
                if (!_isTestRunning && !string.IsNullOrEmpty(value) && RemainingTime > 0)
                {
                    StartTest();
                }

                // 2. Если тест всё еще не запущен (например, время вышло) — блокируем ввод
                if (!_isTestRunning) return;

                // Логика пробела
                if (value.EndsWith(" "))
                {
                    string typedWord = value.Trim();
                    if (typedWord == CurrentWord)
                    {
                        _totalCorrectCharsAccumulated += CurrentWord.Length + 1;
                        MoveToNextWord();
                        return;
                    }
                }

                // Подсчет ошибок (сравнение с текущим вводом)
                if (!string.IsNullOrEmpty(value) && (value.Length > (_userInput?.Length ?? 0)))
                {
                    int index = value.Length - 1;
                    if (index < CurrentWord.Length)
                    {
                        if (value[index] != CurrentWord[index])
                        {
                            _totalErrors++; 
                        }
                    }
                    else
                    {
                        _totalErrors++; 
                    }
                }

                _userInput = value;
                OnPropertyChanged();
                CalculateMetrics();
            }
        }
        
        private void MoveToNextWord()
        {
            // 1. Збільшуємо індекс
            _wordIndex++;
    
            // 2. Перевіряємо, чи є ще слова
            if (_wordIndex < _wordsList.Count)
            {
                // Встановлюємо нове слово
                CurrentWord = _wordsList[_wordIndex];
        
                // *** КЛЮЧОВЕ ОЧИЩЕННЯ: ***
                // Очищаємо _userInput
                _userInput = string.Empty; 
        
                // Повідомляємо View, що властивість UserInput (поле введення) змінилася і має бути порожньою
                OnPropertyChanged(nameof(UserInput)); 
            }
            else
            {
                // Тест завершено
                EndTest();
            }
        }

        public int RemainingTime
        {
            get => _remainingTime;
            set { _remainingTime = value; OnPropertyChanged(); OnPropertyChanged(nameof(TimeDisplay)); }
        }
        
        public string TimeDisplay => $"{RemainingTime / 60:D2}:{RemainingTime % 60:D2}";

        public double WPM
        {
            get => _wpm;
            set { _wpm = value; OnPropertyChanged(); }
        }

        public double Accuracy
        {
            get => _accuracy;
            set { _accuracy = value; OnPropertyChanged(); }
        }

        public bool IsTestRunning
        {
            get => _isTestRunning;
            set { _isTestRunning = value; OnPropertyChanged(); }
        }

        public ICommand StartTestCommand { get; }

        private bool CanStartTest(object obj) => true; // Всегда можно начать/сбросить

        private void StartTest(object obj = null)
        {
            if (IsTestRunning) return; 

            // Загружаем актуальный словарь (укр/англ) из настроек
            LoadWordsFromFile();
            // Генерируем новый текст
            InitializeTest();

            // СБРОС ВСЕХ НАКОПИТЕЛЕЙ
            _totalCorrectCharsAccumulated = 0; 
            _totalErrors = 0; // ОБЯЗАТЕЛЬНО ОБНУЛЯЕМ ОШИБКИ

            IsTestRunning = true;
            _startTime = DateTime.Now;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            RemainingTime--;

            if (RemainingTime <= 0)
            {
                EndTest();
            }
            else
            {
                CalculateMetrics(); 
            }
        }

        private void EndTest()
        {
            _timer.Stop();
            IsTestRunning = false; // Сначала выключаем флаг

            // Сохраняем результат для передачи
            var finalResult = new TestResult
            {
                WPM = (int)this.WPM,
                Accuracy = this.Accuracy,
                TotalTime = TimeSpan.FromSeconds(60 - RemainingTime)
            };

            // ОЧИСТКА: теперь ввод заблокирован, так как IsTestRunning = false
            _currentWord = string.Empty;
            _userInput = string.Empty;
            _textToType = "Тест окончен.";
    
            // Уведомляем интерфейс
            OnPropertyChanged(nameof(CurrentWord));
            OnPropertyChanged(nameof(UserInput));
            OnPropertyChanged(nameof(TextToType));

            StatisticsManager.UpdateBestStats(finalResult);
    
            System.Windows.MessageBox.Show($"Финиш!\nСкорость: {finalResult.WPM} WPM\nТочность: {finalResult.Accuracy}%");
        }

        private void CalculateMetrics()
        {
            TimeSpan timeElapsed = DateTime.Now - _startTime;
            double minutesElapsed = timeElapsed.TotalMinutes;
            if (minutesElapsed < 0.01) minutesElapsed = 0.01;

            // Считаем общий объем ввода
            int currentTypedChars = _userInput?.Length ?? 0;
            int totalCharsTypedOverall = _totalCorrectCharsAccumulated + currentTypedChars;

            // 1. Расчет WPM
            WPM = Math.Round((totalCharsTypedOverall / 5.0) / minutesElapsed, 0);

            // 2. Расчет ТОЧНОСТИ
            if (totalCharsTypedOverall > 0)
            {
                // Формула: ((Всего нажатий - Ошибки) / Всего нажатий) * 100
                double accuracyValue = ((double)(totalCharsTypedOverall - _totalErrors) / totalCharsTypedOverall) * 100;
        
                // Ограничиваем, чтобы не уходило в минус
                Accuracy = Math.Round(Math.Max(0, accuracyValue), 2);
            }
            else
            {
                Accuracy = 100;
            }
        }

        // --- Реализация INotifyPropertyChanged ---
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
    
}