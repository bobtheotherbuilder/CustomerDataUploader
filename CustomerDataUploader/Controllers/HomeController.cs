using CustomerDataUploader.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;

namespace CustomerDataUploader.Controllers
{
    public class HomeController : Controller
    {
        private const string SERVICEURL = "http://evilapi-env.ap-southeast-2.elasticbeanstalk.com/";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UploadFile(UploadFileViewModel vm)
        {
            return View(vm);
        }

        private List<CustomerData> ParseFile(HttpPostedFileBase file)
        {
            List<CustomerData> customers = new List<CustomerData>();
            using (var reader = new StreamReader(file.InputStream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        break;

                    string[] lineData = line.Split(',');
                    int custValue;
                    int.TryParse(lineData[1].Trim(), out custValue);
                    customers.Add(new CustomerData() { customer = lineData[0], value = custValue });
                }
            }

            return customers;
        }


        [HttpPost]
        public ActionResult UploadFile(FormCollection formCollection)
        {
            if (Request == null)
            {
                ViewBag.ErrorMessage = "Invalid request.";
                return View("Error");
            }

            HttpPostedFileBase file = Request.Files["fileName"];
            if ((file == null) || (file.ContentLength == 0) || string.IsNullOrEmpty(file.FileName))
            {
                ViewBag.ErrorMessage = "Please choose a valid csv file.";
                return View("Error");
            }

            List<CustomerData> customers = ParseFile(file);
            int successCout = 0;
            HttpClient client = new HttpClient();
            List<CustomerInfo> updatedCustomers = new List<CustomerInfo>();
            Parallel.ForEach(customers, cust =>
            {
                CustomerInfo custInfo = postCustDataAsync(cust, client).Result;
                if (custInfo != null)
                {
                    updatedCustomers.Add(custInfo);
                    if (custInfo.Hash != null) {
                        successCout += 1;
                    }                    
                }
            });

            client.Dispose();
            //ViewBag.Total = customers.Count;
            //ViewBag.Success = successCout;

            return View(new UploadFileViewModel()
            { customers = updatedCustomers, successCount=successCout });
        }

        private async Task<CustomerInfo> postCustDataAsync(CustomerData cust, HttpClient client)
        {
            string postBody = JsonConvert.SerializeObject(cust);
            var content = new StringContent(postBody, Encoding.UTF8, "application/json");
            CustomerInfo custInfo = new CustomerInfo(cust);
            // + "upload"
            HttpResponseMessage response = await client.PostAsync(new Uri(SERVICEURL+"upload"), content);
            if (response.IsSuccessStatusCode)
            {
                dynamic result = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                string added = result.added;
                if (!string.IsNullOrEmpty(added) && Boolean.Parse(added))
                {
                    custInfo.Hash = result.hash;
                }
                else {
                    custInfo.UploadErrors = new List<string>(result.errors);
                }
                return custInfo;
            }
            return null;
        }

    }
}