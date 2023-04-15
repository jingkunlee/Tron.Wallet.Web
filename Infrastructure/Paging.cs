using System.Text;

namespace Tron.Wallet.Web {
    /// <summary>
    /// 分页控件
    /// </summary>
    public static class Paging {
        public static string Render(int pageIndex, int pageSize, int recordCount, string urlPattern) {
            if (recordCount <= pageSize) return "<tfoot><tr><td colspan=\"20\" style=\"text-align:center;border-bottom:none;\">没有更多了..</td></tr></tfoot>";

            var pageCount = recordCount / pageSize;
            var pageCountMod = recordCount % pageSize;
            if (pageCountMod > 0) pageCount += 1;

            var stringBuilder = new StringBuilder();

            //不需要展现分页控件
            if (pageCount <= 1) return stringBuilder.ToString();

            stringBuilder.Append("<tfoot><tr><td colspan=\"20\" style=\"border:none;\"><ul class=\"pagination\" style=\"margin:0;\">");

            //上一页
            stringBuilder.Append(RenderPrevious(pageIndex, urlPattern));

            //当页码比较多的情况下，需要展现首页与最后一页
            if (pageCount > 9) {
                stringBuilder.Append(RenderPage(1, pageIndex, urlPattern)); //首页

                var i = Math.Max(pageIndex - 3, 2);
                var j = Math.Min(pageIndex + 3, pageCount - 1);

                //补足显示9个页码
                var difference = 6 - (j - i);
                if (difference > 0) {
                    i = Math.Max(pageIndex - (3 + difference), 2);
                    j = Math.Min(pageIndex + (3 + difference), pageCount - 1);
                }

                if (i > 2)
                    stringBuilder.Append("<li class=\"page-item disabled\"><a class=\"page-link\" href=\"javascript:;\">...</a></li>");

                for (; i <= j; i++) {
                    stringBuilder.Append(RenderPage(i, pageIndex, urlPattern));
                }

                if (j < pageCount - 1)
                    stringBuilder.Append("<li class=\"page-item disabled\"><a class=\"page-link\" href=\"javascript:;\">...</a></li>");

                stringBuilder.Append(RenderPage(pageCount, pageIndex, urlPattern)); //最后一页
            } else {
                for (var i = 1; i <= pageCount; i++) {
                    stringBuilder.Append(RenderPage(i, pageIndex, urlPattern));
                }
            }

            //下一页
            stringBuilder.Append(RenderNext(pageIndex, pageCount, urlPattern));

            stringBuilder.Append("</ul></td></tr></tfoot>");

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 上一页
        /// </summary>
        private static string RenderPrevious(int pageIndex, string urlPattern) {
            var stringBuilder = new StringBuilder();

            if (pageIndex > 1) {
                stringBuilder.AppendFormat("<li class=\"page-item\"><a class=\"page-link\" href=\"{0}\"><span aria-hidden=\"true\">&laquo;</span></a></li>", string.Format(urlPattern, pageIndex - 1));
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 输出页码
        /// </summary>
        private static string RenderPage(int pageIndex, int currentPageIndex, string urlPattern) {
            var stringBuilder = new StringBuilder();

            if (pageIndex == currentPageIndex) {
                stringBuilder.AppendFormat("<li class=\"page-item active\"><a class=\"page-link\" href=\"javascript:;\">{0}</a></li>", pageIndex);
            } else {
                stringBuilder.AppendFormat("<li class=\"page-item\"><a class=\"page-link\" href=\"{0}\">{1}</a></li>", string.Format(urlPattern, pageIndex), pageIndex);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 下一页
        /// </summary>
        private static string RenderNext(int pageIndex, int pageCount, string urlPattern) {
            var stringBuilder = new StringBuilder();

            if (pageIndex < pageCount) {
                stringBuilder.AppendFormat("<li class=\"page-item\"><a class=\"page-link\" href=\"{0}\"><span aria-hidden=\"true\">&raquo;</span></a></li>", string.Format(urlPattern, pageIndex + 1));
            }

            return stringBuilder.ToString();
        }
    }
}
