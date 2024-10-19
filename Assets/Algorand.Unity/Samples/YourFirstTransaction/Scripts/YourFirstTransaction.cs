using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace Algorand.Unity.Samples.YourFirstTransaction
{
    public class YourFirstTransaction : MonoBehaviour
    {
        public AlgodClient algod = new("https://testnet-api.algonode.cloud");

        public IndexerClient indexer = new("https://testnet-idx.algonode.cloud");

        public string testnetFaucetUrl = "https://dispenser.testnet.aws.algodev.network/?account={0}";

        public string viewTransactionUrl = "https://testnet.algoexplorer.io/tx/{0}";

        [field: SerializeField]
        public string RecipientText { get; set; }

        [field: SerializeField]
        public string PayAmountText { get; set; }

        [Space]
        public UnityEvent<string> onCheckAlgodStatus;

        public UnityEvent<string> onCheckIndexerStatus;

        public UnityEvent<string> onAccountGenerated;

        public UnityEvent<string> onBalanceTextUpdated;

        public UnityEvent onEnoughBalanceForPayment;

        public UnityEvent<string> onTxnStatusUpdate;

        public UnityEvent onTransactionConfirmedSuccessfully;

        private string txnStatus;

        public string AlgodHealth { get; set; }

        public string IndexerHealth { get; set; }

        public MicroAlgos Balance { get; set; }

        public Account Account { get; set; }

        public string TxnStatus
        {
            get => txnStatus;
            set
            {
                txnStatus = value;
                onTxnStatusUpdate?.Invoke(value);
            }
        }

        public string ConfirmedTxnId { get; set; }

        public void CheckAlgodStatus()
        {
            CheckAlgodStatusAsync().Forget();
        }

        public void CheckIndexerStatus()
        {
            CheckIndexerStatusAsync().Forget();
        }

        private async UniTaskVoid CheckAlgodStatusAsync()
        {
            var response = await algod.HealthCheck();
            AlgodHealth = response.Error ? response.Error : "Connected";
            onCheckAlgodStatus?.Invoke(AlgodHealth);
        }

        private async UniTaskVoid CheckIndexerStatusAsync()
        {
            var response = await indexer.MakeHealthCheck();
            IndexerHealth = response.Error ? response.Error : "Connected";
            onCheckIndexerStatus?.Invoke(IndexerHealth);
        }

        public void GenerateAccount()
        {
            Account = Account.GenerateAccountBaseOneMnemonic(MnemonicManager.Instance.Mnemonic);

            onAccountGenerated?.Invoke(Account.Address.ToString());
            onBalanceTextUpdated?.Invoke(Balance.ToAlgos().ToString("F6"));
        }

        public void OpenFaucetUrl()
        {
            Application.OpenURL(string.Format(testnetFaucetUrl, Account.Address.ToString()));
        }

        public void CheckBalance()
        {
            CheckBalanceAsync().Forget();
        }

        private async UniTaskVoid CheckBalanceAsync()
        {
            var (err, resp) = await indexer.LookupAccountByID(Account.Address);
            if (err)
            {
                Balance = 0;
                if (!err.Message.Contains("no accounts found for address")) Debug.LogError(err);
            }
            else
            {
                Balance = resp.Account.Amount;
            }

            onBalanceTextUpdated?.Invoke(Balance.ToAlgos().ToString("F6"));
            if (Balance >= 1_000) onEnoughBalanceForPayment?.Invoke();
        }

        public void MakePayment(int indexValue)
        {
            MakePaymentAsync(indexValue).Forget();
        }

        public Button paymentButton200;
        public Button paymentButton500;
        public Button paymentButton1500;
        public Button paymentButton5000;

        public Text totalGoldBoughtText;

        private void ShowPaymentButton() {
            paymentButton200.interactable = true;
            paymentButton500.interactable = true;
            paymentButton1500.interactable = true;
            paymentButton5000.interactable = true;
        }

        public async UniTaskVoid MakePaymentAsync(int indexValue)
        {
            if (indexValue == 1) {
                PayAmountText = "1";
            }
            else if (indexValue == 2)
            {
                PayAmountText = "2";
            }
            else if (indexValue == 3)
            {
                PayAmountText = "3";
            }
            else if (indexValue == 4)
            {
                PayAmountText = "4";
            }


            var addressParseError = Address.TryParse(RecipientText, out var recipientAddress);
            if (addressParseError > AddressFormatError.None)
            {
                TxnStatus = $"Recipient address formatted incorrectly: {addressParseError}";
                return;
            }

            if (!double.TryParse(PayAmountText, out var payAmountAlgos))
            {
                TxnStatus = $"Invalid format: Pay amount must be a double, instead it was \"{PayAmountText}\"";
                return;
            }

            // Get the suggested transaction params
            var (txnParamsError, txnParams) = await algod.TransactionParams();
            if (txnParamsError)
            {
                Debug.LogError(txnParamsError);
                TxnStatus = $"error: {txnParamsError}";

                ShowPaymentButton();

                return;
            }

            // Construct and sign the payment transaction
            var paymentTxn = Transaction.Payment(
                Account.Address,
                txnParams,
                recipientAddress,
                MicroAlgos.FromAlgos(payAmountAlgos)
            );
            var signedTxn = Account.SignTxn(paymentTxn);

            // Send the transaction
            try
            {
                var (sendTxnError, txid) = await algod.SendTransaction(signedTxn);
                if (sendTxnError)
                {
                    Debug.LogError(sendTxnError);
                    TxnStatus = $"error: {sendTxnError}";

                    ShowPaymentButton();

                    return;
                }

                // Wait for the transaction to be confirmed
                try
                {
                    var (confirmErr, confirmed) = await algod.WaitForConfirmation(txid.TxId);
                    if (confirmErr)
                    {
                        Debug.LogError(confirmErr);
                        TxnStatus = $"error: {confirmErr}";

                        ShowPaymentButton();

                        return;
                    }

                    TxnStatus = "Transaction confirmed!";
                    ConfirmedTxnId = txid.TxId;
                    onTransactionConfirmedSuccessfully?.Invoke();

                    if (indexValue == 1)
                    {
                        ResourceBoost.Instance.golds += 200;                        
                    }
                    else if (indexValue == 2)
                    {
                        ResourceBoost.Instance.golds += 500;
                    }
                    else if (indexValue == 3)
                    {
                        ResourceBoost.Instance.golds += 1500;
                    }
                    else if (indexValue == 4)
                    {
                        ResourceBoost.Instance.golds += 5000;
                    }
                    totalGoldBoughtText.text = "Total Gold Bought: " + ResourceBoost.Instance.golds.ToString();
                    totalGoldBoughtText.gameObject.SetActive(true);

                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error during transaction confirmation: {ex.Message}");
                    TxnStatus = $"error: {ex.Message}";
                    ShowPaymentButton();
                }                
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during transaction sending: {ex.Message}");
                TxnStatus = $"error: {ex.Message}";
                ShowPaymentButton();
            }            
        }

        public void ViewConfirmedTransaction()
        {
            //Application.OpenURL(string.Format(viewTransactionUrl, ConfirmedTxnId));
            Application.OpenURL("https://explorer.bitquery.io/algorand_testnet");
        }

        public void PlayGame() {
            SceneManager.LoadScene("Logo");
        }
    }


}
