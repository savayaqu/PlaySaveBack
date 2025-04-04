using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using PSB.Api.Request;
using PSB.Api.Request.GoogleDrive;
using PSB.Api.Response.GoogleDrive;
using PSB.Helpers;
using PSB.Interfaces;
using PSB.Models;
using PSB.Services;
using Windows.Gaming.Input;
using static PSB.Utils.Fetch;

namespace PSB.Utils
{
    public class CloudFileUploader
    {
        public static async Task<(bool Success, Save? UpdatedSave)> UploadFileAsync(
            Save save,
            CloudService cloudService,
            IGame game,
            string version,
            string description)
        {
            try
            {
                return cloudService.Name switch
                {
                    "Google Drive" => await UploadToGoogleDriveAsync(save, game, version, description),
                    //"Dropbox" => await UploadToDropboxAsync(save, game, version, description),
                    //"OneDrive" => await UploadToOneDriveAsync(save, game, version, description),
                    _ => throw new NotSupportedException($"Service {cloudService.Name} not supported")
                };
            }
            catch (Exception ex)
            {
                NotificationService.ShowError($"Upload failed: {ex.Message}");
                return (false, null);
            }
        }

        private static async Task<(bool Success, Save? UpdatedSave)> UploadToGoogleDriveAsync(
            Save save,
            IGame game,
            string version,
            string description)
        {
            // 1. Get upload URL
            (var resUploadUrl, var bodyUploadUrl) = await 
                FetchAsync<UploadUrlResponse>(
                    HttpMethod.Post,
                    "google-drive/generate-upload-url",
                    new
            {
                file_name = Path.GetFileName(save.ZipPath),
                file_size = new FileInfo(save.ZipPath).Length,
                version,
                description,
                game_id = game is SideGame ? null : game.Id.ToString(),
                side_game_id = game is SideGame ? game.Id.ToString() : null
            }, true);

            if (!resUploadUrl.IsSuccessStatusCode)
                return (false, null);


            // 2. Direct upload to Google Drive
            using var fileStream = File.OpenRead(save.ZipPath);
            (var fileIdResponse, var fileIdBody) = await FetchAsync<FileIdResponse>(HttpMethod.Post,bodyUploadUrl.UploadUrl, new StreamContent(fileStream));

            if (!fileIdResponse.IsSuccessStatusCode)
                return (false, null);

            // 3. Confirm upload
            var fileHash = ZipHelper.CalculateFileHash(save.ZipPath);
            (var confirmResponse, var confirmBody) = await
                FetchAsync<Save>(
                HttpMethod.Post,
                $"google-drive/confirm-upload/{bodyUploadUrl.SaveId}",
                new ConfirmUploadRequest(fileIdBody!.Id, fileHash), true);

            return (true, confirmBody);
        }
        public static async Task<(bool Success, Save? UpdatedSave)> OverwriteFileAsync(Save existingSave,string newFilePath,string version,string description)
        {
            try
            {
                // 1. Получаем URL для перезаписи
                var (urlResponse, urlBody) = await FetchAsync<OverwriteUrlResponse>(
                    HttpMethod.Post,
                    $"saves/{existingSave.Id}/google-drive/generate-overwrite-url",
                    new
                    {
                        file_name = Path.GetFileName(newFilePath),
                        file_size = new FileInfo(newFilePath).Length
                    },
                    true);

                if (!urlResponse.IsSuccessStatusCode || urlBody == null)
                    return (false, null);

                // 2. Загружаем новый файл
                using var fileStream = File.OpenRead(newFilePath);
                var (uploadResponse, _) = await FetchAsync<object>(
                    HttpMethod.Put,
                    urlBody.UploadUrl,
                    new StreamContent(fileStream));

                if (!uploadResponse.IsSuccessStatusCode)
                    return (false, null);

                // 3. Обновляем метаданные
                var (updateResponse, updatedSave) = await FetchAsync<Save>(
                    HttpMethod.Patch,
                    $"saves/{existingSave.Id}",
                    new UpdateSaveRequest(version, description, ZipHelper.CalculateFileHash(newFilePath), DateTime.Now),true);

                return (updateResponse.IsSuccessStatusCode, updatedSave);
            }
            catch (Exception ex)
            {
                NotificationService.ShowError($"Overwrite failed: {ex.Message}");
                return (false, null);
            }
        }
    }

}
