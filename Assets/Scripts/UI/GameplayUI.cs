using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    public static GameplayUI gamePlayUI;
    public Image crosshairImage;

    private void Awake()
    {
        gamePlayUI = this;
    }

    public void CrosshairVisibility(bool value)
    {
        crosshairImage.gameObject.SetActive(value);
    }
}
