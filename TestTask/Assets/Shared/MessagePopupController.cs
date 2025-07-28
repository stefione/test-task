using PixPlays.Framework.Events;
using System;
using UnityEngine;
namespace TestTask.MagicWords
{
    public class ShowMessagePopupEvent
    {
        public string Title;
        public string Message;
        public Action OnClose;
    }

    public class MessagePopupController : MonoBehaviour
    {
        [SerializeField] private MessagePopup _MessagePopupPrefab;
        [SerializeField] private Transform _Container;

        private void Awake()
        {
            EventManager.Subscribe<ShowMessagePopupEvent>(x => ProcessShowMessagePopupEvent(x));
        }

        private void ProcessShowMessagePopupEvent(ShowMessagePopupEvent x)
        {
            var messagePopup = Instantiate(_MessagePopupPrefab, _Container);
            messagePopup.Open(x.Title, x.Message,
                () =>
                {
                    x.OnClose?.Invoke();
                    Destroy(messagePopup);
                });
        }
    }
}
