using CinemaManager.Api;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace CinemaManager
{
    public partial class AddMovieWindow : Window
    {
        private string? _posterFilePath;

        public AddMovieWindow()
        {
            InitializeComponent();
        }

        private void BtnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Выберите постер",
                Filter = "Изображения (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp|Все файлы (*.*)|*.*",
                Multiselect = false
            };

            if (dlg.ShowDialog() != true)
                return;

            _posterFilePath = dlg.FileName;

            // Загружаем превью без блокировки файла на диске
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(_posterFilePath, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();

            PosterPreview.Source = bitmap;
            PosterPlaceholder.Visibility = Visibility.Collapsed;
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var title = TitleTextBox.Text?.Trim() ?? "";
                var genre = (GenreComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                var ageRating = (AgeRatingComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                var duration = int.TryParse(DurationTextBox.Text, out var d) ? d : 120;
                var description = DescriptionTextBox.Text ?? "";

                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(title), "Title");
                content.Add(new StringContent(description), "Description");
                content.Add(new StringContent(genre), "Genre");
                content.Add(new StringContent(ageRating), "AgeRating");
                content.Add(new StringContent(duration.ToString()), "Duration");
                content.Add(new StringContent(""), "Director");

                if (!string.IsNullOrWhiteSpace(_posterFilePath) && File.Exists(_posterFilePath))
                {
                    var bytes = await File.ReadAllBytesAsync(_posterFilePath);
                    var fileContent = new ByteArrayContent(bytes);

                    // API отдает image/jpeg; на прием тип не критичен, но заголовок полезен
                    var ext = Path.GetExtension(_posterFilePath).ToLowerInvariant();
                    fileContent.Headers.ContentType = ext switch
                    {
                        ".png" => new MediaTypeHeaderValue("image/png"),
                        ".webp" => new MediaTypeHeaderValue("image/webp"),
                        _ => new MediaTypeHeaderValue("image/jpeg"),
                    };

                    content.Add(fileContent, "Poster", Path.GetFileName(_posterFilePath));
                }

                var response = await ApiClient.Http.PostAsync("api/movies", content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Фильм успешно добавлен в базу данных!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close(); // Закрываем окно добавления
                }
                else
                {
                    var serverText = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка при добавлении фильма на сервер.\n{(int)response.StatusCode} {response.ReasonPhrase}\n{serverText}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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