using CodeM.FastApi.Services;
using System.Collections.Generic;

namespace CodeM.FastApi.System.Runtime
{
    public class RuntimeAssert
    {
        private RuntimeEnvironment mEnv;

        public RuntimeAssert(RuntimeEnvironment env)
        {
            mEnv = env;
        }

        /// <summary>
        /// 指定用户是否和当前用户同一机构
        /// </summary>
        /// <param name="id">指定用户Id</param>
        /// <returns></returns>
        public bool IsSameOrgByUserId(string id)
        {
            dynamic user = UserService.GetUserById(id);
            if (user != null)
            {
                return user.Org == mEnv.Current.User.Org;
            }
            return false;
        }

        /// <summary>
        /// 指定用户是否和当前用户同一机构
        /// </summary>
        /// <param name="code">指定用户编码</param>
        /// <returns></returns>
        public bool IsSameOrgByUserCode(string code)
        {
            dynamic user = UserService.GetUserByCode(code);
            if (user != null)
            {
                return user.Org == mEnv.Current.User.Org;
            }
            return false;
        }

        /// <summary>
        /// 当前用户是否有指定产品的有效授权
        /// </summary>
        /// <param name="prodCode">指定产品的编码</param>
        /// <returns></returns>
        public bool IsHaveProductPermission(string prodCode)
        {
            dynamic userProd = UserProdService.GetUserProdByCode(mEnv.Current.User.Code, prodCode);
            return userProd != null && userProd.Actived && !userProd.Deleted;
        }

        /// <summary>
        /// 当前用户是否拥有所有指定产品模块的权限
        /// </summary>
        /// <param name="prodCode">产品编码</param>
        /// <param name="moduleCodes">模块编码数组</param>
        /// <returns></returns>
        public bool IsHaveProductModulePermission(string prodCode, params string[] moduleCodes)
        {
            List<string> source = new List<string>(moduleCodes);

            List<dynamic> userModules = UserModuleService.GetEffectiveListByUserAndProduct(mEnv.Current.User.Code, prodCode);
            for (int i = 0; i < userModules.Count; i++)
            {
                source.Remove(userModules[i].Module.Code);
                if (source.Count == 0)
                {
                    return true;
                }
            }

            List<dynamic> userRoles = UserRoleService.GetEffectiveListByUserAndProduct(mEnv.Current.User.Code, prodCode);

            List<string> roleCodes = new List<string>();
            userRoles.ForEach(item => {
                roleCodes.Add(item.Role);
            });

            List<dynamic> roleModules = RoleModuleService.GetEffectiveListByRole(roleCodes.ToArray());
            for (int i = 0; i < roleModules.Count; i++)
            {
                source.Remove(roleModules[i].Module.Code);
                if (source.Count == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsHaveProductModulePermissionById(params string[] ids)
        {
            if (ids.Length > 0)
            {
                List<dynamic> userModules = UserModuleService.GetListByIds(ids);
                if (userModules.Count == ids.Length)
                {
                    string prodCode = userModules[0].Product;
                    List<string> moduleCodes = new List<string>();
                    for (int i = 0; i < userModules.Count; i++)
                    {
                        dynamic item = userModules[i];
                        if (item.Product != prodCode)
                        {
                            return false;
                        }
                        moduleCodes.Add(item.Module);
                    }
                    return IsHaveProductModulePermission(prodCode, moduleCodes.ToArray());
                }
            }
            return false;
        }

        /// <summary>
        /// 当前用户是否拥有删除指定用户产品授权的能力
        /// </summary>
        /// <param name="userProdId">指定用户产品授权Id</param>
        /// <returns></returns>
        public bool IsHaveDeleteUserProductPermission(string userProdId)
        {
            dynamic userProd = UserProdService.GetUserProdById(userProdId);
            if (userProd != null)
            {
                dynamic user = UserService.GetUserByCode(userProd.User);
                if (user != null && user.Org == mEnv.Current.User.Org)  //是否和当前用户同一机构
                {
                    dynamic userProd2 = UserProdService.GetUserProdByCode(mEnv.Current.User.Code, userProd.Product);
                    return userProd2 != null && userProd2.Actived && !userProd2.Deleted;    //当前用户自身是否有该产品的有效授权
                }
            }
            return false;
        }
    }
}
