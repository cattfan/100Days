using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Shop Data", menuName = "Shop/Shop Data")]
public class ShopData : ScriptableObject
{
    public List<ShopItem> shopItems;
}