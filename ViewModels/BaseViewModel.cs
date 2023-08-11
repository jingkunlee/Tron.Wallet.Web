namespace Tron.Wallet.Web.ViewModels {
    public class BaseViewModel {
        public string? OwnerAddress { get; set; }

        public string? ReceiveAddress { get; set; }

        public IList<Wallet>? Wallets { get; set; }
    }
}
