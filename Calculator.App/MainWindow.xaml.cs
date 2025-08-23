using Calculator.Domain.Services;
using System.Globalization;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace Calculator.App;
public partial class MainWindow : Window
{
    private readonly ICalculatorEngine _engine;
    private readonly IHistoryService _history;

    private decimal? _leftOperand = null;     // lewy operand
    private string? _pendingOperator = null;  // operator oczekujący na prawy operand
    private bool _isNewInput = true;  // flaga infromująca czy zaczynamy wpis nowej liczby do Display

    // Do powtarzania "=" (np 6+6=12, kolejne dodaje 6)
    private string? _repeatOperator = null;
    private decimal? _repeatRightOperand = null;
    private bool _wasEquals = false;          // czy ostatni klawisz to "="

    private bool _pastedRawMode = false; //tryb wklejania

    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture; // zawsze kropki

    public MainWindow(ICalculatorEngine engine, IHistoryService history)
    {
        InitializeComponent();
        CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, Paste_Executed)); // do wklejania
        _engine = engine;
        _history = history;
        Display.Text = "0";
    }

    // event wcisnięcia liczby
    private void Digit_Click(object sender, RoutedEventArgs e)
    {
        _pastedRawMode = false;
        var digit = ((Button)sender).Content?.ToString() ?? "";

        // Po '=' bez operatora zaczynamy nowe działanie
        if (_wasEquals && _pendingOperator is null)
            ResetAll();

        if (_isNewInput || Display.Text == "0")
        {
            Display.Text = digit;
            _isNewInput = false;
        }
        else
        {
            Display.Text += digit;
        }
        _wasEquals = false;
    }
    // event wcisnięcia kropki
    private void Dot_Click(object sender, RoutedEventArgs e)
    {
        _pastedRawMode = false;

        if (_wasEquals && _pendingOperator is null)
            ResetAll();

        if (_isNewInput)
        {
            Display.Text = "0.";
            _isNewInput = false;
        }
        else if (!Display.Text.Contains('.'))
        {
            Display.Text += ".";
        }
        _wasEquals = false;
    }
    // event wcisnięcia operatora
    private void Operator_Click(object sender, RoutedEventArgs e)
    {
        _pastedRawMode = false;

        var op = ((Button)sender).Content?.ToString();

        // Zmiana operatora bez wprowadzania prawego operandu
        if (_pendingOperator != null && _isNewInput)
        {
            _pendingOperator = op;
            return;
        }

        if (_pendingOperator != null && !_isNewInput)
        {
            // łańcuchowe działanie: policz poprzednie, pokaż wynik
            EqualCore(saveRepeat: true);
            // ustaw nowy operator i czekaj na prawy operand
            _pendingOperator = op;
            _isNewInput = true;
        }
        else
        {
            // pierwszy operator w sekwencji
            _leftOperand = ParseDisplay();
            _pendingOperator = op;
            _isNewInput = true;
        }

        _wasEquals = false;
    }
    // event wcisnięcia "="
    private async void Equals_Click(object sender, RoutedEventArgs e)
    {
        // wklejanie tekstu
        if (_pastedRawMode)
        {
            try
            {
                var result = _engine.Evaluate(Display.Text);
                Display.Text = result.ToString(Inv);

                // zapis do historii
                await TrySaveHistoryAsync(Display.Text, result);

                // przygotuj stan po policzeniu
                _leftOperand = result;
                _pendingOperator = null;
                _isNewInput = true;
                _repeatOperator = null;
                _repeatRightOperand = null;
            }
            catch
            {
                // niepoprawny format
                Display.Text = "0";
                _leftOperand = null;
                _pendingOperator = null;
                _isNewInput = true;
                _repeatOperator = null;
                _repeatRightOperand = null;
            }

            _pastedRawMode = false;
            _wasEquals = true;
            return;
        }
        // a op b
        if (_pendingOperator != null && _leftOperand != null && !_isNewInput)
        {
            var (expr, result) = EqualCore(saveRepeat: true);
            await TrySaveHistoryAsync(expr, result);
        }
        // powtarzanie "=" (np. po 6+6=)
        else if (_repeatOperator != null && _repeatRightOperand != null)
        {
            var left = ParseDisplay();
            var expr = $"{left.ToString(Inv)}{_repeatOperator}{_repeatRightOperand.Value.ToString(Inv)}";
            var result = _engine.Evaluate(expr);
            Display.Text = result.ToString(Inv);

            _leftOperand = result;           // wynik staje się lewym operandem
            _isNewInput = true;
            // pozostawiamy _repeatOperator/_repeatRightOperand bez zmian

            await TrySaveHistoryAsync(expr, result);
        }

        _wasEquals = true;
    }
    // robi obliczenia i zarządza stanem
    private (string expr, decimal result) EqualCore(bool saveRepeat)
    {
        var right = ParseDisplay();
        var left = _leftOperand ?? 0m;
        var op = _pendingOperator ?? "+";

        var expr = $"{left.ToString(Inv)}{op}{right.ToString(Inv)}";
        var result = _engine.Evaluate(expr);

        Display.Text = result.ToString(Inv);

        // przygotuj stan na dalsze działania
        _leftOperand = result;
        _pendingOperator = null;     // brak operatora w toku
        _isNewInput = true;

        if (saveRepeat)
        {
            _repeatOperator = op;
            _repeatRightOperand = right;
        }

        return (expr, result);
    }
    // event wcisnięcia C - czyszczenia wszystkiego
    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _pastedRawMode = false;
        ResetAll();
    }
    // event wcisnięcia CE - czyszczenia ostatniej operacji
    private void ClearEntry_Click(object sender, RoutedEventArgs e)
    {
        _pastedRawMode = false;
        Display.Text = "0";
        _isNewInput = true;
        // nie ruszamy lewego i operatora
    }
    // event wcisnięcia ⌫ - czyszczenia ostatniej liczby
    private void Backspace_Click(object sender, RoutedEventArgs e)
    {
        if (_isNewInput) return;
        var t = Display.Text;
        if (t.Length > 1)
            Display.Text = t[..^1];
        else
            Display.Text = "0";
    }
    // event wcisnięcia ± do podmieniania znakó
    private void ToggleSign_Click(object sender, RoutedEventArgs e)
    {
        if (_isNewInput && Display.Text == "0") return;

        if (Display.Text.StartsWith("-", StringComparison.Ordinal))
            Display.Text = Display.Text[1..];
        else if (Display.Text != "0")
            Display.Text = "-" + Display.Text;
    }
    // dodatkowa walidacja, ale nigdy sie nie wykona, bo Evaluate sprawdza wklejone rzeczy
    private decimal ParseDisplay()
        => decimal.TryParse(Display.Text, NumberStyles.Number, Inv, out var d) ? d : 0m;
    private async Task TrySaveHistoryAsync(string expr, decimal result)
    {
        try
        {
            await _history.SaveAsync(expr, result.ToString(Inv), DateTime.UtcNow);
        }
        catch
        {

        }
    }
    private void ResetAll()
    {
        _leftOperand = null;
        _pendingOperator = null;
        _repeatOperator = null;
        _repeatRightOperand = null;
        _isNewInput = true;
        _wasEquals = false;
        Display.Text = "0";
    }
    // menu
    private void History_Click(object sender, RoutedEventArgs e)
    {
    }
    private void Fx_Click(object sender, RoutedEventArgs e)
    {
    }
    private void Settings_Click(object sender, RoutedEventArgs e)
    {
    }
    // podpięcie klawiatury
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        // Cyfry
        if (e.Key >= Key.D0 && e.Key <= Key.D9)
        {
            InvokeDigit(((int)(e.Key - Key.D0)).ToString());
            e.Handled = true; return;
        }
        if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
        {
            InvokeDigit(((int)(e.Key - Key.NumPad0)).ToString());
            e.Handled = true; return;
        }

        // Separator dziesiętny: zarówno '.' jak i ',' traktujemy jako kropkę
        if (e.Key == Key.Decimal || e.Key == Key.OemComma || e.Key == Key.OemPeriod)
        {
            Dot_Click(this, new RoutedEventArgs());
            e.Handled = true; return;
        }

        // Operatory
        if (e.Key == Key.Add || e.Key == Key.OemPlus) { InvokeOperator("+"); e.Handled = true; return; }
        if (e.Key == Key.Subtract || e.Key == Key.OemMinus) { InvokeOperator("-"); e.Handled = true; return; }
        if (e.Key == Key.Multiply) { InvokeOperator("*"); e.Handled = true; return; }
        if (e.Key == Key.Divide || e.Key == Key.Oem2) { InvokeOperator("/"); e.Handled = true; return; }

        // Równa się
        if (e.Key == Key.Enter || e.Key == Key.Return) { Equals_Click(this, new RoutedEventArgs()); e.Handled = true; return; }

        // Kasowanie
        if (e.Key == Key.Back) { Backspace_Click(this, new RoutedEventArgs()); e.Handled = true; return; }
        if (e.Key == Key.Escape) { Clear_Click(this, new RoutedEventArgs()); e.Handled = true; return; }
    }

    // wywołanie użyia przycisków
    private void InvokeDigit(string d)
    {
        var b = new Button { Content = d };
        Digit_Click(b, new RoutedEventArgs());
    }
    private void InvokeOperator(string op)
    {
        var b = new Button { Content = op };
        Operator_Click(b, new RoutedEventArgs());
    }

    // wklejanie tekstu
    private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        try
        {
            var raw = Clipboard.GetText() ?? string.Empty;

            // wyczyść cały stan działania
            _leftOperand = null;
            _pendingOperator = null;
            _repeatOperator = null;
            _repeatRightOperand = null;
            _isNewInput = false;
            _wasEquals = false;

            // tekst przyjmie wklenie
            Display.Text = raw;
            _pastedRawMode = true;
        }
        catch
        {
            // w razie problemu nic nie rób
        }
    }



}
