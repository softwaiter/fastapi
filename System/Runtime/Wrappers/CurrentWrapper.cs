using CodeM.FastApi.Context;
using CodeM.FastApi.Services;
using CodeM.FastApi.System.Utils;

namespace CodeM.FastApi.System.Runtime.Wrappers
{
    public class CurrentWrapper
    {
        private ControllerContext mCC;

        private dynamic mUser = null;
        private dynamic mOrg = null;

        public CurrentWrapper(ControllerContext cc)
        {
            mCC = cc;
        }

        public dynamic User
        {
            get
            {
                if (mUser == null)
                {
                    string userCode = LoginUtils.GetLoginUserCode(mCC);
                    if (userCode != null)
                    {
                        mUser = UserService.GetUserByCode(userCode);
                    }
                }
                return mUser;
            }
        }

        public dynamic Org
        {
            get
            {
                if (mOrg == null)
                {
                    if (User != null)
                    {
                        mOrg = OrgService.GetOrgByCode(User.Org);
                    }
                }
                return mOrg;
            }
        }
    }
}
