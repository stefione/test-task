using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TestTask
{
    public class MainSceneController : MonoBehaviour
    {
        [SerializeField] private Button _AceOfShadowsButton;
        [SerializeField] private Button _MagicWordsButton;
        [SerializeField] private Button _PhoenixFlameButton;

        private void Awake()
        {
            _AceOfShadowsButton.onClick.AddListener(OnAceOfShadowsButtonClick);
            _MagicWordsButton.onClick.AddListener(OnMagicWordsButtonClick);
            _PhoenixFlameButton.onClick.AddListener(OnPhoenixFlameButtonClick);
        }

        private void OnPhoenixFlameButtonClick()
        {
            SceneManager.LoadScene(Scenes.PhoenixFlameScene);
        }

        private void OnMagicWordsButtonClick()
        {
            SceneManager.LoadScene(Scenes.MagicWordsScene);
        }

        private void OnAceOfShadowsButtonClick()
        {
            SceneManager.LoadScene(Scenes.AceOfShadowsScene);
        }
    }
}