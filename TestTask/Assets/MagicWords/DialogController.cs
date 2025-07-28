using PixPlays.Framework.Events;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestTask.MagicWords
{
    public class DialogController : MonoBehaviour
    {
        [SerializeField] private TMP_Text _NameText;
        [SerializeField] private TMP_Text _DialogText;
        [SerializeField] private Image _AvatarLeft;
        [SerializeField] private Image _AvatarRight;
        [SerializeField] private Button _NextButton;
        [SerializeField] private Sprite _AvatarPlaceholder;

        private RemoteData _remoteData;

        private Dictionary<string, Sprite> _dialogSprites = new();
        private int _dialogIndex;
        private bool _avatarPositionSwitch;

        public event Action OnDialogFinishEvent;

        private void Awake()
        {
            _NextButton.onClick.AddListener(OnNextButtonClick);
        }

        public void SetData(RemoteData remoteData, Downloader downloader)
        {
            _remoteData = remoteData;
            CreateSprites(downloader);
            FormatTextForEmojis();

            _dialogIndex = 0;
            SetupDialogData(_remoteData.dialogue[_dialogIndex]);

            gameObject.SetActive(true);
        }

        private void FormatTextForEmojis()
        {
            foreach (var i in _remoteData.dialogue)
            {
                i.text = Regex.Replace(i.text, @"\{(.*?)\}", "<sprite name=$1>");
            }
        }

        private void CreateSprites(Downloader downloader)
        {
            foreach (var avatars in _remoteData.avatars)
            {
                downloader.DownloadImageAsync(avatars.url, result =>
                {
                    Sprite sprite = Sprite.Create(result, new Rect(0, 0, result.width, result.height), Vector2.one * 0.5f);
                    _dialogSprites.Add(avatars.GetId(), sprite);
                },
                error=> {
                    EventManager.Fire(new ShowMessagePopupEvent()
                    {
                        Title = "Error",
                        Message = error
                    });
                });
            }
        }

        private void SetupDialogData(DialogData dialogData)
        {
            _AvatarLeft.gameObject.SetActive(false);
            _AvatarRight.gameObject.SetActive(false);

            _NameText.text = dialogData.name;
            _DialogText.text = dialogData.text;

            if (!_avatarPositionSwitch)
            {
                _NameText.alignment = TextAlignmentOptions.MidlineLeft;
                _DialogText.alignment = TextAlignmentOptions.MidlineLeft;

                string id = dialogData.name + AvatarPosition.left.ToString();
                if (_dialogSprites.TryGetValue(id, out var sprite))
                {
                    _AvatarLeft.sprite = sprite;
                }
                else
                {
                    _AvatarLeft.sprite = _AvatarPlaceholder;
                }

                _AvatarLeft.gameObject.SetActive(true);
            }
            else
            {
                _NameText.alignment = TextAlignmentOptions.MidlineRight;
                _DialogText.alignment = TextAlignmentOptions.MidlineRight;

                string id = dialogData.name + AvatarPosition.right.ToString();
                if (_dialogSprites.TryGetValue(id, out var sprite))
                {
                    _AvatarRight.sprite = sprite;
                }
                else
                {
                    _AvatarRight.sprite = _AvatarPlaceholder;
                }

                _AvatarRight.gameObject.SetActive(true);
            }

            _avatarPositionSwitch = !_avatarPositionSwitch;
        }

        private void OnNextButtonClick()
        {
            if((_dialogIndex + 1) == _remoteData.dialogue.Count)
            {
                EventManager.Fire(new ShowMessagePopupEvent()
                {
                    Title="Dialog Finished",
                    Message="Dialog finished but will continue looping"
                });
                OnDialogFinishEvent?.Invoke();
            }

            _dialogIndex = (_dialogIndex + 1) % _remoteData.dialogue.Count;
            SetupDialogData(_remoteData.dialogue[_dialogIndex]);
        }

        private void CleanUp()
        {
            foreach (var sprite in _dialogSprites)
            {
                Destroy(sprite.Value);
            }
        }

        public void Close()
        {
            CleanUp();
            gameObject.SetActive(false);
        }
    }
}
