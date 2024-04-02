using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace Tron.Wallet.Web.Controllers {
    using HDWallet.Core;
    using HDWallet.Tron;
    using ViewModels;

    public class HomeController : BaseController {
        [HttpGet]
        public IActionResult Index(int accountId = 0, int p = 1, int pageSize = 15) {
            if (Config == null || string.IsNullOrEmpty(Config.Password)) return View("~/Views/Password/Set.cshtml");

            var viewModel = new ViewModel {
                AccountId = accountId,
                OwnerAddress = Config.OwnerAddress,
                ReceiveAddress = Config.ReceiveAddress,
                MnemonicWallets = Config.MnemonicWallets ?? new List<MnemonicWallet>(),
                Wallets = Config.Wallets,
                Url = Request.SetParameter("p", "{0}"),
                PageIndex = p,
                PageSize = pageSize
            };

            var start = (viewModel.PageIndex - 1) * viewModel.PageSize;
            var stop = viewModel.PageIndex * viewModel.PageSize - 1;

            var privateKeyWallets = new List<Wallet>();
            if (accountId == 0) {
                if (Config.PrivateKeyWallets != null && Config.PrivateKeyWallets.Count > 0) {
                    for (var i = 0; i < Config.PrivateKeyWallets.Count; i++) {
                        if (i >= start && i <= stop) {
                            var privateKeyWallet = Config.PrivateKeyWallets[i];
                            if (string.IsNullOrEmpty(privateKeyWallet.PrivateKey)) continue;

                            var privateKey = DESProvider.DESDecrypt(HttpUtility.UrlDecode(privateKeyWallet.PrivateKey), "tron", "wallet");
                            var tronEcKey = new TronECKey(privateKey, TronNetwork.MainNet);
                            var address = tronEcKey.GetPublicAddress();

                            var wallet = new Wallet { AccountId = accountId, WalletId = i, PrivateKey = privateKey, Address = address, Memo = privateKeyWallet.Memo };
                            var cacheWallet = Config?.Wallets?.FirstOrDefault(x => x.Address == address);
                            if (cacheWallet != null) {
                                wallet.TrxBalance = cacheWallet.TrxBalance;
                                wallet.EtherBalance = cacheWallet.EtherBalance;
                                wallet.Memo = cacheWallet.Memo;
                                wallet.CreateTimestamp = cacheWallet.CreateTimestamp;
                                wallet.UpdateTimestamp = cacheWallet.UpdateTimestamp;
                            }

                            privateKeyWallets.Add(wallet);
                        }
                    }

                    viewModel.RecordCount = Config.PrivateKeyWallets.Count;
                }
            } else if (accountId > 0) {
                if (Config.MnemonicWallets != null && Config.MnemonicWallets.Count > 0) {
                    var mnemonicWallet = Config.MnemonicWallets[accountId - 1];
                    var mnemonic = DESProvider.DESDecrypt(HttpUtility.UrlDecode(mnemonicWallet.Mnemonic), "tron", "wallet");
                    IHDWallet<TronWallet> tronHdWallet = new TronHDWallet(mnemonic, string.Empty);
                    var tronWallet = tronHdWallet.GetAccount(0);

                    for (var i = start; i <= stop; i++) {
                        var externalWallet = tronWallet.GetExternalWallet((uint)i);
                        var privateKey = externalWallet.PrivateKey.ToHex();
                        var address = externalWallet.Address;

                        var wallet = new Wallet { AccountId = accountId, WalletId = i, PrivateKey = privateKey, Address = address };
                        var cacheWallet = Config?.Wallets?.FirstOrDefault(x => x.Address == address);
                        if (cacheWallet != null) {
                            wallet.TrxBalance = cacheWallet.TrxBalance;
                            wallet.EtherBalance = cacheWallet.EtherBalance;
                            wallet.Memo = cacheWallet.Memo;
                            wallet.CreateTimestamp = cacheWallet.CreateTimestamp;
                            wallet.UpdateTimestamp = cacheWallet.UpdateTimestamp;
                        }

                        privateKeyWallets.Add(wallet);
                    }

                    viewModel.RecordCount = 10000;
                }
            }

            viewModel.PrivateKeyWallets = privateKeyWallets;
            return View(viewModel);
        }
    }
}