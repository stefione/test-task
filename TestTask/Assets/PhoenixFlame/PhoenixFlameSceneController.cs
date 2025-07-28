using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestTask.PhoenixFlame
{
    public class PhoenixFlameSceneController : MonoBehaviour
    {
        private const string _ActiveAnimText = "Stop Animation";
        private const string _InactiveAnimText = "Start Animation";

        [SerializeField] private Button _AnimationButton;
        [SerializeField] private TMP_Text _ButtonText;
        [SerializeField] private Animator _EffectAnim;
        [SerializeField] private string _AnimStateName;
        private bool _isAnimationPlaying;

        private void Awake()
        {
            _AnimationButton.onClick.AddListener(OnAnimationButtonClick);
        }

        private void OnAnimationButtonClick()
        {
            if (!_isAnimationPlaying)
            {
                _EffectAnim.enabled = true;
                _EffectAnim.Play(_AnimStateName);
                _ButtonText.text = _ActiveAnimText;
            }
            else
            {
                _ButtonText.text = _InactiveAnimText;
                _EffectAnim.enabled = false;
            }
            _isAnimationPlaying = !_isAnimationPlaying;
        }
    }
}
