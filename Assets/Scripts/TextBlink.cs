using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextBlink : MonoBehaviour
{
    public float blinkSpeed = 1.0f;
    private TextMeshProUGUI Text;

    // Start is called before the first frame update
    void Start()
    {
        Text = GetComponent<TMPro.TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        Text.color = new Color(Mathf.PingPong(blinkSpeed * Time.time, 1.0f),
                               Mathf.PingPong(blinkSpeed * Time.time, 1.0f),
                               Mathf.PingPong(blinkSpeed * Time.time, 1.0f),
                               1);
    }
}
