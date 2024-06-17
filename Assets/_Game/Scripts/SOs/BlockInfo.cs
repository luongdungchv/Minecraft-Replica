using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Block Info", menuName = "Scriptable Objects/Block Info")]
public class BlockInfo : ScriptableObject
{
    public List<Texture2D> textures;
    public int[] uvDepthMap;
}
