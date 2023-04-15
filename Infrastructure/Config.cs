namespace Tron.Wallet.Web {
    #region Config

    [Serializable]
    public class Config {
        public string? Password { get; set; }

        public IList<MnemonicWallet>? MnemonicWallets { get; set; }

        public IList<PrivateKeyWallet>? PrivateKeyWallets { get; set; }

        public IList<Wallet>? Wallets { get; set; }
    }

    #endregion

    #region Mnemonic

    [Serializable]
    public class MnemonicWallet {
        public string Mnemonic { get; set; }

        public string? Memo { get; set; }

    }

    #endregion

    #region PrivateKeyWallet

    [Serializable]
    public class PrivateKeyWallet {
        public string PrivateKey { get; set; }

        public string? Memo { get; set; }

    }

    #endregion

    #region Wallet

    [Serializable]
    public class Wallet {
        public int AccountId { get; set; }

        public int WalletId { get; set; }

        public string PrivateKey { get; set; }

        public string Address { get; set; }

        public decimal TrxBalance { get; set; }

        public decimal EtherBalance { get; set; }

        public string? Memo { get; set; }

        public long CreateTimestamp { get; set; }

        public long UpdateTimestamp { get; set; }
    }

    #endregion
}