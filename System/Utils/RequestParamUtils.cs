using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace CodeM.FastApi.System.Utils
{
    public class RequestParamUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type">参数类型（0:Query, 1:Header, 2:Json, 3:Form）</param>
        /// <param name="behaviour">操作类型（0:Set, 1:Add, 2:Remove）</param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void ProcessParam(HttpContext context, int type, int behaviour, string name, string value = null)
        {
            switch (type)
            {
                case 0:
                    ProcessQueryParam(context, behaviour, name, value);
                    break;
            }
        }

        public static void ProcessQueryParam(HttpContext context, int behaviour, string name, string value = null)
        {
            HttpRequest req = context.Request;
            switch (behaviour)
            {
                case 0: //Set
                    if (req.QueryString.HasValue)
                    {
                        if (req.Query.ContainsKey(name))
                        {
                            Regex re = new Regex(string.Concat(name, "\\s*=\\s*[^&]*"), RegexOptions.IgnoreCase);
                            string newContent = re.Replace(req.QueryString.Value, string.Concat(name, "=", value));
                            req.QueryString = new QueryString(newContent);
                        }
                        else
                        {
                            string v = string.Concat(req.QueryString.Value, "&", name, "=", value);
                            req.QueryString = new QueryString(v);
                        }
                    }
                    else
                    {
                        req.QueryString = new QueryString(string.Concat("?", name, "=", value));
                    }
                    break;
                case 1: //Add
                    if (!req.Query.ContainsKey(name))
                    {
                        if (req.QueryString.HasValue)
                        {
                            string v = string.Concat(req.QueryString.Value, "&", name, "=", value);
                            req.QueryString = new QueryString(v);
                        }
                        else
                        {
                            req.QueryString = new QueryString(string.Concat("?", name, "=", value));
                        }
                    }
                    break;
                case 2: //Remove
                    if (req.Query.ContainsKey(name))
                    {
                        Regex re = new Regex(string.Concat(name, "\\s*=\\s*[^&]*[&]{0,1}"), RegexOptions.IgnoreCase);
                        string newContent = re.Replace(req.QueryString.Value, string.Empty);
                        req.QueryString = new QueryString(newContent.Replace("?&", "?"));
                    }
                    break;
            }
        }
    }
}
