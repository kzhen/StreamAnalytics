using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web
{
	public class IndexModule : NancyModule
	{
		public IndexModule()
		{
			Get["/"] = _ =>
				{
					return View["index"];
				};
		}
	}
}