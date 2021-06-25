﻿using CodeM.Common.Orm;

namespace CodeM.FastApi.Services
{
    public class OrgService
    {
        public static dynamic GetOrgById(string id)
        {
            return OrmUtils.Model("Organization").Equals("Id", id).QueryFirst();
        }

        public static dynamic GetOrgByCode(string code)
        { 
            return OrmUtils.Model("Organization").Equals("Code", code).QueryFirst();
        }

        public static dynamic GetOrgByName(string name)
        {
            return OrmUtils.Model("Organization").Equals("Name", name).QueryFirst();
        }
    }
}