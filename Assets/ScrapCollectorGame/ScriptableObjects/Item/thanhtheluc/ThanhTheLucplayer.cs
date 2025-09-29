using UnityEngine;

public class ThanhTheLucplayer : MonoBehaviour
{
    public ThanhTheLuc thanhtheluc;
    public float luongtheluchientai;
    public float luongtheluctoida = 100;
    private float targetTheLuc;
    public float giamToc = 50f;

    private float lastEatTime = -Mathf.Infinity;
    public float eatCooldown = 5f;

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

    public bool TryEat(float restoreAmount)
    {
        if (Time.time - lastEatTime < eatCooldown)
        {
            ItemPickupUIController.Instance?.ShowWarningPopup("Bạn cần chờ thêm trước khi ăn tiếp!", 2f);
            return false;
        }

        if (luongtheluchientai >= luongtheluctoida - 0.01f)
        {
            ItemPickupUIController.Instance?.ShowWarningPopup("Thể lực đã đầy, không thể ăn thêm!", 2f);
            return false;
        }

        AddEnergy(restoreAmount);
        lastEatTime = Time.time;
        return true;
    }

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
