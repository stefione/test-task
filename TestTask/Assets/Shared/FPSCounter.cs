using TMPro;
using UnityEngine;
namespace TestTask
{
    public class FPSCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text _FpsText;
        [SerializeField] private int PrintEveryXFrames = 60;
        private float _totalTime;
        private int _totalFrames;

        private void Update()
        {
            _totalFrames++;
            _totalTime += Time.deltaTime;
            if (_totalFrames >= PrintEveryXFrames)
            {
                if (_totalTime > 0)
                {
                    float fps = _totalFrames / _totalTime;
                    _FpsText.text = fps.ToString("F2");
                }
                _totalFrames = 0;
                _totalTime = 0;
            }
        }
    }
}
