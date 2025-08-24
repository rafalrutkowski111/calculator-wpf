using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Calculator.Domain.Services;

namespace Calculator.App;

public partial class FxWindow : Window
{
    private readonly IExchangeRateFetcher _fetcher;
    private readonly IExchangeRateStore _store;
    private readonly IExchangeAdvisor _advisor;

    public FxWindow(IExchangeRateFetcher fetcher, IExchangeRateStore store, IExchangeAdvisor advisor)
    {
        InitializeComponent();
        _fetcher = fetcher;
        _store = store;
        _advisor = advisor;

        DpFrom.SelectedDate = DateTime.Today.AddDays(-14);
        DpTo.SelectedDate = DateTime.Today;
    }

    private async void Calc_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var cur = ((ComboBoxItem)CmbCur.SelectedItem).Content!.ToString()!;
            var from = DateOnly.FromDateTime(DpFrom.SelectedDate ?? DateTime.Today);
            var to = DateOnly.FromDateTime(DpTo.SelectedDate ?? DateTime.Today);
            if (to < from) { MessageBox.Show("Data 'Do' nie może być wcześniejsza niż 'Od'."); return; }

            if (!decimal.TryParse(TxtAmount.Text.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            { MessageBox.Show("Podaj poprawną kwotę."); return; }

            // spróbuj z db
            var have = await _store.GetAsync(cur, from, to);
            var neededDays = to.DayNumber - from.DayNumber + 1;
            // jeśli brakuje dni – dociągnij cały zakres i upsert
            if (have.Select(p => p.Date).Distinct().Count() < neededDays)
            {
                try
                {
                    var fetched = await _fetcher.FetchAsync(cur, from, to);
                    if (fetched.Count > 0)
                    {
                        await _store.UpsertAsync(cur, fetched);
                        have = await _store.GetAsync(cur, from, to); // ponownie z db
                    }
                }
                catch
                {
                    // brak internetu
                }
            }
            // brak w bazie i brak internetu
            if (have.Count == 0)
            {
                MessageBox.Show("Brak kursów w DB i brak połączenia z NBP. Zmień zakres lub połącz z internetem.");
                GridRates.ItemsSource = null;
                TxtResult.Text = "";
                return;
            }

            GridRates.ItemsSource = have;

            var strategy = RbSell.IsChecked == true ? FxStrategy.Max : FxStrategy.Min;
            var (bestDate, rate, converted) = _advisor.ChooseBest(amount, strategy, have);

            var what = strategy == FxStrategy.Max ? "Najlepszy dzień (MAX)" : "Najlepszy dzień (MIN)";
            TxtResult.Text = $"{what}: {bestDate:yyyy-MM-dd}, kurs {rate} → kwota {converted} PLN";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Błąd FX: " + ex.Message);
        }
    }
}
