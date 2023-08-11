namespace Tron.Wallet.Web.ViewModels {
    public class ViewModel : BaseViewModel {
        public int AccountId { get; set; }

        public IList<MnemonicWallet> MnemonicWallets { get; set; } = new List<MnemonicWallet>();

        public IList<Wallet>? PrivateKeyWallets { get; set; }

        public string Url { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int RecordCount { get; set; }
    }
}
