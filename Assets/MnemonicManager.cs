using UnityEngine;

public class MnemonicManager : MonoBehaviour
{
    public static MnemonicManager Instance { get; private set; }
    public string Mnemonic { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}