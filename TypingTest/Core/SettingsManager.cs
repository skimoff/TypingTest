using System.IO;
using System.Text.Json;
using System.Windows;

namespace TypingTest.Core;

public class SettingsManager
{
   private static readonly string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TypingTest");
    private static readonly string FilePath = Path.Combine(FolderPath, "settings.json");
    public static string UserName { get; set; }

    public static string Language { get; set; } = "ukr";
    public static string Theme { get; set; } = "Dark";
    static SettingsManager()
    {
        Load();
    }

    public static void Save()
    {
        try
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            var settings = new
            {
                Language = Language,
                Theme = Theme,
                UserName = UserName 
            };

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        
            System.Diagnostics.Debug.WriteLine($"Файл сохранен по пути: {FilePath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ОШИБКА СОХРАНЕНИЯ: {ex.Message}");
        }
    }

    public static void Load()
    {
        if (!File.Exists(FilePath))
        {
            ApplyTheme();
            return;
        }

        try
        {
            var json = File.ReadAllText(FilePath);
            using var doc = JsonDocument.Parse(json);
            
            Language = doc.RootElement.GetProperty("Language").GetString() ?? "ukr";
            Theme = doc.RootElement.GetProperty("Theme").GetString() ?? "Dark";
            
            if (doc.RootElement.TryGetProperty("UserName", out var nameProp))
            {
                UserName = nameProp.GetString();
            }

            ApplyTheme();
        }
        catch 
        { 
            ApplyTheme(); 
        }
    }

    public static void ApplyTheme()
    {
        string themeFile = Theme == "Light" ? "LightStyle.xaml" : "DarkStyle.xaml";

        try
        {
            var dictionaries = Application.Current.Resources.MergedDictionaries;
            
            dictionaries.Clear(); 

            var uri = new Uri($"/TypingTest;component/Resources/Styles/{themeFile}", UriKind.RelativeOrAbsolute);
        
            if (Application.LoadComponent(uri) is ResourceDictionary resDict)
            {
                dictionaries.Add(resDict);
            }

            foreach (Window window in Application.Current.Windows)
            {
                window.InvalidateVisual();
            }

            System.Diagnostics.Debug.WriteLine($"Тема успешно изменена на: {themeFile}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ОШИБКА ЗАГРУЗКИ ТЕМЫ: {ex.Message}");
        }
    }
}