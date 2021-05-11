using CodeM.Common.Orm;
using CodeM.Common.Tools.Security;
using System;
using System.Collections.Generic;

namespace CodeM.FastApi.Services
{
    public class RegisterSaasAccountService
    {
        public static dynamic Register(string mobile, string email, string orgName, string name, string pass, dynamic prodObj)
        {
            int transCode = OrmUtils.Model("User").GetTransaction();
            try
            {
                dynamic newOrg = OrmUtils.Model("Organization").NewObject();
                newOrg.Code = "test";
                newOrg.Name = orgName;
                newOrg.Person = name;
                newOrg.Mobile = mobile;
                newOrg.Email = email;
                OrmUtils.Model("Organization").SetValues(newOrg).Save(transCode);

                dynamic newUser = OrmUtils.Model("User").NewObject();
                newUser.Org = newOrg.Code;
                newUser.Code = "test";
                newUser.Name = name;
                newUser.Mobile = mobile;
                newUser.Email = email;
                newUser.Expires = new DateTime(9999, 12, 31);
                newUser.Salt = HashUtils.MD5(Guid.NewGuid().ToString());
                newUser.Password = HashUtils.SHA256(pass + newUser.Salt);
                OrmUtils.Model("User").SetValues(newUser).Save(transCode);

                List<dynamic> basicProducts = ProductService.GetBasicEffectiveList();
                foreach (dynamic prod in basicProducts)
                {
                    dynamic newOrgProduct = OrmUtils.Model("OrgProduct").NewObject();
                    newOrgProduct.Org = newOrg.Code;
                    newOrgProduct.Product = prod.Code;
                    newOrgProduct.Users = prod.FreeUsers;
                    newOrgProduct.Expires = new DateTime(9999, 12, 31);
                    OrmUtils.Model("OrgProduct").SetValues(newOrgProduct).Save(transCode);

                    dynamic newUserProduct = OrmUtils.Model("UserProduct").NewObject();
                    newUserProduct.User = newUser.Code;
                    newUserProduct.Product = prod.Code;
                    newUserProduct.Expires = new DateTime(9999, 12, 31);
                    OrmUtils.Model("UserProduct").SetValues(newUserProduct).Save(transCode);

                    dynamic newUserRole = OrmUtils.Model("UserRole").NewObject();
                    newUserRole.User = newUser.Code;
                    newUserRole.Role = prodObj.AdminRole;
                    OrmUtils.Model("UserRole").SetValues(newUserRole).Save(transCode);

                    if (prodObj != null && prodObj.Code == prod.Code)
                    {
                        prodObj = null;
                    }
                }

                if (prodObj != null)
                {
                    dynamic newOrgProduct = OrmUtils.Model("OrgProduct").NewObject();
                    newOrgProduct.Org = newOrg.Code;
                    newOrgProduct.Product = prodObj.Code;
                    newOrgProduct.Users = prodObj.FreeUsers;
                    newOrgProduct.Expires = DateTime.Now.AddDays(prodObj.FreeDays);
                    OrmUtils.Model("OrgProduct").SetValues(newOrgProduct).Save(transCode);

                    dynamic newUserProduct = OrmUtils.Model("UserProduct").NewObject();
                    newUserProduct.User = newUser.Code;
                    newUserProduct.Product = prodObj.Code;
                    newUserProduct.Expires = new DateTime(9999, 12, 31);
                    OrmUtils.Model("UserProduct").SetValues(newUserProduct).Save(transCode);

                    dynamic newUserRole = OrmUtils.Model("UserRole").NewObject();
                    newUserRole.User = newUser.Code;
                    newUserRole.Role = prodObj.AdminRole;
                    OrmUtils.Model("UserRole").SetValues(newUserRole).Save(transCode);
                }

                OrmUtils.CommitTransaction(transCode);

                return newUser;
            }
            catch (Exception exp)
            {
                OrmUtils.RollbackTransaction(transCode);
                throw exp;
            }
        }
    }
}
