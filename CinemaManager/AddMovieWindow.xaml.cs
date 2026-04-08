using CinemaManager.Api;
using System;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CinemaManager
{
    public partial class AddMovieWindow : Window
    {
        public AddMovieWindow()
        {
            InitializeComponent();
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Собираем данные из полей ввода
                var newMovie = new
                {
                    title = TitleTextBox.Text,
                    genre = (GenreComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    ageRating = (AgeRatingComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    duration = int.TryParse(DurationTextBox.Text, out int d) ? d : 120, // Защита от ввода букв вместо цифр
                    description = DescriptionTextBox.Text,
                    posterData = "poster.jpg"
                };

                var response = await ApiClient.Http.PostAsJsonAsync("api/Movies", newMovie);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Фильм успешно добавлен в базу данных!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close(); // Закрываем окно добавления
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении фильма на сервер.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Сетевая ошибка: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}