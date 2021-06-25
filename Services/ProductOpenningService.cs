using CodeM.Common.Orm;
using System;

namespace CodeM.FastApi.Services
{
    public class ProductOpenningService
    {
        public static void Open(string orgCode, string userCode, dynamic prodObj)
        {
            int transCode = OrmUtils.Model("User").GetTransaction();
            try
            {
                dynamic newOrgProduct = OrmUtils.Model("OrgProduct").NewObject();
                newOrgProduct.Org = orgCode;
                newOrgProduct.Product = prodObj.Code;
                newOrgProduct.Users = prodObj.FreeUsers;
                newOrgProduct.Expires = DateTime.Now.AddDays(prodObj.FreeDays);
                OrmUtils.Model("OrgProduct").SetValues(newOrgProduct).Save(transCode);

                dynamic newUserProduct = OrmUtils.Model("UserProduct").NewObject();
                newUserProduct.User = userCode;
                newUserProduct.Product = prodObj.Code;
                newUserProduct.Expires = new DateTime(9999, 12, 31);
                OrmUtils.Model("UserProduct").SetValues(newUserProduct).Save(transCode);

                dynamic newUserRole = OrmUtils.Model("UserRole").NewObject();
                newUserRole.User = userCode;
                newUserRole.Role = prodObj.AdminRole;
                OrmUtils.Model("UserRole").SetValues(newUserRole).Save(transCode);

                OrmUtils.CommitTransaction(transCode);
            }
            catch (Exception exp)
            {
                OrmUtils.RollbackTransaction(transCode);
                throw exp;
            }
        }
    }
}
