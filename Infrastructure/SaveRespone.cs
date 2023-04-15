using System;

namespace Tron.Wallet.Web {
    [Serializable]
    public sealed class SaveResponse {
        public ResponseStatus Status { get; set; }

        public string? Message { get; set; }
    }
}