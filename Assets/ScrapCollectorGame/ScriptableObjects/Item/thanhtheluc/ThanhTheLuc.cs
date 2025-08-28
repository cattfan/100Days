using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThanhTheLuc : MonoBehaviour
{
    public Image _ThanhTheLuc;
    public void capnhatThanhTheLuc(float luongtheluchientai, float luongtheluctoida)
    {
        _ThanhTheLuc.fillAmount = luongtheluchientai/luongtheluctoida;
    }
}
