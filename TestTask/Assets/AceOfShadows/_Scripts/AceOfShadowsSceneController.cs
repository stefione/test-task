using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TestTask.AceOfShadows
{
    public class AceOfShadowsSceneController : MonoBehaviour
    {
        [SerializeField] private Transform _FirstStackPoint;
        [SerializeField] private Transform _SecondStackPoint;
        [SerializeField] private SpriteRenderer _CardPrefab;
        [SerializeField] private Transform _CardContent;

        [SerializeField] private float _CardHeight;
        [SerializeField] private int _CardCount;
        [SerializeField] private float _CardRotationFactor;
        [SerializeField] private float _CardAnimationDuration;
        [SerializeField] private int _CardBaseSortingOrder;
        [SerializeField] private bool _UseHeightBasedSorting;
        [SerializeField] private AnimationCurve _AnimHeightCurve;

        private List<SpriteRenderer> _cards = new();
        private Coroutine _animationCoroutine;

        public event Action<int> FirstStackCountUpdatedEvent;
        public event Action<int> SecondStackCountUpdatedEvent;
        public event Action<Vector3> FirstStackPositionUpdatedEvent;
        public event Action<Vector3> SecondStackPositionUpdatedEvent;
        public event Action AnimationFinishedEvent;

        public float CardAnimationDuration => _CardAnimationDuration;

        private void Start()
        {
            SetupAndStart();
        }

        private void Update()
        {
            if (_FirstStackPoint.hasChanged)
            {
                FirstStackPositionUpdatedEvent.Invoke(_FirstStackPoint.position);
                _FirstStackPoint.hasChanged = false;
            }

            if (_SecondStackPoint.hasChanged)
            {
                SecondStackPositionUpdatedEvent.Invoke(_SecondStackPoint.position);
                _SecondStackPoint.hasChanged = false;
            }
        }

        public void UpdateAnimationDuration(float value)
        {
            _CardAnimationDuration = value;
        }

        public void SetupAndStart()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            SetupDeck();
            if (_UseHeightBasedSorting)
            {
                _animationCoroutine = StartCoroutine(Coroutine_ZAxisBasedAnimation(_SecondStackPoint));
            }
            else
            {
                _animationCoroutine = StartCoroutine(Coroutine_SortingBasedAnimation(_SecondStackPoint));
            }
        }

        private void SetupDeck()
        {
            foreach (Transform child in _CardContent)
            {
                Destroy(child.gameObject);
            }
            _cards.Clear();

            Vector3 rotation = Vector3.zero;
            float rotationDelta = 360f / _CardCount * _CardRotationFactor;
            for (int i = 0; i < _CardCount; i++)
            {
                var card = Instantiate(_CardPrefab, _CardContent);
                card.transform.eulerAngles = rotation;
                rotation.z += rotationDelta;
                float height = _CardHeight;
                if (!_UseHeightBasedSorting)
                {
                    height = 0;
                    card.sortingOrder = _CardBaseSortingOrder + i;
                }
                card.transform.position = _FirstStackPoint.position - Vector3.forward * height * i;
                _cards.Add(card);
            }

            FirstStackCountUpdatedEvent?.Invoke(_cards.Count);
            SecondStackCountUpdatedEvent?.Invoke(0);

            FirstStackPositionUpdatedEvent?.Invoke(_FirstStackPoint.position);
            SecondStackPositionUpdatedEvent?.Invoke(_SecondStackPoint.position);
        }

        private IEnumerator Coroutine_SortingBasedAnimation(Transform targetPoint)
        {
            var delay = new WaitForSeconds(_CardAnimationDuration);
            float rotationDelta = 360f / _cards.Count * _CardRotationFactor;
            Vector3 targetRotation = Vector3.zero;

            for (int i = _cards.Count - 1; i >= 0; i--)
            {
                FirstStackCountUpdatedEvent?.Invoke(i);
                int inverseIndex = _cards.Count - 1 - i;
                Vector3 targetPos = targetPoint.position;

                int newSortOrder = _CardBaseSortingOrder + inverseIndex;
                if (newSortOrder > _cards[i].sortingOrder)
                {
                    _cards[i].sortingOrder = newSortOrder;
                }

                Sequence seq = DOTween.Sequence();
                seq.Join(_cards[i].transform.DOMove(targetPos, _CardAnimationDuration))
                   .Join(_cards[i].transform.DORotate(targetRotation, _CardAnimationDuration)).OnComplete(() =>
                   {
                       _cards[i].sortingOrder = newSortOrder;
                   });

                targetRotation.z += rotationDelta;
                SecondStackCountUpdatedEvent?.Invoke(inverseIndex + 1);

                yield return delay;
            }
            AnimationFinishedEvent?.Invoke();
        }

        private IEnumerator Coroutine_ZAxisBasedAnimation(Transform targetPoint)
        {
            float rotationDelta = 360f / _cards.Count * _CardRotationFactor;
            Vector3 targetRotation = Vector3.zero;
            for (int i = _cards.Count - 1; i >= 0; i--)
            {
                FirstStackCountUpdatedEvent?.Invoke(i);
                int inverseIndex = _cards.Count - 1 - i;
                Vector3 targetPos = targetPoint.position - Vector3.forward * _CardHeight * inverseIndex;
                Vector3 startPos = _cards[i].transform.position;
                Vector3 startRotation = _cards[i].transform.eulerAngles;

                float lerp = 0;
                while (lerp < 1)
                {
                    Vector3 pos = Vector3.Lerp(startPos, targetPos, lerp);
                    pos.z -= _AnimHeightCurve.Evaluate(lerp);
                    _cards[i].transform.position = pos;
                    _cards[i].transform.eulerAngles = Vector3.Lerp(startRotation, targetRotation, lerp);
                    lerp += Time.deltaTime * (1f / _CardAnimationDuration);
                    yield return null;
                }

                _cards[i].transform.position = targetPos;
                _cards[i].transform.eulerAngles = targetRotation;

                targetRotation.z += rotationDelta;
                SecondStackCountUpdatedEvent?.Invoke(inverseIndex + 1);
            }
            AnimationFinishedEvent?.Invoke();
        }
    }
}