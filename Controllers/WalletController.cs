using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace Tron.Wallet.Web.Controllers {
    using Newtonsoft.Json;
    using Tron.Wallet.Web.ViewModels;
    using Tron.Wallet;
    using Tron.Wallet.Accounts;
    using Tron.Wallet.Contracts;

    public class WalletController : BaseController {
        #region List

        [HttpGet]
        public IActionResult List() {
            var viewModel = new ListViewModel { OwnerAddress = Config.OwnerAddress, ReceiveAddress = Config.ReceiveAddress, Wallets = Config.Wallets };

            return View(viewModel);
        }

        #endregion

        #region Import

        [HttpPost]
        public IActionResult Import(string mnemonicOrPrivateKey, string memo) {
            var saveResponse = new SaveResponse();

            try {
                if (string.IsNullOrEmpty(mnemonicOrPrivateKey)) {
                    var tronEcKey = TronECKey.GenerateKey(TronNetwork.MainNet);
                    var privateKey = tronEcKey.GetPrivateKey();
                    if (Config.PrivateKeyWallets == null) Config.PrivateKeyWallets = new List<PrivateKeyWallet>();

                    Config.PrivateKeyWallets.Add(new PrivateKeyWallet { PrivateKey = HttpUtility.UrlEncode(DESProvider.DESEncrypt(privateKey, "tron", "wallet")), Memo = memo });
                    SaveConfig();

                    saveResponse.Status = ResponseStatus.Success;
                } else if (mnemonicOrPrivateKey.Length == 62 || mnemonicOrPrivateKey.Length == 64) {
                    var privateKey = mnemonicOrPrivateKey;
                    var tronEcKey = new TronECKey(privateKey, TronNetwork.MainNet);

                    if (Config.PrivateKeyWallets == null) Config.PrivateKeyWallets = new List<PrivateKeyWallet>();

                    Config.PrivateKeyWallets.Add(new PrivateKeyWallet { PrivateKey = HttpUtility.UrlEncode(DESProvider.DESEncrypt(privateKey, "tron", "wallet")), Memo = memo });
                    SaveConfig();

                    saveResponse.Status = ResponseStatus.Success;
                } else if (mnemonicOrPrivateKey.Split(" ").Length == 12) {
                    var mnemonic = mnemonicOrPrivateKey;
                    if (Config.MnemonicWallets == null) Config.MnemonicWallets = new List<MnemonicWallet>();

                    Config.MnemonicWallets.Add(new MnemonicWallet { Mnemonic = HttpUtility.UrlEncode(DESProvider.DESEncrypt(mnemonic, "tron", "wallet")), Memo = memo });
                    SaveConfig();

                    saveResponse.Status = ResponseStatus.Success;
                } else throw new Exception("助记词或私钥不正确..");
            } catch (Exception exception) {
                saveResponse.Status = ResponseStatus.Exception;
                saveResponse.Message = exception.Message;
            }

            return Json(saveResponse);
        }

        #endregion

        #region GetByAccountIdByWalletId

        [HttpPost]
        public IActionResult GetByAccountIdByWalletId(int accountId, int walletId, string password) {
            var saveResponse = new SaveResponse();

            try {
                if (Config == null || string.IsNullOrEmpty(Config.Password)) throw new Exception("未设置钱包密码..");
                if (string.IsNullOrEmpty(password)) throw new Exception("请输入钱包密码..");
                var encryptedPassword = HashProvider.MD5Encrypt($"{HashProvider.MD5Encrypt($"{password}//--")}//--");
                if (encryptedPassword != Config.Password) throw new Exception("钱包密码不正确..");

                var wallet = GetWalletByAccountIdByWalletId(accountId, walletId);
                if (wallet == null) throw new Exception("Wallet not exsit.");

                return Json(new { Status = (sbyte)ResponseStatus.Success, PrivateKey = wallet.PrivateKey, Address = wallet.Address });
            } catch (Exception exception) {
                saveResponse.Status = ResponseStatus.Exception;
                saveResponse.Message = exception.Message;
            }

            return Json(saveResponse);
        }

        #endregion

        #region RemoveByAccountIdByWalletId

        [HttpPost]
        public IActionResult RemoveByAccountIdByWalletId(int accountId, int walletId) {
            var saveResponse = new SaveResponse();

            try {
                if (Config == null || Config.Wallets == null) throw new Exception("记录不存在..");

                var wallet = Config.Wallets.FirstOrDefault(x => x.AccountId == accountId && x.WalletId == walletId);
                if (wallet == null) throw new Exception("记录不存在..");

                var isSuccess = Config.Wallets.Remove(wallet);
                if (isSuccess) {
                    SaveConfig();
                    saveResponse.Status = ResponseStatus.Success;
                }
            } catch (Exception exception) {
                saveResponse.Status = ResponseStatus.Exception;
                saveResponse.Message = exception.Message;
            }

            return Json(saveResponse);
        }

        #endregion

        #region GetByOnlineByAccountIdByWalletId

        [HttpPost]
        public ActionResult GetByOnlineByAccountIdByWalletId(int accountId, int walletId) {
            var wallet = GetWalletByAccountIdByWalletId(accountId, walletId);
            if (wallet == null) return Json(new SaveResponse { Message = "Wallet not exsit." });

            wallet.PrivateKey = HttpUtility.UrlEncode(DESProvider.DESEncrypt(wallet.PrivateKey, "tron", "wallet"));
            wallet.UpdateTimestamp = DateTime.Now.GetUnixTimestamp();

            try {
                var responseString = HttpClientHelper.Get($"https://api.trongrid.io/v1/accounts/{wallet.Address}");
                if (string.IsNullOrEmpty(responseString)) throw new Exception();

                var responseObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                if (responseObject == null) throw new Exception();

                if ((bool)responseObject.success != true) throw new Exception();
                if (responseObject.data == null || responseObject.data.Count == 0) throw new Exception("未激活");

                var obj = responseObject.data[0];
                if (obj == null) throw new Exception();

                var createTimestamp = obj.create_time;
                if (createTimestamp != null) wallet.CreateTimestamp = (long)createTimestamp / 1000;

                var balance = obj.balance;
                if (balance != null) wallet.TrxBalance = (long)balance / new decimal(1000000);

                var trc20Tokens = obj.trc20;
                if (trc20Tokens != null) {
                    foreach (var trc20Token in trc20Tokens) {
                        var etherBalance = trc20Token.TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t;
                        if (etherBalance != null) wallet.EtherBalance = (long)etherBalance / new decimal(1000000);
                    }
                }
            } catch (Exception exception) {
                return Json(new { Status = (sbyte)ResponseStatus.Exception, Message = exception.Message });
            }

            if (Config.Wallets == null) Config.Wallets = new List<Wallet>();

            var exsitWallet = Config.Wallets.FirstOrDefault(x => x.AccountId == wallet.AccountId && x.WalletId == wallet.WalletId);
            if (exsitWallet != null) {
                exsitWallet.TrxBalance = wallet.TrxBalance;
                exsitWallet.EtherBalance = wallet.EtherBalance;
                exsitWallet.UpdateTimestamp = wallet.UpdateTimestamp;
            } else {
                Config.Wallets.Add(wallet);
            }

            SaveConfig();

            var result = new {
                Status = (sbyte)ResponseStatus.Success,
                TrxBalance = wallet.TrxBalance.ToString("0.######"),
                EtherBalance = wallet.EtherBalance.ToString("0.######"),
                CreateTime = wallet.CreateTimestamp == 0 ? "--" : DateTimeExtensions.GetDatetimeFromTimestamp(wallet.CreateTimestamp).ToString("MM-dd HH:mm"),
                UpdateTime = DateTimeExtensions.GetDatetimeFromTimestamp(wallet.UpdateTimestamp).ToString("HH:mm:ss"),
            };

            return Json(result);
        }

        #endregion        

        #region TrxTransferAsync

        private static async Task<dynamic> TrxTransferAsync(string privateKey, string to, long amount) {
            var record = TronServiceExtension.GetRecord();
            var transactionClient = record.TronClient?.GetTransaction();

            var account = new TronAccount(privateKey, TronNetwork.MainNet);

            var transactionExtension = await transactionClient?.CreateTransactionAsync(account.Address, to, amount)!;

            var transactionId = transactionExtension.Txid.ToStringUtf8();

            var transactionSigned = transactionClient.GetTransactionSign(transactionExtension.Transaction, privateKey);
            var returnObj = await transactionClient.BroadcastTransactionAsync(transactionSigned);

            return new { Result = returnObj.Result, Message = returnObj.Message?.ToStringUtf8(), TransactionId = transactionId };
        }

        #endregion

        #region EtherTransferAsync

        private static async Task<string> EtherTransferAsync(string privateKey, string toAddress, decimal amount, string? memo) {
            const string contractAddress = "TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t";

            var record = TronServiceExtension.GetRecord();
            var contractClientFactory = record.ServiceProvider.GetService<IContractClientFactory>();
            var contractClient = contractClientFactory?.CreateClient(ContractProtocol.TRC20);

            var account = new TronAccount(privateKey, TronNetwork.MainNet);

            const long feeAmount = 60 * 1000000L;

#pragma warning disable CS8602 // 解引用可能出现空引用。
            return await contractClient.TransferAsync(contractAddress, account, toAddress, amount, memo, feeAmount);
#pragma warning restore CS8602 // 解引用可能出现空引用。
        }

        #endregion

        #region TransferOut

        [HttpPost]
        public ActionResult TransferOut(WalletTransaction transaction) {
            var saveResponse = new SaveResponse();

            try {
                if (string.IsNullOrEmpty(transaction.FromAddress)) throw new Exception("请填写转出地址..");
                if (string.IsNullOrEmpty(transaction.ToAddress)) throw new Exception("请填写接收地址..");
                if (transaction.Token == 0) throw new Exception("请选择 Token 类型..");
                if (transaction.Amount <= 0) throw new Exception("请填写转出金额..");
                if (string.IsNullOrEmpty(transaction.Password)) throw new Exception("请输入钱包密码..");

                var encryptedPassword = HashProvider.MD5Encrypt($"{HashProvider.MD5Encrypt($"{transaction.Password}//--")}//--");
                if (encryptedPassword != Config.Password) throw new Exception("钱包密码不正确..");

                var wallet = GetWalletByAccountIdByWalletId(transaction.AccountId, transaction.WalletId);
                if (wallet == null) throw new Exception("钱包不存在..");

                if (transaction.FromAddress != wallet.Address) throw new Exception("钱包地址与私钥不匹配..");

                var transferAmount = (long)(transaction.Amount * 1000000L);
                var transactionId = string.Empty;

                if (transaction.Memo == null) transaction.Memo = string.Empty;
                switch ((Token)transaction.Token) {
                    case Token.Ether: {
                            transactionId = EtherTransferAsync(wallet.PrivateKey, transaction.ToAddress, transaction.Amount, transaction.Memo).Result;
                            break;
                        }
                    case Token.Trx: {
                            var result = TrxTransferAsync(wallet.PrivateKey, transaction.ToAddress, transferAmount).Result;
                            transactionId = result.TransactionId;
                            break;
                        }
                }

                return Json(new { Status = ResponseStatus.Success, TransactionId = transactionId });

            } catch (Exception exception) {
                saveResponse.Status = ResponseStatus.Exception;
                saveResponse.Message = exception.Message;
            }

            return Json(saveResponse);
        }

        #endregion

        #region AccountPermissionUpdateAsync

        private static async Task<dynamic> AccountPermissionUpdateAsync(string privateKey, string toAddress) {
            var record = TronServiceExtension.GetRecord();
            var transactionClient = record.TronClient?.GetTransaction();
            var account = new TronAccount(privateKey, TronNetwork.MainNet);

            var transactionExtention = await transactionClient?.CreateAccountPermissionUpdateTransactionAsync(account.Address, toAddress)!;

            var transactionSigned = transactionClient.GetTransactionSign(transactionExtention.Transaction, privateKey);
            var returnObj = await transactionClient.BroadcastTransactionAsync(transactionSigned);

            return new { Result = returnObj.Result, Message = returnObj.Message?.ToStringUtf8(), TransactionId = transactionExtention.Transaction.GetTxid() };
        }

        #endregion

        #region Authorize

        [HttpPost]
        public ActionResult Authorize(int accountId, int walletId, string ownerAddress, string password) {
            var saveResponse = new SaveResponse();

            try {
                var wallet = GetWalletByAccountIdByWalletId(accountId, walletId);
                if (wallet == null) throw new Exception("钱包不存在..");

                if (string.IsNullOrEmpty(password)) throw new Exception("请输入钱包密码..");
                var encryptedPassword = HashProvider.MD5Encrypt($"{HashProvider.MD5Encrypt($"{password}//--")}//--");
                if (encryptedPassword != Config.Password) throw new Exception("钱包密码不正确..");

                var result = AccountPermissionUpdateAsync(wallet.PrivateKey, ownerAddress).Result;
                return Json(new { Status = ResponseStatus.Success, TransactionId = result.TransactionId });

            } catch (Exception exception) {
                saveResponse.Status = ResponseStatus.Exception;
                saveResponse.Message = exception.Message;
            }

            return Json(saveResponse);
        }

        #endregion
    }
}
