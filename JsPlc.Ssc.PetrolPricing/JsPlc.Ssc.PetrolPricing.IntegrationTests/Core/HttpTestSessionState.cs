using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace JsPlc.Ssc.PetrolPricing.IntegrationTests.Core
{
	public class HttpTestSessionState : HttpSessionStateBase
	{
		Dictionary<string, object> session = new Dictionary<string, object>();
		
		public override object this[string name]
		{
			get
			{
				return session.ContainsKey(name) ? session[name] : null;
			}
			set
			{
				if (session.ContainsKey(name))
				{
					session[name] = value;
				}
				else
				{
					session.Add(name, value);
				}
			}
		}
	}
}
