using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Core
{
    /// <summary>
    /// Defines an <see cref="AutoMapper"/> mappings based
    /// initialisations contract.
    /// </summary>
    public interface IAutoMapperInitialise
    {
        /// <summary>
        /// Initialises the instance.
        /// </summary>
        void Initialise();
    }
}
