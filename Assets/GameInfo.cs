using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInfo : MonoBehaviour
{
    public int currentModelMode;
    public int currentPVMode;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        try {
            currentPVMode = PlayerPrefs.GetInt("PVMode");
            currentModelMode = PlayerPrefs.GetInt("ModelMode");
        } catch {

        }
        
        Invoke("LoadNextScene", 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void LoadNextScene() {
        SceneManager.LoadScene(1);
    }
}
