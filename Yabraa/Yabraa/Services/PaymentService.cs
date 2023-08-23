using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Crypto.Macs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Yabraa.DTOs;
using YabraaEF;
using YabraaEF.Models;

namespace Yabraa.Services
{
    public class PaymentService
    {
        private ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public PaymentService(ApplicationDbContext dbContext, IConfiguration configuration)
        {

            _context = dbContext;
            _configuration = configuration;
        }
        public async Task<string> Payment(PaymentDto paymentDto, string userId,decimal amount, string currency, string paymentType)
        {
            DateTime dateTime = DateTime.Now;
            var status = _context.VisitStatuses.FirstOrDefault(c => c.Name.Contains("Pending"));
          
            if ( !string.IsNullOrWhiteSpace(userId) && status is not null)
            {
               
               
                  
                    Invoice invoice = new Invoice()
                    {
                        CreateDT = dateTime,
                        LocationAltitude = paymentDto.locationAltitude,
                        LocationLongitude = paymentDto.locationLongitude,
                        LocationLatitude = paymentDto.locationLatitude,
                        PaymentMethodId = paymentDto.PaymentMethodId,
                        UserId = userId,
                        TotalPrice = paymentDto.amount,
                      //  CheckoutId = checkoutId,    
                    };
                    _context.Invoices.Add(invoice);
                    //_context.SaveChanges();
                    foreach (var package in paymentDto.packages)
                    {
                        InvoiceDetails invoiceDetails = new InvoiceDetails()
                        {
                            Count = 1,
                            InvoiceId = invoice.InvoiceId,
                            Invoice = invoice,
                            PackageId = package.packageId,
                            Price = package.price,

                        };
                        _context.InvoiceDetails.Add(invoiceDetails);
                        var serviceTypeId = _context.Packages.Where(c => c.PackageId == package.packageId).Include(c => c.Service).FirstOrDefault()?.Service.ServiceTypeId;
                        //_context.SaveChanges();
                        VisitDetails visitDetails = new VisitDetails()
                        {
                            ApplicationUserId = userId,
                            PackageId = package.packageId,
                            InvoiceDetailsId = invoiceDetails.InvoiceDetailsId,
                            InvoiceDetails = invoiceDetails,
                            LocationAltitude = paymentDto.locationAltitude,
                            LocationLongitude = paymentDto.locationLongitude,
                            LocationLatitude = paymentDto.locationLatitude,
                            UserFamilyId = package.userFamilyId,
                            Notes = package.notes,
                            VisitDT = package.dateTime,
                            VisitStatusId = status.VisitStatusId,
                            ServiceTypeId = serviceTypeId.Value

                        };
                        _context.VisitDetails.Add(visitDetails);
                        //_context.SaveChanges();
                        //invoiceDetails.VisitDetails.Add(visitDetails);
                        //invoice.InvoiceDetails.Add(invoiceDetails);
                    }

                    //_context.Invoices.Add(invoice);
                    int statusOperation = _context.SaveChanges();
                    if (statusOperation > 0)
                    {
                        var checkout = GetCheckoutId(amount, currency, paymentType,invoice.InvoiceId,paymentDto.PaymentMethodId);
                        if (checkout != null && checkout.Count > 0)
                        {
                            string checkoutId = checkout["id"];
                            invoice.CheckoutId=checkoutId;
                            _context.SaveChanges();
                            return checkoutId;
                        }
                      
                    }
              
               
            }

            return null;
        }
        string getentityId( int PaymentMethodId)
        {
          var PaymentMethod =  _context.PaymentMethods.FirstOrDefault(c => c.PaymentMethodId == PaymentMethodId);
            if (PaymentMethod != null)
            {
                if (PaymentMethod.NameEN== "Apple_Pay")
                {
                    return "8ac7a4c889bff1b60189c946252f04be";
                }
                else if (PaymentMethod.NameEN == "VISA")
                {
                    return "8ac7a4c889bff1b60189c941f49d04b1";
                }
                else if (PaymentMethod.NameEN == "MASTER")
                {
                    return "8ac7a4c889bff1b60189c941f49d04b1";
                }
                else if (PaymentMethod.NameEN == "MADA")
                {
                    return "8ac7a4c889bff1b60189c94298a804b5";
                }
            }
            return null;
        }
        public Dictionary<string, dynamic> GetCheckoutId(decimal amount, string currency, string paymentType,long invoiceId,int PaymentMethodId)
        {
            Dictionary<string, dynamic> responseData;

            string entityId = getentityId(PaymentMethodId);
            //remove customParameters[3DS2_enrolled] in live mode
            string data = $"entityId={entityId}&amount={amount}&currency={currency}&paymentType={paymentType}&customParameters[3DS2_enrolled]=true&merchantTransactionId={invoiceId}";

            //   string url = "https://test.oppwa.com/v1/checkouts";
            string url = "https://eu-test.oppwa.com/v1/checkouts";
            
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
        //    request.Headers["Authorization"] = "Bearer OGE4Mjk0MTc0ZDA1OTViYjAxNGQwNWQ4MjllNzAxZDF8OVRuSlBjMm45aA==";
            request.Headers["Authorization"] = "Bearer OGFjN2E0Yzg4OWJmZjFiNjAxODljOTQwZDI1OTA0YWR8cHJmMkJ5SlE0Zg==";

            request.ContentType = "application/x-www-form-urlencoded";
            Stream PostData = request.GetRequestStream();
            PostData.Write(buffer, 0, buffer.Length);
            PostData.Close();
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                HttpStatusCode statusCode = response.StatusCode;
                Console.WriteLine(statusCode);
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                 responseData = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(reader.ReadToEnd());
                reader.Close();
                dataStream.Close();
            }
            return responseData;
        }
        public Dictionary<string, dynamic> GetCheckoutStatus(string CheckoutId, int PaymentMethodId)
        {
            //decimal amount = 500;
            //string currency = "EUR";
            //string type = "DB";
            Dictionary<string, dynamic> responseData;
            string entityId = getentityId(PaymentMethodId);

            string data = $"entityId={entityId}";
           //string data = $"entityId=8a8294174d0595bb014d05d82e5b01d2&amount={amount}&currency={currency}&paymentType={type}";

            string url = $"https://eu-test.oppwa.com/v1/checkouts/{CheckoutId}/payment?" + data;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            request.Headers["Authorization"] = "Bearer OGFjN2E0Yzg4OWJmZjFiNjAxODljOTQwZDI1OTA0YWR8cHJmMkJ5SlE0Zg==";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                //var s = new JavaScriptSerializer();
                //responseData = s.Deserialize<Dictionary<string, dynamic>>(reader.ReadToEnd());
                responseData = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(reader.ReadToEnd());

                reader.Close();
                dataStream.Close();
            }

            if (responseData is not null)
            {
                string result = responseData["result"]["code"];
                string pattern = @"^(000\.000\.|000\.100\.1|000\.[36]|000\.400\.[1][12]0)";
                bool isMatch = Regex.IsMatch(result, pattern);
                string pattern2 = @"^(000\.400\.0[^3]|000\.400\.100)";
                bool isMatch2 = Regex.IsMatch(result, pattern2);
                if (isMatch|| isMatch2)
                {
                    var invoice = _context.Invoices.FirstOrDefault(c => c.CheckoutId == CheckoutId);
                    if (invoice is not null)
                    {
                        invoice.Paid = true;
                        _context.SaveChanges();

                    }
                }

            }
           

            return responseData;
        }
        public async Task<object> GetTransactionInfo2(string checkoutId)
        {
           
            string merchantId = "8a8294174d0595bb014d05d82e5b01d2";
            string accessToken = "OGE4Mjk0MTc0ZDA1OTViYjAxNGQwNWQ4MjllNzAxZDF8OVRuSlBjMm45aA==";

            // إعداد واجهة الاستعلام للحصول على معلومات المعاملة
            string url = $"https://test.oppwa.com/v1/checkouts/{checkoutId}/payment";
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            // إرسال طلب الاستعلام
            HttpResponseMessage response = await client.GetAsync(url + "?entityId=" + merchantId);
            string jsonResponse = await response.Content.ReadAsStringAsync();

            // تحليل استجابة الاستعلام
        //    dynamic transactionInfo = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonResponse);
           var transactionInfo = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonResponse);

            // الحصول على معلومات المعاملة
            //string invoiceId = transactionInfo.id;
            //decimal amount = transactionInfo.amount.value;
            //string status = transactionInfo.status;

            // استخدام معلومات المعاملة كما تحتاج
            //Console.WriteLine($"Invoice ID: {invoiceId}");
            //Console.WriteLine($"Amount: {amount}");
            //Console.WriteLine($"Status: {status}");

            Console.WriteLine(transactionInfo);
            return transactionInfo;
        }

        public string GetTransactionInfo(string CheckoutId)
        {
            //Dictionary<string, dynamic> responseData;
            //string data = "entityId=8a8294174d0595bb014d05d82e5b01d2";
            //string url = $"https://test.oppwa.com/v3/query/{CheckoutId}?" + data;
            //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            //request.Method = "GET";
            //request.Headers["Authorization"] = "Bearer OGE4Mjk0MTc0ZDA1OTViYjAxNGQwNWQ4MjllNzAxZDF8OVRuSlBjMm45aA==";
            //using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            //{
            //    Stream dataStream = response.GetResponseStream();
            //    StreamReader reader = new StreamReader(dataStream);
            //    //var s = new JavaScriptSerializer();
            //    //responseData = s.Deserialize<Dictionary<string, dynamic>>(reader.ReadToEnd());
            //    responseData = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(reader.ReadToEnd());
            //    reader.Close();
            //    dataStream.Close();
            //}
            //return responseData;
            var client = new HttpClient();
            var uri = "https://test.oppwa.com/payment/rest/payments/lookup.json";
            var payload = "{\"checkoutId\":\"" + CheckoutId + "\"}";
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = client.PostAsync(uri, content).Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;
          //  var data = JsonConvert.DeserializeObject<object>(responseContent);
            //var data2 = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(responseContent);
           var data3 = JsonConvert.DeserializeObject<string>(responseContent);
            return data3;
          

        }
        public async Task<List<Payments>> GetHistoryPayment( string userId)
        {           
           var dates = _context.VisitDetails.Where(c => c.ApplicationUserId == userId && c.InvoiceDetails.Invoice.Paid)
               .Include(c => c.ApplicationUser).Include(c => c.VisitStatus).Include(c => c.UserFamily).Include(c => c.Package.Service).Include(c => c.InvoiceDetails.Invoice)
               .Select(c => new HistoryVisitsDTO
               {
                   PackageNameAR = c.Package.NameAR,
                   PackageNameEN = c.Package.NameEN,
                   ServiceAR = c.Package.Service.NameAR,
                   ServiceEN = c.Package.Service.NameEN,
                   Price =  c.InvoiceDetails.Price,                  
                   VisitDT = c.VisitDT.Date.ToString(),
                   Status = c.VisitStatus.Name,               
                 
               }).OrderByDescending(c => c.VisitDT).ToList();


             Dictionary<string, List<HistoryVisitsDTO>> visitsByDate = dates.GroupBy(visit => visit.VisitDT).ToDictionary(group => group.Key, group => group.ToList());
            List<Payments> payments = new List<Payments>();    
            foreach (KeyValuePair<string, List<HistoryVisitsDTO>> kvp in visitsByDate)
            {
                payments.Add(new Payments
                {
                    Date = kvp.Key, 
                    Items=kvp.Value
                });
            }

            return payments;
        }
        public async Task<List<PaymentMethodDTO>> GetPaymentMethods()
        {
            var PaymentMethodDTOs = _context.PaymentMethods
                         .Select(c => new PaymentMethodDTO
                         {
                  
                             Name = c.NameEN,
                             PaymentMethodId = c.PaymentMethodId
                         }).ToList();         

            return PaymentMethodDTOs;
        }

        //public object FormatAppointments()
        //{
        //   string userId = "c68a8973-b486-4ea4-af94-d9bab7d7abe1";
        //    var dates =  _context.VisitDetails.Where(c => c.ApplicationUserId == userId)
        //        .Include(c => c.ApplicationUser).Include(c => c.VisitStatus).Include(c => c.UserFamily).Include(c => c.Package.Service).Include(c => c.InvoiceDetails.Invoice)
        //        .Select(c => new HistoryVisitsDTO
        //        {
        //            //PackageNameAR = c.Package.NameAR,
        //            PackageNameEN = c.Package.NameEN,
        //            ServiceAR = c.Package.Service.NameAR,
        //            PackageId = c.Package.PackageId,
        //           // ServiceEN = c.Package.Service.NameEN,
        //            Price = c.InvoiceDetails.Price,
        //            Date = c.VisitDT.Date,
        //           // Status = c.VisitStatus.Name,
        //           // AppointmentId = c.VisitDetailsId
        //        }).ToList();
        //    var filteredAppointments = dates.Join(_context.Packages.ToList(), appointment => appointment.PackageId, package => package.PackageId, (appointment, package) => new { Appointment = appointment, Package = package });
        //    var results = filteredAppointments.Join(_context.Services.ToList(), appointment => appointment.Package.ServiceId, service => service.ServiceId, (appointment, service) => new { Appointment = appointment, Package = appointment.Package, Service = service });

        //    var output = new List<object>();

        //    foreach (var result in results)
        //    {
        //        output.Add(new { Appointment = result.Appointment, Service = result.Service, Package = result.Package });
        //    }

        //    // Send the data to the mobile developer
        //    var json = JsonConvert.SerializeObject(output);
        //    //using (var stream = new MemoryStream())
        //    //{
        //    //    stream.Write(json.ToUtf8Bytes());
        //    //    var response = new HttpResponseMessage(HttpStatusCode.OK);
        //    //    response.Content = new ByteArrayContent(stream.ToArray());
        //    //    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        //    //    Console.WriteLine(json);
        //    //}

        //    return json;
        //}
    }
    public class Payments
    {
        public string Date { get; set; }
        public List<HistoryVisitsDTO> Items { get; set; }
    }
   

  
}
