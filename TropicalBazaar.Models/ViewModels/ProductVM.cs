﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TropicalBazaar.Models.ViewModels
{
    public class ProductVM
    {
        public Product Product { get; set; }


        [ValidateNever]
        public IEnumerable<SelectListItem> CategoryList { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> UnitList { get; set; }


    }
}
