using Newtonsoft.Json;
using PixPlays.Framework.Events;
using System;
using System.Collections;
using UnityEngine;

namespace TestTask.MagicWords
{
    public class MagicWordsSceneController : MonoBehaviour
    {
        [SerializeField] private string _Url;
        [SerializeField] private bool _PreDownload;
        [SerializeField] private int _Timeout;

        private RemoteData _remoteData;
        private Downloader _downloader;
        private int _textureDownloadCount;

        public event Action<RemoteData, Downloader> OnRemoteDataDownloadFinishEvent;
        public event Action<float, bool> OnDownloadUpdateEvent; //progress, additive

        private void Start()
        {
            _downloader = new(_Timeout);
            _textureDownloadCount = 0;
            _ = _downloader.DownloadTextAsync(_Url, OnRemoteDownloadFinish, HandleError,
                progress =>
                {
                    OnDownloadUpdateEvent(progress, false);
                });
        }

        private void HandleError(string error)
        {
            EventManager.Fire(new ShowMessagePopupEvent()
            {
                Title = "Error",
                Message = error
            });
        }

        private void OnRemoteDownloadFinish(string result)
        {
            _remoteData = JsonConvert.DeserializeObject<RemoteData>(result);
            if (_PreDownload)
            {
                foreach (var avatar in _remoteData.avatars)
                {
                    _ = _downloader.DownloadImageAsync(avatar.url, result => OnTextureLoadingFinish(), error =>
                      {
                          HandleError(error);
                          OnTextureLoadingFinish();
                      },
                    progress =>
                    {
                        float part = 1f / (float)_remoteData.avatars.Count;
                        OnDownloadUpdateEvent(part * progress, true);
                    });
                }
            }
            else
            {
                OnRemoteDataDownloadFinishEvent?.Invoke(_remoteData, _downloader);
            }
        }

        private void OnTextureLoadingFinish()
        {
            _textureDownloadCount++;

            float progress = (float)_textureDownloadCount / (float)_remoteData.avatars.Count;
            OnDownloadUpdateEvent(progress, false);

            if (_textureDownloadCount == _remoteData.avatars.Count)
            {
                OnRemoteDataDownloadFinishEvent?.Invoke(_remoteData, _downloader);
            }
        }
    }
}
