using Avalonia.Controls;

namespace TestApp;

public partial class MainView : UserControl
{

    public MainView()
    {
        InitializeComponent();
    }

    public void Close()
    {
        ((MainViewModel)DataContext).Close();        
    }
}