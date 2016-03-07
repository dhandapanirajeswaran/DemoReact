using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JsPlc.Ssc.PetrolPricing.Portal.Facade
{
	public class FacadeResponse<T>
	{
		public string ErrorMessage { get; set; }

		public T ViewModel { get; set; }
	}
}