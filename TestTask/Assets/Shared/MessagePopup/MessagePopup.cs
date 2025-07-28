using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestTask
{
    public class MessagePopup : MonoBehaviour
    {
        [SerializeField] private TMP_Text _Title;
        [SerializeField] private TMP_Text _Message;
        [SerializeField] private Transform _PanelTransform;
        [SerializeField] private float _AnimSpeed;
        [SerializeField] private Button _CloseButton;
        [SerializeField] private Button _BackgroundButton;

        private Action _onClose;
        private void Awake()
        {
            _CloseButton.onClick.AddListener(OnCloseButtonClick);
            _BackgroundButton.onClick.AddListener(OnCloseButtonClick);
        }

        private void OnCloseButtonClick()
        {
            Close();
        }

        public void Open(string title, string message, Action onClose)
        {
            _onClose = onClose;
            gameObject.SetActive(true);
            _Title.text = title;
            _Message.text = message;
            _PanelTransform.transform.localScale = Vector3.zero;
            _PanelTransform.DOScale(Vector3.one, _AnimSpeed).SetEase(Ease.OutBack);
        }

        public void Close()
        {
            _PanelTransform.DOScale(Vector3.zero, _AnimSpeed).SetEase(Ease.InBack).OnComplete(() =>
            {
                gameObject.SetActive(false);
                _onClose?.Invoke();
            });
        }
    }
}