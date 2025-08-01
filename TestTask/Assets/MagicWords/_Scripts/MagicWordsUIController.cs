using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TestTask.MagicWords
{
    public class MagicWordsUIController : MonoBehaviour
    {
        [SerializeField] private MagicWordsSceneController _SceneController;
        [SerializeField] private DialogController _DialogController;
        [SerializeField] private Slider _Slider;

        private void Awake()
        {
            _DialogController.Close();
            _SceneController.OnRemoteDataDownloadFinishEvent += _SceneController_OnRemoteDataDownloadFinishEvent;
            _SceneController.OnDownloadUpdateEvent += _SceneController_OnDownloadUpdateEvent;
            _Slider.gameObject.SetActive(true);
        }

        private void _SceneController_OnDownloadUpdateEvent(float progress, bool additive)
        {
            if (additive)
            {
                _Slider.value += progress;
            }
            else
            {
                _Slider.value = progress;
            }
        }

        private void _SceneController_OnRemoteDataDownloadFinishEvent(RemoteData remoteData, Dictionary<string, Texture2D> textures)
        {
            _DialogController.SetData(remoteData, textures);
            _Slider.gameObject.SetActive(false);
        }
    }
}