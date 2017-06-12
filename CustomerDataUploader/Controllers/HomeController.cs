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

        [HttpPost]
        public async Task<ActionResult> UploadFileAsync(FormCollection formCollection)
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

            var tasks = customers.Select(async cust =>
            {
                CustomerInfo custInfo = new CustomerInfo(cust);
                bool successUpload = await postCustLineData(custInfo, client);

                if (successUpload)
                {
                    {
                        updatedCustomers.Add(custInfo);
                        successCout += 1;
                    }
                }
            });
            await Task.WhenAll(tasks);

            client.Dispose();
            UploadFileViewModel vm = new UploadFileViewModel()
                { customers = updatedCustomers, total = customers.Count, successCount = successCout, fileName =file.FileName };
            return View("UploadFile", vm);
        }


        public ActionResult SearchData(string info)
        {
            ViewBag.Customer = info;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SearchDataAsync(string searchHash)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync($"{SERVICEURL}check?hash={searchHash}");
            if (response.IsSuccessStatusCode)
            {
                dynamic result = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                ViewBag.Info = result.customer;
            }
            else
            {
                ViewBag.Info = "Hash not found";
            }

            return View("SearchData");
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

        private async Task<bool> postCustLineData(CustomerInfo custInfo, HttpClient client)
        {
            string postBody = JsonConvert.SerializeObject(custInfo.CustomerData);
            var content = new StringContent(postBody, Encoding.UTF8, "application/json");
            
            HttpResponseMessage response = await client.PostAsync(new Uri(SERVICEURL + "upload"), content);
            if (response.IsSuccessStatusCode)
            {                
                Console.WriteLine(custInfo.CustomerData.customer);
                dynamic result = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
                string added = result.added;
                if (!string.IsNullOrEmpty(added) && Boolean.Parse(added))
                {
                    custInfo.Hash = result.hash;
                    return true;
                }
                else
                {
                    custInfo.UploadErrors = new List<string>(result.errors);
                }
            }
            return false;
        }
    }
}