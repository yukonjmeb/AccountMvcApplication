using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helper
{
    public interface IWebApiHelper
    {
        string PostData(Method method, object PostData, string targetUrl, string ContentType = "application/x-www-form-urlencoded", int CodePage = 65001);

        string getData(string action, string value);

    }
}
