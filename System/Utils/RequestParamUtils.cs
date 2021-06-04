using CodeM.Common.Tools.Json;
using CodeM.FastApi.Context;
using CodeM.FastApi.System.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeM.FastApi.System.Utils
{
    public class RequestParamUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="item">PermissionDataParam模型对象实例</param>
        public static void ProcessParam(ControllerContext cc, dynamic item, RuntimeEnvironment env)
        {
            int type = item.Type;
            switch (type)  //参数类型（0:Query, 1:Header, 2:PostJson, 3:PostForm）
            {
                case 0: //Query
                    ProcessQueryParam(cc, item, env);
                    break;
                case 1: //Header
                    ProcessHeaderParam(cc, item, env);
                    break;
                case 2: //PostJson
                    ProcessJsonParam(cc, item, env);
                    break;
                case 3: //PostForm
                    ProcessFormParam(cc, item, env);
                    break;
            }
        }

        public static void ProcessQueryParam(ControllerContext cc,
            dynamic item, RuntimeEnvironment env)
        {
            HttpRequest req = cc.Request;

            int behaviour = item.Behaviour;
            string name = item.Name;
            string value = item.Value;

            if (!string.IsNullOrWhiteSpace(value))
            {
                string currentValue = null;
                if (req.Query.ContainsKey(name))
                {
                    currentValue = req.Query[name];
                }

                value = PermissionUtils.ExecDataPermissionParamValue(
                    item.PermissionData, name, env, currentValue);
            }

            switch (behaviour)  //操作类型（0:Set, 1:Add, 2:Remove）
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

        public static void ProcessHeaderParam(ControllerContext cc,
            dynamic item, RuntimeEnvironment env)
        {
            HttpRequest req = cc.Request;

            int behaviour = item.Behaviour;
            string name = item.Name;
            dynamic value = item.Value;

            if (!string.IsNullOrWhiteSpace(value))
            {
                dynamic currentValue = cc.Headers.Get(name, null);
                value = PermissionUtils.ExecDataPermissionParamValue(
                    item.PermissionData, name, env, currentValue);
            }

            //操作类型（0:Set, 1:Add, 2:Remove）
            switch (behaviour)
            {
                case 0: //Set
                    req.Headers[name] = value;
                    break;
                case 1: //Add
                    if (!cc.Headers.ContainsKey(name))
                    {
                        req.Headers.Add(name, value);
                    }
                    break;
                case 2: //Remove
                    req.Headers.Remove(name);
                    break;
            }
        }

        public static void ProcessJsonParam(ControllerContext cc,
            dynamic item, RuntimeEnvironment env)
        {
            dynamic jsonObj = cc.PostJson;
            if (jsonObj == null)
            {
                jsonObj = new Json2Dynamic().CreateObject();
            }

            HttpRequest req = cc.Request;

            int behaviour = item.Behaviour;
            string name = item.Name;
            dynamic value = item.Value;

            if (!string.IsNullOrWhiteSpace(value))
            {
                dynamic currentValue = null;
                jsonObj.TryGetValue(name, out currentValue);

                value = PermissionUtils.ExecDataPermissionParamValue(
                    item.PermissionData, name, env, currentValue);
            }

            //操作类型（0:Set, 1:Add, 2:Remove）
            switch (behaviour)
            {
                case 0: //Set
                    if (jsonObj.SetValueByPath(name, value))
                    {
                        MemoryStream newBody = new MemoryStream();
                        newBody.Write(Encoding.UTF8.GetBytes(jsonObj.ToString()));
                        newBody.Seek(0, SeekOrigin.Begin);
                        req.Body = newBody;
                    }
                    else
                    {
                        throw new Exception(string.Concat("处理请求参数异常：", item.PermissionData, " - ", item.Name));
                    }
                    break;
                case 1: //Add
                    if (!jsonObj.HasPath(name))
                    {
                        if (jsonObj.SetValueByPath(name, value))
                        {
                            MemoryStream newBody = new MemoryStream();
                            newBody.Write(Encoding.UTF8.GetBytes(jsonObj.ToString()));
                            newBody.Seek(0, SeekOrigin.Begin);
                            req.Body = newBody;
                        }
                        else
                        {
                            throw new Exception(string.Concat("处理请求参数异常：", item.PermissionData, " - ", item.Name));
                        }
                    }
                    break;
                case 2: //Remove
                    if (!jsonObj.HasPath(name))
                    {
                        if (jsonObj.RemovePath(name))
                        {
                            MemoryStream newBody = new MemoryStream();
                            newBody.Write(Encoding.UTF8.GetBytes(jsonObj.ToString()));
                            newBody.Seek(0, SeekOrigin.Begin);
                            req.Body = newBody;
                        }
                        else
                        {
                            throw new Exception(string.Concat("处理请求参数异常：", item.PermissionData, " - ", item.Name));
                        }
                    }
                    break;
            }
        }

        public static void ProcessFormParam(ControllerContext cc,
            dynamic item, RuntimeEnvironment env)
        {
            HttpRequest req = cc.Request;
            if (req.HasFormContentType)
            {
                int behaviour = item.Behaviour;
                string name = item.Name;
                dynamic value = item.Value;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    dynamic currentValue = cc.PostForms.Get(name, null);
                    value = PermissionUtils.ExecDataPermissionParamValue(
                        item.PermissionData, name, env, currentValue);
                }

                switch (behaviour)
                {
                    case 0: //Set
                        Dictionary<string, StringValues> _tmp;
                        if (req.Form != null)
                        {
                            _tmp = new Dictionary<string, StringValues>(req.Form);
                        }
                        else
                        {
                            _tmp = new Dictionary<string, StringValues>();
                        }

                        if (_tmp != null)
                        {
                            _tmp[name] = value;
                            req.Form = new FormCollection(_tmp);
                        }
                        break;
                    case 1: //Add
                        Dictionary<string, StringValues> _tmp2;
                        if (req.Form != null)
                        {
                            _tmp2 = new Dictionary<string, StringValues>(req.Form);
                        }
                        else
                        {
                            _tmp2 = new Dictionary<string, StringValues>();
                        }

                        if (_tmp2 != null && !_tmp2.ContainsKey(name))
                        {
                            _tmp2.Add(name, value);
                            req.Form = new FormCollection(_tmp2);
                        }
                        break;
                    case 2: //Remove
                        if (req.Form != null)
                        {
                            Dictionary<string, StringValues> _tmp3 = new Dictionary<string, StringValues>(req.Form);
                            _tmp3.Remove(name);
                            req.Form = new FormCollection(_tmp3);
                        }
                        break;
                }
            }
        }
    }
}
