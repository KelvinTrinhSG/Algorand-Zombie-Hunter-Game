using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainConnectManager : MonoBehaviour
{
    public Button createNewAccountButton;
    public Button useCurrentAccountButton;
    public TMP_Text walletAddressText;

    public void ChangeToAlgorandConnectScene()
    {
        SceneManager.LoadScene("AlgorandConnect");
    }

    public void ChangeToAlgorandShopAndPlayScene()
    {
        SceneManager.LoadScene("AlgorandShopAndPlay");
    }

    public string LoadWalletAddress()
    {
        // Kiểm tra xem "WalletAddress" đã tồn tại trong PlayerPrefs chưa
        if (PlayerPrefs.HasKey("WalletAddress"))
        {
            string walletAddress = PlayerPrefs.GetString("WalletAddress");

            // Kiểm tra xem giá trị của "WalletAddress" có rỗng hoặc null không
            if (!string.IsNullOrEmpty(walletAddress))
            {
                Debug.Log("Wallet address found: " + walletAddress);
                return walletAddress;
            }
            else
            {
                Debug.LogWarning("Wallet address is empty.");
                return string.Empty;
            }
        }
        else
        {
            Debug.LogWarning("No wallet address found in PlayerPrefs.");
            return string.Empty;
        }
    }

    private void Start()
    {
        useCurrentAccountButton.gameObject.SetActive(false);
        walletAddressText.gameObject.SetActive(false);

        // Kiểm tra xem "SavedString" có tồn tại trong PlayerPrefs và không rỗng
        if (PlayerPrefs.HasKey("SavedString"))
        {
            string savedString = PlayerPrefs.GetString("SavedString");
            if (!string.IsNullOrEmpty(savedString))
            {
                Debug.Log("Đã có dữ liệu được lưu!");
                string playerWalletAddress = LoadWalletAddress();
                if (!string.IsNullOrEmpty(playerWalletAddress)) {
                    useCurrentAccountButton.gameObject.SetActive(true);
                    walletAddressText.text = playerWalletAddress;
                    walletAddressText.gameObject.SetActive(true);

                    Debug.Log("newAccountView.mnemonic.RefValue: " + savedString);
                    MnemonicManager.Instance.Mnemonic = savedString;
                }
            }
            else
            {
                Debug.Log("Chưa có dữ liệu!");
            }
        }
        else
        {
            Debug.Log("Chưa có dữ liệu!");
        }
    }


}
