using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRotate : MonoBehaviour
{
    private RectTransform rectTransform;

    // Start is called before the first frame update
    void Start()
    {
        this.rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        this.rectTransform.Rotate(0, 0, 3);
    }
}
