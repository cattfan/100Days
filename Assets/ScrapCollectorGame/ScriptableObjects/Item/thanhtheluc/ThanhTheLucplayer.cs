using UnityEngine;

public class ThanhTheLucplayer : MonoBehaviour
{
    public ThanhTheLuc thanhtheluc;
    public float luongtheluchientai;
    public float luongtheluctoida = 100;

    void Start()
    {
        luongtheluchientai = luongtheluctoida;
        thanhtheluc.capnhatThanhTheLuc(luongtheluchientai, luongtheluctoida);
    }

    public void TruTheLuc(float amount)
    {
        luongtheluchientai -= amount;
        if (luongtheluchientai < 0)
            luongtheluchientai = 0;

        thanhtheluc.capnhatThanhTheLuc(luongtheluchientai, luongtheluctoida);

        if (luongtheluchientai <= 0)
        {
            Debug.Log("Bạn đã quá mệt rồi!");
        }
    }
}
