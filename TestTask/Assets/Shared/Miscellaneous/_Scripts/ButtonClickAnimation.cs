using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestTask
{
    public class ButtonClickAnimation : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
    {
        [SerializeField] Shadow _Shadow;
        [SerializeField] RectTransform _ButtonContent;
        [SerializeField] Vector3 AnimationOffset;
        [SerializeField] Vector2 ShadowDistance;
        [SerializeField] float _DownSpeed;
        [SerializeField] float _UpSpeed;

        public void OnPointerDown(PointerEventData eventData)
        {
            DOTween.To(() => _Shadow.effectDistance, x => _Shadow.effectDistance = x, new Vector2(0,0), _DownSpeed);
            _ButtonContent.DOAnchorPos(AnimationOffset, _DownSpeed);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            DOTween.To(() => _Shadow.effectDistance, x => _Shadow.effectDistance = x, ShadowDistance, _UpSpeed);
            _ButtonContent.DOAnchorPos(Vector3.zero, _UpSpeed);
        }
    }
}
