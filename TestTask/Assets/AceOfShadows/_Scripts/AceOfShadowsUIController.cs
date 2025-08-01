using PixPlays.Framework.Events;
using System;
using TestTask.MagicWords;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestTask.AceOfShadows
{
    public class AceOfShadowsUIController : MonoBehaviour
    {
        [SerializeField] private RectTransform _CanvasRect;
        [SerializeField] private TMP_Text _FirstStackCounterText;
        [SerializeField] private TMP_Text _SecondStackCounterText;
        [SerializeField] private Vector2 _CounterOffset;
        [SerializeField] private AceOfShadowsSceneController _SceneController;
        [SerializeField] private Button _StartButton;
        [SerializeField] private TMP_InputField _AnimationDurationInput;

        private Camera _cam;

        private void Awake()
        {
            _cam = Camera.main;

            _SceneController.FirstStackCountUpdatedEvent += OnFirstStackUpdatedEvent;
            _SceneController.SecondStackCountUpdatedEvent += OnSecondStackUpdatedEvent;
            _SceneController.FirstStackPositionUpdatedEvent += OnFirstStackPositionUpdatedEvent;
            _SceneController.SecondStackPositionUpdatedEvent += OnSecondStackPositionUpdatedEvent;
            _SceneController.AnimationFinishedEvent += OnAnimationFinishedEvent;
            _StartButton.onClick.AddListener(OnStartButtonClick);
            _AnimationDurationInput.onValueChanged.AddListener(OnAnimationDurationInputValueChange);
            _AnimationDurationInput.text = _SceneController.CardAnimationDuration.ToString("F2");

        }

        private void OnAnimationDurationInputValueChange(string stringValue)
        {
            if(!float.TryParse(stringValue, out float value))
            {
                return;
            }

            if (value <= 0)
            {
                return;
            }

            _SceneController.UpdateAnimationDuration(value);
        }

        private void OnStartButtonClick()
        {
            _SceneController.SetupAndStart();
        }

        private void OnAnimationFinishedEvent()
        {
            EventManager.Fire(new ShowMessagePopupEvent() { 
                Title="Finish",
                Message="All animations finished. Restarting",
                OnClose= _SceneController.SetupAndStart
            });
        }

        private Vector3 GetCanvasPoint(Vector3 worldPos, float zPos)
        {
            Vector3 screenPoint = _cam.WorldToScreenPoint(worldPos);
            float widthFactor = (float)Screen.width / _CanvasRect.rect.width;
            float heightFactor = (float)Screen.height / _CanvasRect.rect.height;
            return new Vector3(screenPoint.x , screenPoint.y, zPos)+new Vector3(_CounterOffset.x*widthFactor,_CounterOffset.y*heightFactor,0);
        }

        private void OnSecondStackPositionUpdatedEvent(Vector3 worldPos)
        {
            _SecondStackCounterText.transform.position = GetCanvasPoint(worldPos, _SecondStackCounterText.transform.position.z);
        }

        private void OnFirstStackPositionUpdatedEvent(Vector3 worldPos)
        {
            _FirstStackCounterText.transform.position = GetCanvasPoint(worldPos, _FirstStackCounterText.transform.position.z);
        }

        private void OnSecondStackUpdatedEvent(int obj)
        {
            _SecondStackCounterText.text = obj.ToString();
        }

        private void OnFirstStackUpdatedEvent(int obj)
        {
            _FirstStackCounterText.text = obj.ToString();
        }
    }
}