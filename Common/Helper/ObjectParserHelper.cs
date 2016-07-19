using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Common.Helper
{
    public class ObjectParserHelper 
    {
        public static string ObjectToJson(object obj)
        {
            string result = string.Empty;

            try
            {
                result = JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
            }
            catch
            {
                return string.Empty;
            }
            return result;
        }

        public static T JsonToObject<T>(string serializeString)
        {

            T result;
            try
            {
                result = JsonConvert.DeserializeObject<T>(serializeString);

            }
            catch (Exception ex)
            {
                throw new InvalidCastException(string.Format(" Json轉型Entity失敗:{0},Input:{1}", ex.Message, serializeString));
                //return default(T);
            }
            return result;
        }

        public static string ObjectToPostString(object original)
        {
            StringBuilder poststring = new StringBuilder();
            if (original == null)
            {
                return string.Empty;
            }

            int count = 0;
            foreach (var item in original.GetType().GetProperties())
            {

                if (item.GetValue(original) == null)
                {
                    continue;
                }
                if (count == 0)
                {

                    poststring.AppendFormat("{0}={1}", item.Name, HttpUtility.UrlEncode(item.GetValue(original).ToString()));
                }
                else
                {
                    poststring.AppendFormat("&{0}={1}", item.Name, HttpUtility.UrlEncode(item.GetValue(original).ToString()));
                }
                count++;
            }

            return poststring.ToString();

        }

    }
}
