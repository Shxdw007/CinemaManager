using CinemaManager.Api;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CinemaManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadRealDataAsync(); // Вызываем загрузку реальных данных
        }

        // Асинхронный метод для получения фильмов из БД через API
        private async void LoadRealDataAsync()
        {
            try
            {
                // Делаем GET-запрос и сразу парсим JSON в список объектов Movie
                var movies = await ApiClient.Http.GetFromJsonAsync<List<Movie>>("api/Movies");

                if (movies != null)
                {
                    // Отдаем данные в таблицу
                    MoviesGrid.ItemsSource = movies;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к серверу: {ex.Message}\nУбедитесь, что API запущен!");
            }
        }

        // --- Обработчики кнопок меню оставляем как есть ---
        private void BtnAddMovie_Click(object sender, RoutedEventArgs e)
        {
            AddMovieWindow addMovieWin = new AddMovieWindow();
            addMovieWin.ShowDialog();
            LoadRealDataAsync();
        }

        private void BtnEditMovie_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: Movie movie })
                return;

            var win = new AddMovieWindow(movie);
            win.ShowDialog();
            LoadRealDataAsync();
        }

        private async void BtnDeleteMovie_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: Movie movie })
                return;

            var confirm = MessageBox.Show($"Удалить фильм «{movie.Title}»?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                var resp = await ApiClient.Http.DeleteAsync($"api/movies/{movie.Id}");
                if (!resp.IsSuccessStatusCode)
                {
                    var text = await resp.Content.ReadAsStringAsync();
                    MessageBox.Show($"Не удалось удалить фильм: {(int)resp.StatusCode} {resp.ReasonPhrase}\n{text}");
                    return;
                }

                LoadRealDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        private void BtnSchedule_Click(object sender, RoutedEventArgs e)
        {
            ScheduleWindow scheduleWin = new ScheduleWindow();
            scheduleWin.Owner = this;
            scheduleWin.Show();
            Hide();
        }

        private void BtnStats_Click(object sender, RoutedEventArgs e)
        {
            StatsWindow statsWin = new StatsWindow();
            statsWin.Owner = this;
            statsWin.Show();
            Hide();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HallsWindow hallsWin = new HallsWindow();
            hallsWin.Owner = this;
            hallsWin.Show();
            Hide();
        }

        private void BtnExitApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }

    // Структура фильма (свойства должны совпадать с тем, что отдает API)
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public int Duration { get; set; } // В API это число
        public string AgeRating { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public bool IsComingSoon { get; set; }
    }
}