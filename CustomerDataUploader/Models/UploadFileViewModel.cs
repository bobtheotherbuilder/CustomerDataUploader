﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomerDataUploader.Models
{
    public class UploadFileViewModel
    {
        public List<CustomerInfo> customers;
        public string fileName;
        public int total;
        public int successCount;

    }
}