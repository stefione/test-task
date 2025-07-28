using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TestTask.MagicWords
{
    public class Downloader
    {
        private Dictionary<string, List<Action<Texture2D>>> _pendingDownloadTextureActions = new();
        private Dictionary<string, Texture2D> _textureCache = new();

        private int _timeout = 3;

        public Downloader(int timeout)
        {
            _timeout = timeout;
        }

        public async void DownloadImageAsync(string url, Action<Texture2D> onTextureDownloaded, Action<string> onError, Action<float> onProgressUpdate = null)
        {
            try
            {
                if (_textureCache.TryGetValue(url, out var tex))
                {
                    onTextureDownloaded?.Invoke(tex);
                    return;
                }

                if (_pendingDownloadTextureActions.TryGetValue(url, out var pendingDownloadActions))
                {
                    pendingDownloadActions.Add(onTextureDownloaded);
                    return;
                }

                using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
                {
                    uwr.timeout = _timeout;
                    var operation = uwr.SendWebRequest();
                    var downloadActions = new List<Action<Texture2D>>() { onTextureDownloaded };
                    _pendingDownloadTextureActions.Add(url, downloadActions);

                    while (!operation.isDone && Application.isPlaying)
                    {
                        onProgressUpdate?.Invoke(operation.webRequest.downloadProgress);
                        await Task.Yield();
                    }

                    if (!Application.isPlaying)
                    {
                        return;
                    }

                    if (operation.webRequest.result != UnityWebRequest.Result.Success)
                    {
                        string error = $"Failed to download image at {url}: " + uwr.error;
                        Error(url, onError, error);
                        return;
                    }

                    Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                    _textureCache.Add(url, texture);
                    foreach (var action in downloadActions)
                    {
                        action?.Invoke(texture);
                    }
                    _pendingDownloadTextureActions.Remove(url);
                }
            }
            catch (System.Exception ex)
            {
                string exceptionMsg = $"Exception for {url}: {ex.Message}";
                Error(url, onError, exceptionMsg);
                throw ex;
            }
        }

        public async void DownloadTextAsync(string url, Action<string> onDataDownloaded, Action<string> onError, Action<float> onProgressUpdate = null)
        {
            try
            {
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.timeout = _timeout;
                    var operation = request.SendWebRequest();

                    while (!operation.isDone && Application.isPlaying)
                    {
                        onProgressUpdate?.Invoke(operation.webRequest.downloadProgress);
                        await Task.Yield();
                    }

                    if (!Application.isPlaying)
                    {
                        return;
                    }

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        string error = $"Request Error: {request.error}";
                        Error(url, onError, error);
                        return;
                    }

                    onDataDownloaded?.Invoke(request.downloadHandler.text);
                }
            }
            catch (System.Exception ex)
            {
                string exceptionMsg = $"Exception: {ex.Message}";
                Error(url, onError, exceptionMsg);
                throw ex;
            }
        }

        private void Error(string id, Action<string> onError, string error)
        {
            _pendingDownloadTextureActions.Remove(id);
            Debug.LogError(error);
            onError?.Invoke(error);
        }
    }
}
