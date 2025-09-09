using UnityEngine;
using System.Collections;

namespace Trashbin
{
    public partial class Trashbin : MonoBehaviour
    {
        private IEnumerator ItemFlyOutAnimation(GameObject item, Vector3 targetPosition, float delay = 0f)
        {
            if (item == null) yield break;
            yield return new WaitForSeconds(delay);

            Vector3 startPosition = transform.position;
            Vector3 originalScale = item.transform.localScale;
            item.transform.localScale = Vector3.zero;

            float flyTime = 0.5f;
            float elapsedTime = 0f;
            Vector3 midPoint = Vector3.Lerp(startPosition, targetPosition, 0.5f) + Vector3.up * 2f;

            while (elapsedTime < flyTime)
            {
                if (item == null) yield break;

                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / flyTime;
                float easedProgress = EaseOutQuad(progress);

                item.transform.position = QuadraticBezier(startPosition, midPoint, targetPosition, easedProgress);
                float scaleProgress = Mathf.Clamp01(progress / 0.3f);
                item.transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, EaseOutBack(scaleProgress));
                item.transform.Rotate(0, 0, Time.deltaTime * 180f);

                yield return null;
            }

            if (item != null)
            {
                item.transform.position = targetPosition;
                item.transform.localScale = originalScale;
                StartCoroutine(ItemLandingBounce(item));
            }
        }

        private IEnumerator ItemLandingBounce(GameObject item)
        {
            if (item == null) yield break;
            Vector3 originalScale = item.transform.localScale;
            float bounceTime = 0.2f, elapsedTime = 0f;

            while (elapsedTime < bounceTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / bounceTime;
                float bounceScale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.2f;
                item.transform.localScale = originalScale * bounceScale;
                yield return null;
            }

            if (item != null)
            {
                item.transform.localScale = originalScale;
                ItemPickup itemPickup = item.GetComponent<ItemPickup>();
                if (itemPickup != null)
                    itemPickup.EnablePickupNow();
            }
        }

        private Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }

        private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private float EaseOutBack(float t)
        {
            float c1 = 1.70158f, c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
