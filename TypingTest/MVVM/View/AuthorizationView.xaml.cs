using System.IO;
using System.Windows;
using System.Windows.Input;
using TypingTest.MVVM.ViewModel;


namespace TypingTest.MVVM.View;

public partial class AuthorizationView : Window
{
    public AuthorizationView()
    {
        InitializeComponent();
    }
    
    
    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if(e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }
    
    private void Button_Quit(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}