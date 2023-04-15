using System.Text;
using System.Security.Cryptography;

namespace Tron.Wallet.Web {
    public static class DESProvider {
        #region //-----DESEncrypt-----//

        /// <summary>
        /// 使用DES加密（Added by niehl 2005-4-6）
        /// </summary>
        /// <param name="originalValue">待加密的字符串</param>
        /// <param name="key">密钥(最大长度8)</param>
        /// <param name="iv">初始化向量(最大长度8)</param>
        /// <returns>加密后的字符串</returns>
        public static string DESEncrypt(string originalValue, string key, string iv) {
            //将key和IV处理成8个字符
            key += "12345678";
            iv += "12345678";
            key = key.Substring(0, 8);
            iv = iv.Substring(0, 8);

            SymmetricAlgorithm sa = new DESCryptoServiceProvider();
            sa.Key = Encoding.UTF8.GetBytes(key);
            sa.IV = Encoding.UTF8.GetBytes(iv);
            var ct = sa.CreateEncryptor();
            var b = Encoding.Default.GetBytes(originalValue);
            using (var ms = new MemoryStream()) {
                var cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
                cs.Write(b, 0, b.Length);
                cs.FlushFinalBlock();
                cs.Close();

                return Convert.ToBase64String(ms.ToArray()).Replace("+", " ");
            }
        }

        public static string DESEncrypt(string originalValue, string key) {
            return DESEncrypt(originalValue, key, key);
        }

        #endregion

        #region //-----DESDecrypt-----//

        /// <summary>
        /// 使用DES解密（Added by niehl 2005-4-6）
        /// </summary>
        /// <param name="encryptedValue">待解密的字符串</param>
        /// <param name="key">密钥(最大长度8)</param>
        /// <param name="iv">m初始化向量(最大长度8)</param>
        /// <returns>解密后的字符串</returns>
        public static string DESDecrypt(string encryptedValue, string key, string iv) {
            //将key和IV处理成8个字符
            key += "12345678";
            iv += "12345678";
            key = key.Substring(0, 8);
            iv = iv.Substring(0, 8);

            SymmetricAlgorithm sa = new DESCryptoServiceProvider();
            sa.Key = Encoding.UTF8.GetBytes(key);
            sa.IV = Encoding.UTF8.GetBytes(iv);
            var ct = sa.CreateDecryptor();
            var b = Convert.FromBase64String(encryptedValue.Replace(" ", "+"));
            using (var ms = new MemoryStream()) {
                var cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
                cs.Write(b, 0, b.Length);
                cs.FlushFinalBlock();
                cs.Close();

                return Encoding.Default.GetString(ms.ToArray());
            }
        }

        public static string DESDecrypt(string encryptedValue, string key) {
            return DESDecrypt(encryptedValue, key, key);
        }

        #endregion
    }
}
