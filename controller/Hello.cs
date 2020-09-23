﻿using CodeM.FastApi.Context;
using System.Threading.Tasks;

namespace CodeM.FastApi.Controller
{
    public class Hello
    {

        public async Task Handle(ControllerContext cc)
        {
            await cc.JsonAsync("Hello World.");
        }

    }
}
