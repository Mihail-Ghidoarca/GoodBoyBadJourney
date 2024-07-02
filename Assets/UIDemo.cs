using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDemo : MonoBehaviour
{
    public TMP_Text canvasText;
    public TMP_Text worldText;

    private void Start()
    {
        canvasText.text = "VICTORY!\r\nThe good boy has finally found its owner. \r\n\r\nScore: " + GlobalVars.playerScore + "\n";
    }

    //public void ButtonDemo()
    //{
    //    canvasText.text = "VICTORY!\r\nThe good boy has finally found its owner. \r\n\r\nScore: " + GlobalVars.playerScore + "\n";
    //}
}
