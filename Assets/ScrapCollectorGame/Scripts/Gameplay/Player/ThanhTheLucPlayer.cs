using UnityEngine;

public class ThanhTheLucPlayer : MonoBehaviour
{
    public ThanhTheLuc thanhTheLuc;
    public float theLucHienTai;
    public float theLucToiDa = 100;
    void Start()
    {
        theLucHienTai = theLucToiDa;
        thanhTheLuc.CapNhatThanhTheLuc(theLucHienTai,theLucToiDa);
    }


    public void OnMouseDown()
    {
        theLucHienTai -= 10;
        thanhTheLuc.CapNhatThanhTheLuc(theLucHienTai,theLucToiDa);
        Debug.Log("Đã click!");
    }
}
