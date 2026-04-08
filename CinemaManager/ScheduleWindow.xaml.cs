using CinemaManager.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Input;

namespace CinemaManager
{
    /// <summary>
    /// Логика взаимодействия для ScheduleWindow.xaml
    /// </summary>
    public partial class ScheduleWindow : Window
    {
        public ScheduleWindow()
        {
            InitializeComponent();
            DatePicker.SelectedDate = DateTime.Today;
            LoadLookupsAsync();
            LoadSessionsAsync();
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (Owner is Window owner)
                owner.Show();
        }

        private async void LoadLookupsAsync()
        {
            try
            {
                var movies = await ApiClient.Http.GetFromJsonAsync<List<ApiMovieFull>>("api/movies") ?? new();
                MovieComboBox.ItemsSource = movies;
                if (movies.Count > 0) MovieComboBox.SelectedIndex = 0;

                var halls = await ApiClient.Http.GetFromJsonAsync<List<ApiHallFull>>("api/halls") ?? new();
                HallComboBox.ItemsSource = halls;
                if (halls.Count > 0) HallComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки справочников: {ex.Message}");
            }
        }

        private async void LoadSessionsAsync()
        {
            try
            {
                var sessions = await ApiClient.Http.GetFromJsonAsync<List<ApiSession>>("api/sessions") ?? new();
                var rows = sessions
                    .OrderBy(s => s.StartTime)
                    .Select(s => new SessionRow
                    {
                        Id = s.Id,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        MovieTitle = s.Movie?.Title ?? $"MovieId={s.MovieId}",
                        HallName = s.Hall?.Name ?? $"HallId={s.HallId}",
                        TicketPrice = s.TicketPrice
                    })
                    .ToList();

                SessionsGrid.ItemsSource = rows;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сеансов: {ex.Message}\nУбедитесь, что API запущен и вы авторизованы.");
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void BtnAddSession_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MovieComboBox.SelectedValue is not int movieId || movieId <= 0)
                {
                    MessageBox.Show("Выберите фильм.");
                    return;
                }

                if (HallComboBox.SelectedValue is not int hallId || hallId <= 0)
                {
                    MessageBox.Show("Выберите зал.");
                    return;
                }

                var date = DatePicker.SelectedDate;
                if (date is null)
                {
                    MessageBox.Show("Выберите дату.");
                    return;
                }

                if (!TimeSpan.TryParse(TimeTextBox.Text, out var time))
                {
                    MessageBox.Show("Некорректное время. Пример: 18:30");
                    return;
                }

                if (!decimal.TryParse(PriceTextBox.Text, out var price) || price <= 0)
                {
                    MessageBox.Show("Некорректная цена.");
                    return;
                }

                var start = date.Value.Date.Add(time);

                var payload = new
                {
                    movieId,
                    hallId,
                    startTime = start,
                    ticketPrice = price
                };

                var resp = await ApiClient.Http.PostAsJsonAsync("api/sessions", payload);

                if (resp.StatusCode == HttpStatusCode.Conflict)
                {
                    var msg = await resp.Content.ReadAsStringAsync();
                    MessageBox.Show(string.IsNullOrWhiteSpace(msg) ? "Пересечение сеансов по времени." : msg);
                    return;
                }

                if (!resp.IsSuccessStatusCode)
                {
                    var text = await resp.Content.ReadAsStringAsync();
                    MessageBox.Show($"Не удалось добавить сеанс: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{text}");
                    return;
                }

                LoadSessionsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления сеанса: {ex.Message}");
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }

    public sealed class SessionRow
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string MovieTitle { get; set; } = string.Empty;
        public string HallName { get; set; } = string.Empty;
        public decimal TicketPrice { get; set; }
    }

    public sealed class ApiSession
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public int HallId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TicketPrice { get; set; }
        public ApiMovie? Movie { get; set; }
        public ApiHallRef? Hall { get; set; }
    }

    public sealed class ApiMovie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public sealed class ApiMovieFull
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    public sealed class ApiHallFull
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed class ApiHallRef
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
