using CodeM.FastApi.Config;
using CodeM.FastApi.Context;
using CodeM.FastApi.System.Runtime.Wrappers;
using Microsoft.AspNetCore.Http;

namespace CodeM.FastApi.System.Runtime
{
    public class RuntimeEnvironment
    {
        private HttpContext mContext;
        private ApplicationConfig mConfig;
        private ControllerContext mCC;

        private CurrentWrapper mCurrent = null;
        private RequestWrapper mRequest = null;

        public RuntimeEnvironment(HttpContext context, ApplicationConfig config)
        {
            mContext = context;
            mConfig = config;
            mCC = ControllerContext.FromHttpContext(context, config);
        }

        public CurrentWrapper Current
        {
            get
            {
                if (mCurrent == null)
                {
                    mCurrent = new CurrentWrapper(mCC);
                }
                return mCurrent;
            }
        }

        public RequestWrapper Request
        {
            get
            {
                if (mRequest == null)
                {
                    mRequest = new RequestWrapper(mCC);
                }
                return mRequest;
            }
        }
    }
}
