using UnityEngine;

public class ThanhTheLucplayer : MonoBehaviour
{
    public ThanhTheLuc thanhtheluc;
    public float luongtheluchientai;
    public float luongtheluctoida = 100;
    private float targetTheLuc;      // mục tiêu cần giảm về
    public float giamToc = 50f;      // tốc độ giảm (điều chỉnh cho mượt)

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
            // di chuyển dần về giá trị target
            luongtheluchientai = Mathf.MoveTowards(luongtheluchientai, targetTheLuc, giamToc * Time.deltaTime);
            // cập nhật UI
            thanhtheluc.capnhatThanhTheLuc(luongtheluchientai, luongtheluctoida);
        }
    }

    // Method gốc để trừ thể lực
    public void TruTheLuc(float amount)
    {
        targetTheLuc -= amount;
        if (targetTheLuc < 0)
            targetTheLuc = 0;
        if (targetTheLuc <= 0)
        {
            Debug.Log("Bạn đã quá mệt rồi!");
        }
    }

    // ✅ Methods mới cho save/load system - NĂNG LƯỢNG

    // Method để set năng lượng trực tiếp (cho load game)
    public void SetEnergy(float newEnergy)
    {
        newEnergy = Mathf.Clamp(newEnergy, 0, luongtheluctoida);
        luongtheluchientai = newEnergy;
        targetTheLuc = newEnergy;

        // Cập nhật UI ngay lập tức
        if (thanhtheluc != null)
        {
            thanhtheluc.capnhatThanhTheLuc(luongtheluchientai, luongtheluctoida);
        }

        Debug.Log($"Energy set to: {luongtheluchientai}/{luongtheluctoida}");
    }

    // Method để hồi năng lượng
    public void AddEnergy(float restoreAmount)
    {
        targetTheLuc += restoreAmount;
        if (targetTheLuc > luongtheluctoida)
            targetTheLuc = luongtheluctoida;

        Debug.Log($"Restoring {restoreAmount} energy. Target energy: {targetTheLuc}");
    }

    // Method để set năng lượng tối đa mới
    public void SetMaxEnergy(float newMaxEnergy)
    {
        float energyPercentage = luongtheluchientai / luongtheluctoida;
        luongtheluctoida = newMaxEnergy;

        // Giữ % năng lượng hiện tại
        luongtheluchientai = luongtheluctoida * energyPercentage;
        targetTheLuc = luongtheluchientai;

        if (thanhtheluc != null)
        {
            thanhtheluc.capnhatThanhTheLuc(luongtheluchientai, luongtheluctoida);
        }
    }

    // Method để lấy % năng lượng hiện tại
    public float GetEnergyPercentage()
    {
        return luongtheluctoida > 0 ? luongtheluchientai / luongtheluctoida : 0f;
    }

    // Method để kiểm tra player có hết năng lượng không
    public bool IsExhausted()
    {
        return luongtheluchientai <= 0;
    }

    // Method để hồi năng lượng ngay lập tức (không có animation)
    public void InstantRestoreEnergy(float restoreAmount)
    {
        luongtheluchientai = Mathf.Min(luongtheluchientai + restoreAmount, luongtheluctoida);
        targetTheLuc = luongtheluchientai;

        if (thanhtheluc != null)
        {
            thanhtheluc.capnhatThanhTheLuc(luongtheluchientai, luongtheluctoida);
        }
    }

    // Method để tiêu hao năng lượng ngay lập tức (không có animation)
    public void InstantUseEnergy(float amount)
    {
        luongtheluchientai = Mathf.Max(luongtheluchientai - amount, 0);
        targetTheLuc = luongtheluchientai;

        if (thanhtheluc != null)
        {
            thanhtheluc.capnhatThanhTheLuc(luongtheluchientai, luongtheluctoida);
        }

        if (luongtheluchientai <= 0)
        {
            Debug.Log("Bạn đã quá mệt rồi!");
        }
    }
}