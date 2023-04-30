using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parryable : MonoBehaviour
{

    [SerializeField] private Sprite nearSprite;
    [SerializeField] private SpriteRenderer graphics;

    Sprite originalSprite;

    void Start() {
        originalSprite = graphics.sprite;
    }

    public void PlayerClose(bool isClose) {
        if (isClose)
        {
            graphics.sprite = nearSprite;
        } else
        {
            graphics.sprite = originalSprite;
        }
    }

}
