﻿using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Cors;

[assembly: OwinStartup(typeof(Web.Startup))]

namespace Web
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			app.UseNancy();
		}
	}
}
