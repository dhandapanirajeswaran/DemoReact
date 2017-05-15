﻿using JsPlc.Ssc.PetrolPricing.Models;
using JsPlc.Ssc.PetrolPricing.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsPlc.Ssc.PetrolPricing.Business.Services
{
    public class SystemSettingsService : ISystemSettingsService
    {
        private IPetrolPricingRepository _repository;

        public SystemSettingsService(IPetrolPricingRepository repository)
        {
            _repository = repository;
        }

        public SystemSettings GetSystemSettings()
        {
            return _repository.GetSystemSettings();
        }

        public SystemSettings UpdateSystemSettings(SystemSettings settings)
        {
            _repository.UpdateSystemSettings(settings);
            return _repository.GetSystemSettings();
        }
    }
}
