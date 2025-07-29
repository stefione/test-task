using JetBrains.Annotations;
using Newtonsoft.Json;
using PixPlays.Framework.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TestTask.MagicWords
{
    public class MagicWordsSceneController : MonoBehaviour
    {
        [SerializeField] private string _Url;
        [SerializeField] private int _Timeout;

        private RemoteData _remoteData;
        private Downloader _downloader;
        private HashSet<AvatarData> _textureDownloadFinishSet = new();
        private Dictionary<string, Texture2D> _avatarTextures = new();

        public event Action<RemoteData, Dictionary<string, Texture2D>> OnRemoteDataDownloadFinishEvent;
        public event Action<float, bool> OnDownloadUpdateEvent; //progress, additive

        private void Start()
        {
            _downloader = new(_Timeout);
            _textureDownloadFinishSet.Clear();
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
            _avatarTextures.Clear();
            _remoteData = JsonConvert.DeserializeObject<RemoteData>(result);
            
            if (!_remoteData.IsValid(HandleError))
            {
                return;
            }

            foreach (var avatar in _remoteData.avatars)
            {
                _ = _downloader.DownloadImageAsync(avatar.url,
                    result =>
                    {
                        OnTextureLoadingFinish(avatar, result);
                    },
                    error =>
                    {
                        HandleError(error);

                        //Move on even if failed to download image. Can be removed to block the game if error is displayed
                        OnTextureLoadingFinish(avatar, null);
                    });
            }
        }

        private void OnTextureLoadingFinish(AvatarData avatar, [CanBeNull] Texture2D texture)
        {
            bool success = _textureDownloadFinishSet.Add(avatar);
            
            if (!success)
            {
                return;
            }

            if (texture != null)
            {
                _avatarTextures.TryAdd(avatar.name, texture);
            }

            float progress = (float)_textureDownloadFinishSet.Count / (float)_remoteData.avatars.Count;
            OnDownloadUpdateEvent(progress, false);

            if (_textureDownloadFinishSet.Count == _remoteData.avatars.Count)
            {
                OnRemoteDataDownloadFinishEvent?.Invoke(_remoteData, _avatarTextures);
            }
        }
    }
}
