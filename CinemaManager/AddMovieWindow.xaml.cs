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
        private readonly Movie? _editMovie;

        public AddMovieWindow()
        {
            InitializeComponent();
        }

        public AddMovieWindow(Movie movieToEdit) : this()
        {
            _editMovie = movieToEdit;

            TitleTextBox.Text = movieToEdit.Title;
            DescriptionTextBox.Text = movieToEdit.Description;
            DurationTextBox.Text = movieToEdit.Duration.ToString();

            SelectComboItemByText(GenreComboBox, movieToEdit.Genre);
            SelectComboItemByText(AgeRatingComboBox, movieToEdit.AgeRating);
            IsComingSoonCheckBox.IsChecked = movieToEdit.IsComingSoon;
        }

        private static void SelectComboItemByText(ComboBox comboBox, string value)
        {
            for (var i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i] is ComboBoxItem item &&
                    string.Equals(item.Content?.ToString(), value, StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedIndex = i;
                    return;
                }
            }
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
                var isComingSoon = IsComingSoonCheckBox.IsChecked == true;

                if (_editMovie is not null)
                {
                    // Режим редактирования: делаем PUT (постер не меняем здесь).
                    var payload = new
                    {
                        title,
                        description,
                        genre,
                        duration,
                        ageRating,
                        director = "",
                        isComingSoon
                    };

                    var response = await ApiClient.Http.PutAsJsonAsync($"api/movies/{_editMovie.Id}", payload);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Фильм успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close();
                        return;
                    }

                    var serverText = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка при обновлении фильма.\n{(int)response.StatusCode} {response.ReasonPhrase}\n{serverText}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Режим добавления: POST multipart с файлом (если выбран)
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(title), "Title");
                content.Add(new StringContent(description), "Description");
                content.Add(new StringContent(genre), "Genre");
                content.Add(new StringContent(ageRating), "AgeRating");
                content.Add(new StringContent(duration.ToString()), "Duration");
                content.Add(new StringContent(""), "Director");
                content.Add(new StringContent(isComingSoon ? "true" : "false"), "IsComingSoon");

                if (!string.IsNullOrWhiteSpace(_posterFilePath) && File.Exists(_posterFilePath))
                {
                    var bytes = await File.ReadAllBytesAsync(_posterFilePath);
                    var fileContent = new ByteArrayContent(bytes);

                    var ext = Path.GetExtension(_posterFilePath).ToLowerInvariant();
                    fileContent.Headers.ContentType = ext switch
                    {
                        ".png" => new MediaTypeHeaderValue("image/png"),
                        ".webp" => new MediaTypeHeaderValue("image/webp"),
                        _ => new MediaTypeHeaderValue("image/jpeg"),
                    };

                    content.Add(fileContent, "Poster", Path.GetFileName(_posterFilePath));
                }

                var createResponse = await ApiClient.Http.PostAsync("api/movies", content);

                if (createResponse.IsSuccessStatusCode)
                {
                    MessageBox.Show("Фильм успешно добавлен в базу данных!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close(); // Закрываем окно добавления
                }
                else
                {
                    var serverText = await createResponse.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка при добавлении фильма на сервер.\n{(int)createResponse.StatusCode} {createResponse.ReasonPhrase}\n{serverText}",
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