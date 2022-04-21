using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressBarMove : MonoBehaviour
{

    RectTransform rectTransform;
    Vector2 speed;
    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        speed = new Vector2(-2, 0);
    }

    // Update is called once per frame
    void Update()
    {



    }

    public void StartMove() {
        StartCoroutine("IE_Move");
    }

    public void StopMove() {
        StopCoroutine("IE_Move");
    }

    IEnumerator IE_Move() {
        while (true) {
            if (rectTransform.anchoredPosition.x <= -2560) {
                rectTransform.anchoredPosition = new Vector2(2560, 0);
            }

            rectTransform.anchoredPosition += speed;
            yield return new WaitForSeconds(0.05f);
        }

    }
}
