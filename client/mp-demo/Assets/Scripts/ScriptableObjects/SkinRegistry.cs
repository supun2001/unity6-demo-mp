using UnityEngine;

[CreateAssetMenu(fileName = "SkinRegistry", menuName = "Game/SkinRegistry")]
public class SkinRegistry : ScriptableObject
{
    public Texture2D[] skins;
}
