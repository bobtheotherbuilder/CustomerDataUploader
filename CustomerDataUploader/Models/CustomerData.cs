using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

/// <summary>
/// Contains all related customer info        
/// </summary>
namespace CustomerDataUploader.Models
{
    public class CustomerData
    {
        [Display(Name = "Property Value")]
        public string property { get; set; }

        [Display(Name = "Customer Name")]
        public string customer { get; set; }

        [Display(Name = "Action Name")]
        public string action { get; set; }

        [Display(Name = "Action Value")]
        public int value { get; set; }

        [Display(Name = "File Name")]
        public string file { get; set; }

    }
}