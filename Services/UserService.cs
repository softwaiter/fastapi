using CodeM.Common.Orm;
using CodeM.Common.Tools.Security;
using System;

namespace CodeM.FastApi.Services
{
    public class UserService
    {
        public static dynamic GetUserById(string id)
        {
            return OrmUtils.Model("User").Equals("Id", id).QueryFirst();
        }

        public static dynamic GetUserByCode(string code)
        {
            return OrmUtils.Model("User").Equals("Code", code).QueryFirst();
        }

        public static dynamic GetUserByMobile(string mobile)
        {
            return OrmUtils.Model("User").Equals("Mobile", mobile).QueryFirst();
        }

        public static dynamic GetUserByEmail(string email)
        {
            return OrmUtils.Model("User").Equals("Email", email).QueryFirst();
        }

        public static dynamic FindUser(string user)
        {
            dynamic result = OrmUtils.Model("User").Equals("Code", user)
                .Or(new SubFilter().Equals("Mobile", user))
                .Or(new SubFilter().Equals("Email", user))
                .QueryFirst();
            return result;
        }

        public static void UpdatePassword(string userId, string newPass)
        {
            dynamic updateObj = OrmUtils.Model("User").NewObject();
            updateObj.Id = userId;
            updateObj.Salt = HashUtils.MD5(Guid.NewGuid().ToString());
            updateObj.Password = HashUtils.SHA256(newPass + updateObj.Salt);
            updateObj.MustChangePassNow = true;
            OrmUtils.Model("User").Equals("Id", userId).SetValues(updateObj).Update();
        }
    }
}
