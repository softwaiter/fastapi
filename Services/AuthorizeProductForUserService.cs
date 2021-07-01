using CodeM.Common.Orm;
using System;

namespace CodeM.FastApi.Services
{
    public class AuthorizeProductForUserService
    {
        public static void Auth(string currOrgCode, string currUserCode, string userCode, string prodCode)
        {
            dynamic user = UserService.GetUserByCode(userCode);
            if (user == null)
            {
                throw new Exception("被授权用户无效。");
            }

            dynamic prod = ProductService.GetProductByCode(prodCode);
            if (prod == null)
            {
                throw new Exception("授权产品无效。");
            }

            if (user.Org != currOrgCode)
            {
                throw new Exception("不能为非本机构用户授权。");
            }

            dynamic orgProd = OrgProdService.GetOrgProdByCode(currOrgCode, prodCode);
            if (orgProd == null)
            {
                throw new Exception("当前登录用户所属机构无指定产品权限。");
            }

            dynamic userProd = UserProdService.GetUserProdByCode(currUserCode, prodCode);
            if (userProd == null)
            {
                throw new Exception("当前登录用户无指定产品权限。");
            }

            long currOrgProdUsers = UserProdService.GetUserProdCountByCode(currOrgCode, prodCode);
            if (currOrgProdUsers >= orgProd.Users)
            {
                throw new Exception("已授权人数达到机构允许上限。");
            }

            dynamic newObj = OrmUtils.Model("UserProduct").NewObject();
            newObj.User = userCode;
            newObj.Product = prodCode;
            newObj.Expires = new DateTime(9999, 12, 31);
            OrmUtils.Model("UserProduct").SetValues(newObj).Save();
        }
    }
}
