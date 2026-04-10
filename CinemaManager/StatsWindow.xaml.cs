using CinemaManager.Api;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Net.Http.Json;

namespace CinemaManager
{
    /// <summary>
    /// Логика взаимодействия для StatsWindow.xaml
    /// </summary>
    public partial class StatsWindow : Window
    {
        public StatsWindow()
        {
            InitializeComponent();
            StartDatePicker.SelectedDate = DateTime.Today.AddDays(-7);
            EndDatePicker.SelectedDate = DateTime.Today;
            LoadStatsAsync();
        }

        private async void LoadStatsAsync()
        {
            try
            {
                StatsStatusText.Text = "Загрузка…";

                var url = BuildStatsUrl();
                var dto = await ApiClient.Http.GetFromJsonAsync<StatsDto>(url);
                if (dto is null)
                {
                    StatsStatusText.Text = "Не удалось получить статистику.";
                    return;
                }

                MoviesCountText.Text = dto.Movies.ToString();
                HallsCountText.Text = dto.Halls.ToString();
                SessionsCountText.Text = dto.Sessions.ToString();
                TicketsCountText.Text = dto.Tickets.ToString();
                TotalIncomeText.Text = $"Общий доход: {dto.TotalIncome} руб.";
                MovieSalesGrid.ItemsSource = dto.MovieSales ?? new List<MovieSalesDto>();

                StatsStatusText.Text = $"Обновлено: {DateTime.Now:dd.MM.yyyy HH:mm}";
            }
            catch (Exception ex)
            {
                StatsStatusText.Text = "Ошибка загрузки.";
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}\nУбедитесь, что API запущен и вы авторизованы.");
            }
        }

        private void BtnShow_Click(object sender, RoutedEventArgs e)
        {
            LoadStatsAsync();
        }

        private string BuildStatsUrl()
        {
            var parts = new List<string>();

            if (StartDatePicker.SelectedDate is DateTime start)
                parts.Add($"startDate={Uri.EscapeDataString(start.ToString("O", CultureInfo.InvariantCulture))}");

            if (EndDatePicker.SelectedDate is DateTime end)
            {
                // inclusive end-of-day
                var inclusive = end.Date.AddDays(1).AddTicks(-1);
                parts.Add($"endDate={Uri.EscapeDataString(inclusive.ToString("O", CultureInfo.InvariantCulture))}");
            }

            return parts.Count == 0 ? "api/stats" : $"api/stats?{string.Join("&", parts)}";
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (Owner is Window owner)
                owner.Show();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }

    public sealed class StatsDto
    {
        public int Movies { get; set; }
        public int Halls { get; set; }
        public int Sessions { get; set; }
        public int Tickets { get; set; }
        public decimal TotalIncome { get; set; }
        public List<MovieSalesDto>? MovieSales { get; set; }
    }

    public sealed class MovieSalesDto
    {
        public string MovieTitle { get; set; } = string.Empty;
        public int TicketsSold { get; set; }
        public decimal Income { get; set; }
    }
}
