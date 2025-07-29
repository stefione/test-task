using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TestTask
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

        public async Task DownloadImageAsync(string url, Action<Texture2D> onTextureDownloaded, Action<string> onError, Action<float> onProgressUpdate = null)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    string error = "URL is null or empty";
                    Error(url, onError, error);
                    return;
                }

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

                    bool success = await HandleRequest(operation, url, onProgressUpdate, onError);

                    if (!success)
                    {
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

        public async Task DownloadTextAsync(string url, Action<string> onDataDownloaded, Action<string> onError, Action<float> onProgressUpdate = null)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    string error = "URL is null or empty";
                    Error(url, onError, error);
                    return;
                }

                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.timeout = _timeout;
                    var operation = request.SendWebRequest();

                    bool success = await HandleRequest(operation, url, onProgressUpdate, onError);

                    if (success)
                    {
                        onDataDownloaded?.Invoke(request.downloadHandler.text);
                    }
                }
            }
            catch (System.Exception ex)
            {
                string exceptionMsg = $"Exception: {ex.Message}";
                Error(url, onError, exceptionMsg);
                throw ex;
            }
        }

        private async Task<bool> HandleRequest(UnityWebRequestAsyncOperation operation, string url, Action<float> onProgressUpdate, Action<string> onError)
        {
            float time = _timeout;

            while (!operation.isDone && Application.isPlaying && time > 0)
            {
                time -= Time.deltaTime;
                onProgressUpdate?.Invoke(operation.webRequest.downloadProgress);
                await Task.Yield();
            }

            if (!Application.isPlaying)
            {
                return false;
            }

            if (!operation.isDone)
            {
                operation.webRequest.Abort();
                string error = $"Request timeout {url}: Aborted";
                Error(url, onError, error);
                return false;
            }

            if (operation.webRequest.result != UnityWebRequest.Result.Success)
            {
                string error = $"Failed to download data from {url}: " + operation.webRequest.error;
                Error(url, onError, error);
                return false;
            }

            return true;
        }

        private void Error(string id, Action<string> onError, string error)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _pendingDownloadTextureActions.Remove(id);
            }

            Debug.LogError(error);
            onError?.Invoke(error);
        }
    }
}
