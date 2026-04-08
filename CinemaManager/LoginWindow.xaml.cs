using CinemaManager.Api;
using CinemaManager.Api.Contracts;
using System;
using System.Net;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Input;

namespace CinemaManager
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var email = EmailTextBox.Text?.Trim() ?? string.Empty;
                var password = PasswordBox.Password ?? string.Empty;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Введите email и пароль.");
                    return;
                }

                var resp = await ApiClient.Http.PostAsJsonAsync("api/users/login", new LoginRequest
                {
                    Email = email,
                    Password = password
                });

                if (resp.StatusCode == HttpStatusCode.Unauthorized)
                {
                    MessageBox.Show("Неверный логин или пароль.");
                    return;
                }

                resp.EnsureSuccessStatusCode();

                var data = await resp.Content.ReadFromJsonAsync<LoginResponse>();
                if (data is null || string.IsNullOrWhiteSpace(data.Token))
                {
                    MessageBox.Show("Сервер вернул пустой ответ.");
                    return;
                }

                ApiClient.SetBearerToken(data.Token);
                AppSession.User = new CurrentUser { Id = data.User.Id, Email = data.User.Email, Role = data.User.Role };

                var main = new MainWindow();
                Application.Current.MainWindow = main;
                main.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка входа: {ex.Message}");
            }
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
}
