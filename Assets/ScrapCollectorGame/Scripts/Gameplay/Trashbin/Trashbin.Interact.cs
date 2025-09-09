using UnityEngine;
using System.Collections;

namespace Trashbin
{
    public partial class Trashbin : MonoBehaviour
    {
        public void Interact()
        {
            if (!CanInteract()) return;

            if (playerStamina != null)
                playerStamina.TruTheLuc(staminaCost);

            CheckTrashbin();
        }

        public bool CanInteract()
        {
            if (isChecked) return false;

            if (playerStamina != null && playerStamina.luongtheluchientai < staminaCost)
            {
                ItemPickupUIController.Instance.ShowWarningPopup("Không đủ thể lực!");
                return false;
            }
            return true;
        }

        private void CheckTrashbin()
        {
            SetChecked(true);
            StartResetTimer();

            float randomValue = Random.Range(0f, 1f);
            if (randomValue <= spawnChance)
            {
                if (audioManagement != null)
                    audioManagement.PlaySFX(audioManagement.SuccessTrashbinInteract);
                SpawnItemsWithItemData();
            }
            else
            {
                if (audioManagement != null)
                    audioManagement.PlaySFX(audioManagement.FailTrashbinInteract);
                ShowFailIcon();
            }
        }

        private void ShowFailIcon()
        {
            if (FailInteractIcon != null)
            {
                FailInteractIcon.SetActive(true);
                StartCoroutine(HideIconAfterDelay(1f, FailInteractIcon));
            }
        }

        private IEnumerator HideIconAfterDelay(float delay, GameObject icon)
        {
            yield return new WaitForSeconds(delay);
            icon.SetActive(false);
        }
    }
}
