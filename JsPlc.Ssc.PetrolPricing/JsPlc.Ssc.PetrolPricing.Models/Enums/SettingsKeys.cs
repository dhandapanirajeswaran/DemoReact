using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Models.Enums
{
    public enum SettingsKeys
    {
        UploadPath = 1,
        SomeOtherVal = 2
    }

    public enum EmailSendMode
    {
        Test = 0,
        Live = 1
    }

    public enum FuelTypeItem
    {
        Super_Unleaded = 1,
        Unleaded = 2,
        Unknown1 = 3,
        Unknown2 = 4,
        Super_Diesel = 5,
        Diesel = 6,
        LPG = 7
    }
}
