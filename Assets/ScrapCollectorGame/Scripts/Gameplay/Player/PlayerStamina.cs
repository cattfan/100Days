using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;         // Thể lực tối đa
    public float currentStamina;            // Thể lực hiện tại
    private float targetStamina;            // Giá trị mục tiêu (để MoveTowards)
    public float changeSpeed = 50f;         // Tốc độ hồi/giảm mượt

    [Header("UI")]
    public ThanhTheLuc staminaBar;          // Script UI để hiển thị thanh thể lực

    void Start()
    {
        currentStamina = maxStamina;
        targetStamina = maxStamina;

        if (staminaBar != null)
            staminaBar.capnhatThanhTheLuc(currentStamina, maxStamina);
    }

    void Update()
    {
        // Làm mượt: currentStamina tiến dần về targetStamina
        if (Mathf.Abs(currentStamina - targetStamina) > 0.01f)
        {
            currentStamina = Mathf.MoveTowards(currentStamina, targetStamina, changeSpeed * Time.deltaTime);

            if (staminaBar != null)
                staminaBar.capnhatThanhTheLuc(currentStamina, maxStamina);
        }
    }

    // Giảm thể lực
    public void ReduceStamina(float amount)
    {
        targetStamina = Mathf.Max(0, targetStamina - amount);

        if (targetStamina <= 0)
        {
            Debug.Log("Bạn đã quá mệt rồi!");
        }
    }

    // Hồi thể lực
    public void RestoreStamina(float amount)
    {
        targetStamina = Mathf.Min(maxStamina, targetStamina + amount);
        Debug.Log("Thể lực đã được hồi phục: " + amount);
    }
}
