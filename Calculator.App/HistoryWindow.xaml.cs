using Calculator.Domain.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Calculator.App
{
    public partial class HistoryWindow : Window
    {
        private readonly IHistoryService _history;
        // zmienna potrzeba do Debounce, czyli po 250ms zacznie filtrować dane
        private readonly DispatcherTimer _debounce = new() { Interval = TimeSpan.FromMilliseconds(250) };

        public HistoryWindow(IHistoryService history)
        {
            InitializeComponent();
            _history = history;
            _debounce.Tick += async (_, __) => { _debounce.Stop(); await ReloadAsync(); };
            Loaded += async (_, __) => await ReloadAsync();
        }

        private async Task ReloadAsync()
        {
            var q = TxtFilter.Text?.Trim();
            // dodatkowo filtroanie działa od przynajmniej 2 znaku
            var rows = string.IsNullOrWhiteSpace(q) || q!.Length < 2
                ? await _history.GetLastAsync(500)
                : await _history.SearchAsync(q!, 500);

            var view = rows
                .Select(r => new HistoryRow(
                    r.Id,
                    r.Expression,
                    r.Result,
                    r.CreatedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")))
                .ToList();

            GridHistory.ItemsSource = view;
        }
        // po wpisie czegoś w filtr autormatycznie odświerza widok listy
        private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            _debounce.Stop();
            _debounce.Start();
        }

        public record HistoryRow(int Id, string Expression, string Result, string CreatedLocal);
    }
}
