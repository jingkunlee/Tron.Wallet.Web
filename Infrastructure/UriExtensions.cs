using System.Web;

namespace Tron.Wallet.Web {
    public static class UriExtensions {
        public static string SetParameter(this HttpRequest request, string key, string value) {
            var queryStringDict = HttpUtility.ParseQueryString(request.QueryString.ToString());
            queryStringDict.Remove(key);
            queryStringDict.Remove("p");

            var tuples = new List<Tuple<string, string>>();
            foreach (var queryStringDictKey in queryStringDict.AllKeys) {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                tuples.Add(new Tuple<string, string>(queryStringDictKey, HttpUtility.UrlEncode(queryStringDict[queryStringDictKey])));
#pragma warning restore CS8604 // 引用类型参数可能为 null。
            }

            tuples.Add(new Tuple<string, string>(key, value));

            return request.Path + "?" + string.Join("&", tuples.Select(x => $"{x.Item1}={x.Item2}"));
        }
    }
}
