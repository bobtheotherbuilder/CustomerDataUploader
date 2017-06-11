using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

/// <summary>
/// Contains basic customer data plus the hash or upload errors        
/// </summary>
namespace CustomerDataUploader.Models
{
    public class CustomerInfo
    {
        public CustomerData CustomerData { get; set; }
        
        public string Hash { get; set; }

        public List<string> UploadErrors;

        public CustomerInfo(CustomerData customerData)
        {
            CustomerData = customerData;
        }
    }
}