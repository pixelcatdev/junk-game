using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Randomise the sprite
public class SpriteProps : MonoBehaviour
{
    public List<Sprite> sprites;
    public bool randomiseSprite;

    private void Start()
    {
        if (randomiseSprite)
        {
            if (sprites.Count > 0)
            {
                GetComponent<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Count)];
            }
        }
    }
}
