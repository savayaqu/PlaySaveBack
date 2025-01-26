using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using playsaveback.Services;
using System.Windows;
using System.Text.Json;
using playsaveback.Models;
using System.Diagnostics;

namespace playsaveback.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        private readonly ApiService _apiService;

        public LoginViewModel()
        {
            _apiService = new ApiService();
        }

        [RelayCommand]
        public async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var loginData = new
            {
                email = Email,
                password = Password
            };

            try
            {
                var response = await _apiService.PostAsync("api/login", loginData);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonSerializer.Deserialize<LoginResponse>(responseData);

                    // Сохраняем токен через ApiService
                    _apiService.SetAuthToken(jsonResponse.Token);
                    Console.WriteLine(jsonResponse.Token);
                    MessageBox.Show("Авторизация прошла успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Логика после успешной авторизации

                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка авторизации: {errorMessage}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка соединения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
