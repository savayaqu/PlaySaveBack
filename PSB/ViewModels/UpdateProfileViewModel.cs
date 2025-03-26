using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using PSB.Models;
using PSB.Utils;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using static PSB.Utils.Fetch;

namespace PSB.ViewModels
{
    public partial class UpdateProfileViewModel : ObservableObject
    {
        [ObservableProperty] public partial string NickName { get; set; } = AuthData.User!.Nickname;
        [ObservableProperty] public partial ProfileViewModel ProfileViewModel { get; set; } = App.MainWindow!.ProfileViewModel;
        [ObservableProperty] public partial string? AvatarUrl { get; set; }
        [ObservableProperty] public partial string? HeaderUrl { get; set; }
        [ObservableProperty] public partial bool IsAvatarUrlInputVisible { get; set; } = false;
        [ObservableProperty] public partial bool IsHeaderUrlInputVisible { get; set; } = false;
        [ObservableProperty] public partial BitmapImage? AvatarImage { get; set; }
        [ObservableProperty] public partial BitmapImage? HeaderImage { get; set; }
        [ObservableProperty] public partial StorageFile? AvatarFile { get; set; }
        [ObservableProperty] public partial StorageFile? HeaderFile { get; set; }

        public BitmapImage CurrentAvatar
        {
            get
            {
                if (AvatarImage != null)
                    return AvatarImage;

                return !string.IsNullOrEmpty(ProfileViewModel.User.Avatar)
                    ? new BitmapImage(new Uri(ProfileViewModel.User.Avatar))
                    : new BitmapImage();
            }
        }

        public BitmapImage CurrentHeader
        {
            get
            {
                if (HeaderImage != null)
                    return HeaderImage;

                return !string.IsNullOrEmpty(ProfileViewModel.User.Header)
                    ? new BitmapImage(new Uri(ProfileViewModel.User.Header))
                    : new BitmapImage();
            }
        }

        [RelayCommand]
        public async Task UpdateProfile()
        {
            try
            {
                using var formData = new MultipartFormDataContent();

                // Добавляем nickname
                if (!string.IsNullOrEmpty(NickName) && NickName != ProfileViewModel.User.Nickname)
                {
                    formData.Add(new StringContent(NickName), "nickname");
                }

                // Обработка аватара
                if (IsAvatarUrlInputVisible && !string.IsNullOrEmpty(AvatarUrl))
                {
                    formData.Add(new StringContent(AvatarUrl), "avatar");
                }
                else if (AvatarFile != null)
                {
                    var fileStream = await AvatarFile.OpenStreamForReadAsync();
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/" + AvatarFile.FileType.Replace(".", ""));
                    formData.Add(fileContent, "avatar_file", AvatarFile.Name);
                }

                // Обработка хедера
                if (IsHeaderUrlInputVisible && !string.IsNullOrEmpty(HeaderUrl))
                {
                    formData.Add(new StringContent(HeaderUrl), "header");
                }
                else if (HeaderFile != null)
                {
                    var fileStream = await HeaderFile.OpenStreamForReadAsync();
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/" + HeaderFile.FileType.Replace(".", ""));
                    formData.Add(fileContent, "header_file", HeaderFile.Name);
                }

                // Отладочный вывод
                Debug.WriteLine("FormData contents:");
                foreach (var content in formData)
                {
                    Debug.WriteLine($"- Headers: {string.Join(", ", content.Headers)}");
                    if (content is StringContent stringContent)
                    {
                        Debug.WriteLine($"  String value: {await stringContent.ReadAsStringAsync()}");
                    }
                    else if (content is StreamContent streamContent)
                    {
                        Debug.WriteLine($"  Stream content: {content.Headers.ContentDisposition?.Name} ({content.Headers.ContentType})");
                    }
                }

                (var res, var body) = await FetchAsync<User>(HttpMethod.Post, "profile", body: formData);
                // Обработка ответа...
                if(res.IsSuccessStatusCode)
                {
                    AuthData.User = body;
                    ProfileViewModel.User = AuthData.User;
                    OnPropertyChanged(nameof(ProfileViewModel));
                    App.DialogService!.HideDialog();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in UpdateProfile: {ex.Message}");
            }
        }

        [RelayCommand]
        public async Task PickAvatarFile()
        {
            IsAvatarUrlInputVisible = false;
            var file = await PickImageFileAsync();
            if (file != null)
            {
                await LoadImageFromFile(file, isAvatar: true);
            }
        }

        [RelayCommand]
        public async Task PickHeaderFile()
        {
            IsHeaderUrlInputVisible = false;
            var file = await PickImageFileAsync();
            if (file != null)
            {
                await LoadImageFromFile(file, isAvatar: false);
            }
        }

        [RelayCommand]
        public void ShowAvatarUrlInput() => IsAvatarUrlInputVisible = true;

        [RelayCommand]
        public void ShowHeaderUrlInput() => IsHeaderUrlInputVisible = true;

        private async Task<StorageFile?> PickImageFileAsync()
        {
            var openPicker = new FileOpenPicker();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".gif");
            openPicker.FileTypeFilter.Add(".svg");

            return await openPicker.PickSingleFileAsync();
        }

        private async Task LoadImageFromFile(StorageFile file, bool isAvatar)
        {
            try
            {
                using var stream = await file.OpenAsync(FileAccessMode.Read);
                var bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(stream);

                if (isAvatar)
                {
                    AvatarImage = bitmap;
                    AvatarFile = file; // Сохраняем файл
                    AvatarUrl = null;
                    OnPropertyChanged(nameof(CurrentAvatar));
                }
                else
                {
                    HeaderImage = bitmap;
                    HeaderFile = file; // Сохраняем файл
                    HeaderUrl = null;
                    OnPropertyChanged(nameof(CurrentHeader));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading image: {ex.Message}");
            }
        }

        [RelayCommand]
        public void ConfirmAvatarUrl()
        {
            if (!string.IsNullOrEmpty(AvatarUrl))
            {
                try
                {
                    AvatarImage = new BitmapImage(new Uri(AvatarUrl));
                    OnPropertyChanged(nameof(CurrentAvatar));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading avatar from URL: {ex.Message}");
                    AvatarImage = null;
                    OnPropertyChanged(nameof(CurrentAvatar));
                    // Здесь можно добавить уведомление об ошибке
                }
            }
        }

        [RelayCommand]
        public void ConfirmHeaderUrl()
        {
            if (!string.IsNullOrEmpty(HeaderUrl))
            {
                try
                {
                    HeaderImage = new BitmapImage(new Uri(HeaderUrl));
                    OnPropertyChanged(nameof(CurrentHeader));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading header from URL: {ex.Message}");
                    HeaderImage = null;
                    OnPropertyChanged(nameof(CurrentHeader));
                    // Здесь можно добавить уведомление об ошибке
                }
            }
        }
    }
}
