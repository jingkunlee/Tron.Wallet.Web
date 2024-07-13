using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace Tron.Wallet.Web.Controllers {
    using HDWallet.Core;
    using HDWallet.Tron;
    using Newtonsoft.Json;
    using Tron;

    public class BaseController : Controller {

        private readonly string configFileName = "config.json";
        protected Config Config { get; set; }

        public BaseController() {
            var configPath = Path.GetFullPath($"./{configFileName}");
            if (!System.IO.File.Exists(configPath)) {
                var fileStream = System.IO.File.Create(configPath);
                fileStream.Dispose();
            }

            using var streamReader = new StreamReader(configPath);
            Config = JsonConvert.DeserializeObject<Config>(streamReader.ReadToEnd()) ?? new Config();
        }

        #region SaveConfig

        protected void SaveConfig() {
            var configPath = Path.GetFullPath($"./{configFileName}");
            if (!System.IO.File.Exists(configPath)) {
                var fileStream = System.IO.File.Create(configPath);
                fileStream.Dispose();
            }

            using var streamWriter = new StreamWriter(configPath);
            streamWriter.WriteLine(JsonConvert.SerializeObject(Config ?? new Config())); ;
        }

        #endregion

        #region GetWalletByAccountIdByWalletId

        protected Wallet? GetWalletByAccountIdByWalletId(int accountId, int walletId) {
            if (Config == null) return null;

            Wallet? wallet = null;

            if (accountId == 0) {
                if (Config.PrivateKeyWallets == null || Config.PrivateKeyWallets.Count == 0) return null;
                var privateKeyWallet = Config.PrivateKeyWallets[walletId];
                if (privateKeyWallet == null || string.IsNullOrEmpty(privateKeyWallet.PrivateKey)) return null;

                var privateKey = DESProvider.DESDecrypt(HttpUtility.UrlDecode(privateKeyWallet.PrivateKey), "tron", "wallet");
                var tronEcKey = new TronECKey(privateKey, TronNetwork.MainNet);
                var address = tronEcKey.GetPublicAddress();

                wallet = new Wallet { AccountId = accountId, WalletId = walletId, PrivateKey = privateKey, Address = address, Memo = privateKeyWallet.Memo };
            } else if (accountId > 0) {
                if (Config.MnemonicWallets == null || Config.MnemonicWallets.Count == 0) return null;
                var mnemonicWallet = Config.MnemonicWallets[accountId - 1];
                if (mnemonicWallet == null || string.IsNullOrEmpty(mnemonicWallet.Mnemonic)) return null;

                var mnemonic = DESProvider.DESDecrypt(HttpUtility.UrlDecode(mnemonicWallet.Mnemonic), "tron", "wallet");
                IHDWallet<TronWallet> tronHdWallet = new TronHDWallet(mnemonic, string.Empty);
                var tronWallet = tronHdWallet.GetAccount(0);

                var externalWallet = tronWallet.GetExternalWallet((uint)walletId);
                var privateKey = externalWallet.PrivateKey.ToHex();
                var address = externalWallet.Address;

                wallet = new Wallet { AccountId = accountId, WalletId = walletId, PrivateKey = privateKey, Address = address, Memo = mnemonicWallet.Memo };
            }

            return wallet;
        }

        #endregion
    }
}
