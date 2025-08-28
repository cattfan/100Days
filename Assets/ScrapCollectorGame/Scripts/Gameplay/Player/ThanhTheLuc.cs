using UnityEngine;
using UnityEngine.UI;

public class ThanhTheLuc : MonoBehaviour
{
    public Image thanhTheLucImage;
    public void CapNhatThanhTheLuc(float theLucHienTai, float theLucToiDa)
    {
        float tiLeTheLuc = theLucHienTai / theLucToiDa;
        thanhTheLucImage.fillAmount = tiLeTheLuc;
    }
}
