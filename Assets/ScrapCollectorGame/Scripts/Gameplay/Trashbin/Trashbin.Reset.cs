using UnityEngine;
using System.Collections;

namespace Trashbin
{
    public partial class Trashbin : MonoBehaviour
    {
        private void StartResetTimer()
        {
            if (resetCoroutine != null) StopCoroutine(resetCoroutine);
            resetCoroutine = StartCoroutine(ResetTrashbinTimer());
        }

        private IEnumerator ResetTrashbinTimer()
        {
            float timeRemaining = resetTime;
            while (timeRemaining > 0)
            {
                if (showResetTimer && Mathf.FloorToInt(timeRemaining) % 10 == 0 && timeRemaining == Mathf.Floor(timeRemaining))
                    Debug.Log($"Trashbin '{TrashbinName}' reset in {Mathf.FloorToInt(timeRemaining)}s");

                timeRemaining -= Time.deltaTime;
                yield return null;
            }

            ResetTrashbin();
        }

        private void ResetTrashbin()
        {
            SetChecked(false);
            resetCoroutine = null;
            Debug.Log($"Trashbin '{TrashbinName}' reset!");
        }

        public void ForceReset()
        {
            if (resetCoroutine != null) StopCoroutine(resetCoroutine);
            ResetTrashbin();
        }

        public float GetTimeUntilReset()
        {
            if (resetCoroutine == null || !isChecked) return 0f;
            return resetTime; // có thể cải thiện sau
        }
    }
}
