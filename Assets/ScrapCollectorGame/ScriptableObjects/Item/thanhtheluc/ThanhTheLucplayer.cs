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
}
