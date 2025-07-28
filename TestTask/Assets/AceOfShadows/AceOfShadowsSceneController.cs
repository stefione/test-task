using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        public event Action<int> FirstStackUpdatedEvent;
        public event Action<int> SecondStackUpdatedEvent;

        public event Action<Vector3> FirstStackPositionUpdatedEvent;
        public event Action<Vector3> SecondStackPositionUpdatedEvent;

        public event Action AnimationFinishedEvent;

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

        public void SetupAndStart()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            SetupDeck(_CardContent, _FirstStackPoint, _CardCount, _CardHeight, _CardRotationFactor, _CardPrefab);
            if (_UseHeightBasedSorting)
            {
                _animationCoroutine = StartCoroutine(Coroutine_ZAxisBasedAnimation(_cards, _SecondStackPoint, _CardAnimationDuration, _CardRotationFactor, _CardHeight));
            }
            else
            {
                _animationCoroutine = StartCoroutine(Coroutine_Animation(_cards, _CardBaseSortingOrder, _SecondStackPoint, _CardAnimationDuration, _CardRotationFactor));
            }
        }

        private void SetupDeck(Transform cardContent, Transform spawnPoint, int cardCount, float cardHeight, float cardRotationFactor, SpriteRenderer cardPrefab)
        {
            foreach (Transform child in cardContent)
            {
                Destroy(child.gameObject);
            }
            _cards.Clear();

            Vector3 rotation = Vector3.zero;
            float rotationDelta = 360f / cardCount * cardRotationFactor;
            for (int i = 0; i < cardCount; i++)
            {
                var card = Instantiate(cardPrefab, cardContent);
                card.transform.eulerAngles = rotation;
                rotation.z += rotationDelta;
                if (!_UseHeightBasedSorting)
                {
                    cardHeight = 0;
                    card.sortingOrder = _CardBaseSortingOrder + i;
                }
                card.transform.position = spawnPoint.position - Vector3.forward * cardHeight * i;
                _cards.Add(card);
            }

            FirstStackUpdatedEvent?.Invoke(_cards.Count);
            SecondStackUpdatedEvent?.Invoke(0);

            FirstStackPositionUpdatedEvent?.Invoke(_FirstStackPoint.position);
            SecondStackPositionUpdatedEvent?.Invoke(_SecondStackPoint.position);
        }

        private IEnumerator Coroutine_Animation(List<SpriteRenderer> cards, int baseSortingOrder, Transform targetPoint, float delayTime, float cardRotationFactor)
        {
            var delay = new WaitForSeconds(delayTime);
            float rotationDelta = 360f / _cards.Count * cardRotationFactor;
            Vector3 targetRotation = Vector3.zero;

            for (int i = cards.Count - 1; i >= 0; i--)
            {
                FirstStackUpdatedEvent?.Invoke(i);
                int inverseIndex = cards.Count - 1 - i;
                Vector3 targetPos = targetPoint.position;

                int newSortOrder = baseSortingOrder + inverseIndex;
                if (newSortOrder > cards[i].sortingOrder)
                {
                    cards[i].sortingOrder = newSortOrder;
                }

                Sequence seq = DOTween.Sequence();
                seq.Join(cards[i].transform.DOMove(targetPos, delayTime))
                   .Join(cards[i].transform.DORotate(targetRotation, delayTime)).OnComplete(() =>
                   {
                       cards[i].sortingOrder = newSortOrder;
                   });

                targetRotation.z += rotationDelta;
                SecondStackUpdatedEvent?.Invoke(inverseIndex + 1);

                yield return delay;
            }
            AnimationFinishedEvent?.Invoke();
        }

        private IEnumerator Coroutine_ZAxisBasedAnimation(List<SpriteRenderer> cards, Transform targetPoint, float delayTime, float cardRotationFactor, float cardHeight)
        {
            float rotationDelta = 360f / _cards.Count * cardRotationFactor;
            Vector3 targetRotation = Vector3.zero;
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                FirstStackUpdatedEvent?.Invoke(i);
                int inverseIndex = cards.Count - 1 - i;
                Vector3 targetPos = targetPoint.position - Vector3.forward * cardHeight * inverseIndex;
                Vector3 startPos = cards[i].transform.position;
                Vector3 startRotation = cards[i].transform.eulerAngles;

                float lerp = 0;
                while (lerp < 1)
                {
                    Vector3 pos = Vector3.Lerp(startPos, targetPos, lerp);
                    pos.z -= _AnimHeightCurve.Evaluate(lerp);
                    cards[i].transform.position = pos;
                    _cards[i].transform.eulerAngles = Vector3.Lerp(startRotation, targetRotation, lerp);
                    lerp += Time.deltaTime * (1f / delayTime);
                    yield return null;
                }

                cards[i].transform.position = targetPos;
                cards[i].transform.eulerAngles = targetRotation;

                targetRotation.z += rotationDelta;
                SecondStackUpdatedEvent?.Invoke(inverseIndex + 1);
            }
            AnimationFinishedEvent?.Invoke();
        }
    }
}