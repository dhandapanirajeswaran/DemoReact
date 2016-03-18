using JsPlc.Ssc.PetrolPricing.IntegrationTests.Core;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace JsPlc.Ssc.PetrolPricing.IntegrationTests.Portal.Steps
{
	public abstract class StepsBase
	{
		public Mock<IPrincipal> MockPrincipal;
		public Mock<HttpContextBase> MockContext;
		public Mock<ControllerContext> MockControllerContext;
		public HttpTestSessionState Session = new HttpTestSessionState();
		public const string TestUserName = "Integration tests";

		public StepsBase()
		{
			MockPrincipal = new Mock<IPrincipal>();
			MockPrincipal.SetupGet(p => p.Identity.Name).Returns(TestUserName);
			
			MockContext = new Mock<HttpContextBase>();
			MockContext.SetupGet(con => con.User).Returns(MockPrincipal.Object);
			MockContext.SetupGet(con => con.Session).Returns(Session);

			MockControllerContext = new Mock<ControllerContext>();
			MockControllerContext.SetupGet(con => con.HttpContext).Returns(MockContext.Object);
		}
	}
}
