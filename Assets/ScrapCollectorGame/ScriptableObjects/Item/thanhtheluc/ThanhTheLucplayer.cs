using UnityEngine;

public class ThanhTheLucplayer : MonoBehaviour
{
    public ThanhTheLuc thanhtheluc;
    public float luongtheluchientai;
    public float luongtheluctoida = 100;
    private float targetTheLuc;      // mục tiêu cần giảm về
    public float giamToc = 50f;      // tốc độ giảm (điều chỉnh cho mượt)

    // 🌟 Thêm biến hồi chiêu
    private float lastEatTime = -Mathf.Infinity;
    public float eatCooldown = 5f;   // 5 giây giữa hai lần ăn

    void Start()
    {
        luongtheluchientai = luongtheluctoida;
        targetTheLuc = luongtheluchientai;
        thanhtheluc.capnhatThanhTheLuc(luongtheluchientai, luongtheluctoida);
    }

    void Update()
    {
        if (luongtheluchientai != targetTheLuc)
        {
            luongtheluchientai = Mathf.MoveTowards(luongtheluchientai, targetTheLuc, giamToc * Time.deltaTime);
            thanhtheluc.capnhatThanhTheLuc(luongtheluchientai, luongtheluctoida);
        }
    }

    /// <summary>
    /// Thử ăn đồ để hồi thể lực, trả về true nếu thành công.
    /// Kiểm tra đầy thể lực và hồi chiêu 5 giây.
    /// </summary>
    public bool TryEat(float restoreAmount)
    {
        // Kiểm tra hồi chiêu
        if (Time.time - lastEatTime < eatCooldown)
        {
            ItemPickupUIController.Instance?.ShowWarningPopup("Bạn cần chờ thêm trước khi ăn tiếp!", 2f);
            return false;
        }

        // Kiểm tra đầy thể lực
        if (luongtheluchientai >= luongtheluctoida - 0.01f)
        {
            ItemPickupUIController.Instance?.ShowWarningPopup("Thể lực đã đầy, không thể ăn thêm!", 2f);
            return false;
        }

        // Hồi thể lực
        AddEnergy(restoreAmount);
        lastEatTime = Time.time;
        return true;
    }

    // ================== Các hàm cũ giữ nguyên ===================

    public void TruTheLuc(float amount)
    {
        targetTheLuc -= amount;
        if (targetTheLuc < 0) targetTheLuc = 0;
        if (targetTheLuc <= 0) Debug.Log("Bạn đã quá mệt rồi!");
    }

    public void SetEnergy(float newEnergy)
    {
        newEnergy = Mathf.Clamp(newEnergy, 0, luongtheluctoida);
        luongtheluchientai = newEnergy;
        targetTheLuc = newEnergy;

        if (thanhtheluc != null)
            thanhtheluc.capnhatThanhTheLuc(luongtheluchientai, luongtheluctoida);
    }

    public void AddEnergy(float restoreAmount)
    {
        targetTheLuc += restoreAmount;
        if (targetTheLuc > luongtheluctoida)
            targetTheLuc = luongtheluctoida;
    }

    public void SetMaxEnergy(float newMaxEnergy)
    {
        float energyPercentage = luongtheluchientai / luongtheluctoida;
        luongtheluctoida = newMaxEnergy;
        luongtheluchientai = luongtheluctoida * energyPercentage;
        targetTheLuc = luongtheluchientai;

        if (thanhtheluc != null)
            thanhtheluc.capnhatThanhTheLuc(luongtheluchientai, luongtheluctoida);
    }

    public float GetEnergyPercentage()
    {
        return luongtheluctoida > 0 ? luongtheluchientai / luongtheluctoida : 0f;
    }

    public bool IsExhausted()
    {
        return luongtheluchientai <= 0;
    }

    public void InstantRestoreEnergy(float restoreAmount)
    {
        luongtheluchientai = Mathf.Min(luongtheluchientai + restoreAmount, luongtheluctoida);
        targetTheLuc = luongtheluchientai;

        if (thanhtheluc != null)
            thanhtheluc.capnhatThanhTheLuc(luongtheluchientai, luongtheluctoida);
    }

    public void InstantUseEnergy(float amount)
    {
        luongtheluchientai = Mathf.Max(luongtheluchientai - amount, 0);
        targetTheLuc = luongtheluchientai;

        if (thanhtheluc != null)
            thanhtheluc.capnhatThanhTheLuc(luongtheluchientai, luongtheluctoida);

        if (luongtheluchientai <= 0)
            Debug.Log("Bạn đã quá mệt rồi!");
    }
}
