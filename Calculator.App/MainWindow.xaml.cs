using System.Windows;
using Calculator.Domain.Services;

namespace Calculator.App;

public partial class MainWindow : Window
{
    private readonly ICalculatorEngine _engine;
    private readonly IHistoryService _history;

    public MainWindow(ICalculatorEngine engine, IHistoryService history)
    {
        InitializeComponent();
        _engine = engine;
        _history = history;
    }
}
