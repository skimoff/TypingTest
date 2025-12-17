using System.IO;
using System.Text.Json;
using TypingTest.MVVM.Model;

namespace TypingTest.Core;

public static class StatisticsManager
{
    private static readonly string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TypingTest");
    private static readonly string FilePath = Path.Combine(FolderPath, "stats.json");

    private static TestResult _bestStats = new TestResult { WPM = 0, Accuracy = 0 };
    private static int _totalTests = 0;
    private static string _currentLanguage = "ukr"; // По умолчанию украинский

    public static event Action OnStatisticsChanged;

    static StatisticsManager() { LoadFromFile(); }

    public static string CurrentLanguage
    {
        get => _currentLanguage;
        set { if (_currentLanguage != value) { _currentLanguage = value; SaveToFile(); } }
    }

    public static TestResult GetBestStats() => _bestStats;
    public static int GetTotalTestsCount() => _totalTests;

    public static void UpdateBestStats(TestResult newStats)
    {
        _totalTests++;
        if (newStats.WPM > _bestStats.WPM || (newStats.WPM == _bestStats.WPM && newStats.Accuracy > _bestStats.Accuracy))
        {
            _bestStats = newStats;
        }
        SaveToFile();
        OnStatisticsChanged?.Invoke();
    }

    private static void SaveToFile()
    {
        try
        {
            if (!Directory.Exists(FolderPath)) Directory.CreateDirectory(FolderPath);
            var data = new { BestStats = _bestStats, TotalTests = _totalTests, CurrentLanguage = _currentLanguage };
            File.WriteAllText(FilePath, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
        } catch { }
    }

    private static void LoadFromFile()
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            var data = JsonDocument.Parse(File.ReadAllText(FilePath)).RootElement;
            if (data.TryGetProperty("BestStats", out var s)) _bestStats = JsonSerializer.Deserialize<TestResult>(s.GetRawText());
            if (data.TryGetProperty("TotalTests", out var t)) _totalTests = t.GetInt32();
            if (data.TryGetProperty("CurrentLanguage", out var l)) _currentLanguage = l.GetString();
        } catch { }
    }
}