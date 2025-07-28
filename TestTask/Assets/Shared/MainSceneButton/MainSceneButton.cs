using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace TestTask
{
    public class MainSceneButton : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            SceneManager.LoadScene(Scenes.MainScene);
        }
    }
}
