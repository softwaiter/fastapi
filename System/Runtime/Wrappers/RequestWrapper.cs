using CodeM.FastApi.Context;
using CodeM.FastApi.Context.Params;
using CodeM.FastApi.System.Utils;
using Microsoft.AspNetCore.Routing;

namespace CodeM.FastApi.System.Runtime.Wrappers
{
    public class RequestWrapper
    {
        private ControllerContext mCC;

        private RouteParams mRouteParams = null;

        public RequestWrapper(ControllerContext cc)
        {
            mCC = cc;
        }

        public string Path
        {
            get
            {
                return mCC.Request.Path;
            }
        }

        public string Method
        {
            get
            {
                return mCC.Request.Method;
            }
        }

        public HeaderParams Headers
        {
            get
            {
                return mCC.Headers;
            }
        }

        public RouteParams RouteParams
        {
            get
            {
                if (mRouteParams == null)
                {
                    RouteValueDictionary rvd = PermissionUtils.GetPermissionRouteValue(mCC.Request);
                    RouteData rd = new RouteData(rvd);
                    mRouteParams = new RouteParams(rd);
                }
                return mRouteParams;
            }
        }

        public QueryParams QueryParams
        {
            get
            {
                return mCC.QueryParams;
            }
        }

        public PostForms PostForms
        {
            get
            {
                return mCC.PostForms;
            }
        }

        public string PostContent
        {
            get
            {
                return mCC.PostContent;
            }
        }

        public dynamic PostJson
        {
            get
            {
                return mCC.PostJson;
            }
        }

    }
}
