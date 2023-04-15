using Microsoft.AspNetCore.Mvc;

namespace Tron.Wallet.Web.Controllers {
    public class PasswordController : BaseController {
        #region Set

        [HttpPost]
        public IActionResult Set(string password, string confirmPassword) {
            var saveResponse = new SaveResponse();

            try {
                if (string.IsNullOrEmpty(password)) throw new Exception("请设置钱包密码..");
                if (string.IsNullOrEmpty(confirmPassword)) throw new Exception("请再次输入钱包密码..");
                if (password != confirmPassword) throw new Exception("两次密码不一致..");

                var encryptedPassword = HashProvider.MD5Encrypt($"{HashProvider.MD5Encrypt($"{password}//--")}//--");
                Config.Password = encryptedPassword;

                SaveConfig();
                saveResponse.Status = ResponseStatus.Success;
            } catch (Exception exception) {
                saveResponse.Status = ResponseStatus.Exception;
                saveResponse.Message = exception.Message;
            }

            return Json(saveResponse);
        }

        #endregion
    }
}
