using CinemaManager.Api;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CinemaManager
{
    public partial class HallsWindow : Window
    {
        public HallsWindow()
        {
            InitializeComponent();
            LoadHallsAsync();
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (Owner is Window owner)
                owner.Show();
        }

        private async void LoadHallsAsync()
        {
            try
            {
                var halls = await ApiClient.Http.GetFromJsonAsync<List<ApiHall>>("api/halls") ?? new();
                var rows = new List<HallRow>();

                foreach (var h in halls)
                {
                    rows.Add(new HallRow
                    {
                        Id = h.Id,
                        Name = h.Name,
                        Type = h.Type,
                        Capacity = $"{h.RowsCount * h.SeatsPerRow} мест ({h.RowsCount}x{h.SeatsPerRow})",
                        Actions = "—"
                    });
                }

                HallsGrid.ItemsSource = rows;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки залов: {ex.Message}\nУбедитесь, что API запущен и вы авторизованы.");
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void BtnSaveHall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var name = HallNameTextBox.Text?.Trim() ?? string.Empty;
                var type = (HallTypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(type))
                {
                    MessageBox.Show("Введите название и тип зала.");
                    return;
                }

                if (!int.TryParse(RowsCountTextBox.Text, out var rows) || rows <= 0)
                {
                    MessageBox.Show("Некорректное число рядов.");
                    return;
                }

                if (!int.TryParse(SeatsPerRowTextBox.Text, out var seatsPerRow) || seatsPerRow <= 0)
                {
                    MessageBox.Show("Некорректное число мест в ряду.");
                    return;
                }

                var payload = new
                {
                    name,
                    type,
                    rowsCount = rows,
                    seatsPerRow
                };

                var response = await ApiClient.Http.PostAsJsonAsync("api/halls", payload);
                if (!response.IsSuccessStatusCode)
                {
                    var text = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Не удалось сохранить зал: {(int)response.StatusCode} {response.ReasonPhrase}\n{text}");
                    return;
                }

                HallNameTextBox.Text = "";
                RowsCountTextBox.Text = "10";
                SeatsPerRowTextBox.Text = "15";
                HallTypeComboBox.SelectedIndex = 0;

                await System.Threading.Tasks.Task.Delay(150);
                LoadHallsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения зала: {ex.Message}");
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }

    public sealed class HallRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Capacity { get; set; } = string.Empty;
        public string Actions { get; set; } = string.Empty;
    }

    public sealed class ApiHall
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int RowsCount { get; set; }
        public int SeatsPerRow { get; set; }
    }

}