namespace Tron.Wallet.Web {
    public class WalletTransaction {
        public int AccountId { get; set; }

        public int WalletId { get; set; }

        public string? FromAddress { get; set; }

        public string? ToAddress { get; set; }

        public sbyte Token { get; set; }

        public decimal Amount { get; set; }

        public string? Memo { get; set; }

        public string? Password { get; set; }
    }

    public enum Token {
        Trx = 1,
        Ether = 2
    }
}
