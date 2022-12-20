using BroomService.Resources;
using BroomService.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BroomService.Helpers
{
    public class Common
    {

        public const string BaseUrl = "https://app.bell-boy.com";
        public const string CreateClient = "https://api.icount.co.il/api/v3.php/client/create?cid={0}&user={1}&pass={2}&client_name={3}&email={4}";
        public const string DeleteClient = "https://api.icount.co.il/api/v3.php/client/delete?cid={0}&user={1}&pass={2}&client_id={3}";
        public const string GeneratSale = "https://api.icount.co.il/api/v3.php/paypage/generate_sale?cid={0}&user={1}&pass={2}&client_name={3}&paypage_id={4}&sum={5}&email={6}&Phone={7}&vat_id={8}&success_url={9}&failure_url={10}&ipn_url={11}&";
        public const string CreatePage = "https://api.icount.co.il/api/v3.php/paypage/create?cid={0}&user={1}&pass={2}&page_name={3}&currency_id={4}&";
        public const string DeletePage = "https://api.icount.co.il/api/v3.php/paypage/create?cid={0}&user={1}&pass={2}&paypage_id={3}";
        public const string GoogleMapPlacesKey = "AIzaSyCFomQKPynvwJ7o_aMUvFtfVfru2Wu-xCw";

        public static void GeneratePassword(string p, string userType, ref string _salt, ref string _password)
        {
            if (userType == "new")
            {
                _salt = FetchRandString(6);
            }
            _password = EncryptString(p, _salt);
        }

        public static string DecryptString(string EncryptedString, string salt)
        {
            if (EncryptedString == null)
                return null;
            if (EncryptedString == "")
                return "";

            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(salt));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the decoder
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] DataToDecrypt = Convert.FromBase64String(EncryptedString);

            // Step 5. Attempt to decrypt the string
            try
            {
                ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
                Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }

            // Step 6. Return the decrypted string in UTF8 format
            return UTF8.GetString(Results);
        }

        public static DeliveryDistanceViewModel CalculateDistanceTime(string origin, string destination)
        {
            var deliveryBookedVM = new DeliveryDistanceViewModel
            {
                Time = 10000,
                Distance = 10000
            };

            if (origin == null || destination == null) return deliveryBookedVM;

            try
            {
                var mode = "bicycling"; //driving, walking, bicycling
                string key = Convert.ToString(ConfigurationManager.AppSettings["GoogleMapApiKey"]);
                string url = "https://maps.googleapis.com/maps/api/distancematrix/xml?origins=" + origin + "&destinations=" + destination + "&mode=" + mode + "&key=" + key;
                WebRequest request = WebRequest.Create(url);
                using (WebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        DataSet dsResult = new DataSet();
                        dsResult.ReadXml(reader);
                        var duration = dsResult.Tables["duration"].Rows[0]["value"].ToString();
                        var distance = dsResult.Tables["distance"].Rows[0]["value"].ToString();
                        deliveryBookedVM.Time = Convert.ToInt32(duration) / 60;
                        deliveryBookedVM.Distance = Convert.ToInt32(distance) / 1000;
                    }
                }
                return deliveryBookedVM;
            }
            catch (Exception)
            {
                return deliveryBookedVM;
            }
        }

        public static string EncryptString(string Password, string salt)
        {

            if (Password == null)
                return null;
            if (Password == "")
                return "";

            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(salt));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the encoder
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] DataToEncrypt = UTF8.GetBytes(Password);

            // Step 5. Attempt to encrypt the string
            try
            {
                ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
                Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }

            // Step 6. Return the encrypted string as a base64 encoded string
            return Convert.ToBase64String(Results);
        }

        public static string FetchRandString(int size)
        {
            try
            {
                Random random = new Random();
                string combination = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                char[] passwords = new char[size];
                for (int i = 0; i < size; i++)
                {
                    passwords[i] = combination[random.Next(0, combination.Length)];
                }
                return new string(passwords);
            }
            catch
            {
                throw;
            }
        }

        public static string GenerateUsername(int size)
        {
            try
            {
                Random random = new Random();
                string combination = "abcdefghijklmnopqrstuvwxyz0123456789_";
                char[] username = new char[size];
                for (int i = 0; i < size; i++)
                {
                    username[i] = combination[random.Next(0, combination.Length)];
                }
                return new string(username);
            }
            catch
            {
                throw;
            }
        }

        public static bool SendSignupConfirmationEmail(string firstname, string email, string userID)
        {
            bool result = true;
            try
            {
                string _path = BaseUrl;
                string _controllerPath = "/Account/EmailVerification?userId=" + userID;

                var callbackUrl = "<a href=" + _path + _controllerPath + ">" + Resource.verify_email + "</a>";

                MailMessage mailmsg = new MailMessage();
                mailmsg.From = new MailAddress(ConfigurationManager.AppSettings["MAIL_FROM"].ToString());
                mailmsg.Subject = Resource.verify_email_broom_service;
                mailmsg.IsBodyHtml = true;
                mailmsg.Body = Resource.hello + " " + firstname + ",<br/><br/>";
                mailmsg.Body += "<b>" + Resource.click_on_link +
                        "<br/><br/>" + callbackUrl;
                mailmsg.Body += "<br />";
                mailmsg.Body += Resource.thank_you;

                AlternateView view = AlternateView.CreateAlternateViewFromString(mailmsg.Body, Encoding.UTF8, "text/html");
                mailmsg.AlternateViews.Add(view);

                string mailTo = email;
                MailAddress addressTo = new MailAddress(mailTo);
                if (mailTo.Length > 0)
                {
                    mailmsg.To.Add(addressTo.ToString());
                    var smtpHost = ConfigurationManager.AppSettings["SMTP_Host"].ToString();
                    var smtpPort = ConfigurationManager.AppSettings["SMTP_Port"].ToString();
                    SmtpClient SmtpServer = new SmtpClient(smtpHost);
                    SmtpServer.UseDefaultCredentials = true;
                    SmtpServer.EnableSsl = true;
                    SmtpServer.Port = !string.IsNullOrEmpty(smtpPort) ? Convert.ToInt32(smtpPort) : 0;
                    //Please enter here user password
                    SmtpServer.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["MAIL_FROM"].ToString(),
                        ConfigurationManager.AppSettings["MAIL_PWD"].ToString());
                    SmtpServer.Send(mailmsg);
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool SendEmailResetPassword(string firstname, string email, string password)
        {
            bool result = true;
            try
            {
                MailMessage mailmsg = new MailMessage();
                mailmsg.From = new MailAddress(ConfigurationManager.AppSettings["MAIL_FROM"].ToString());
                mailmsg.Subject = Resource.password_details;
                mailmsg.IsBodyHtml = true;
                mailmsg.Body = Resource.hello + " " + firstname + ",";
                mailmsg.Body += "<br />" + Resource.your_login_credentials;
                mailmsg.Body += "<br />";
                mailmsg.Body += Resource.email_post + " " + email + "<br />";
                mailmsg.Body += Resource.password_post + " " + password + "<br />";
                mailmsg.Body += "<br />";
                mailmsg.Body += Resource.thank_you;

                AlternateView view = AlternateView.CreateAlternateViewFromString(mailmsg.Body, Encoding.UTF8, "text/html");
                mailmsg.AlternateViews.Add(view);

                string mailTo = email;
                MailAddress addressTo = new MailAddress(mailTo);
                if (mailTo.Length > 0)
                {
                    mailmsg.To.Add(addressTo.ToString());
                    var smtpHost = ConfigurationManager.AppSettings["SMTP_Host"].ToString();
                    var smtpPort = ConfigurationManager.AppSettings["SMTP_Port"].ToString();
                    SmtpClient SmtpServer = new SmtpClient(smtpHost);
                    SmtpServer.UseDefaultCredentials = true;
                    SmtpServer.EnableSsl = true;
                    SmtpServer.Port = !string.IsNullOrEmpty(smtpPort) ? Convert.ToInt32(smtpPort) : 0;
                    //Please enter here user password
                    SmtpServer.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["MAIL_FROM"].ToString(),
                        ConfigurationManager.AppSettings["MAIL_PWD"].ToString());
                    SmtpServer.Send(mailmsg);
                }
                return result;
            }
            catch (Exception ex)
            {
                return result = false;
            }
        }

        public static bool SendEmailForGrantAccess(string email, string comment, string Link)
        {
            bool result = true;
            try
            {
                MailMessage mailmsg = new MailMessage();
                mailmsg.From = new MailAddress(ConfigurationManager.AppSettings["MAIL_FROM"].ToString());
                mailmsg.Subject = Resource.grant_access_title;

                mailmsg.IsBodyHtml = true;
                mailmsg.Body = "<html><head><title>" + Resource.grant_access_title + "</title></head><body style=" + "background-color:#38547f;" + "><table border=" + "0" + " cellpadding=" + "0" + " cellspacing=" + "0" + " width=" + "100%" + ">" +
                  "<tr><td align=" + "center" + " bgcolor=" + "#38547f" + "><table border=" + "0" + " cellpadding=" + "0" + " cellspacing=" + "0" + " width=" + "100%" + " style=" + "max-width:600px;" + "><tr><td align=" + "center" +
                  " valign=" + "top" + " style=" + "padding:15px 0px;" + "><a href=" + "#" + " target=" + "_blank" + " style=" + "display:inline-block;" + "><img src=" + "https://app.broomservice.co.il/Content/images/logo_small.png" +
                  " border=" + "0" + " style=" + "display:block;width:35%;" + "></a></td></tr></table></td></tr><tr><td align=" + "center" + " bgcolor=" + "#38547f" + "><table border=" + "0" + " cellpadding=" + "0" +
                  " cellspacing=" + "0" + " width=" + "100%" + " style=" + "max-width:600px;" + "><tr><td align=" + "left" + " bgcolor=" + "#ffffff" + " style=" + "padding:36px 24px 0;border-top:3px solid #d4dadf;" +
                  "><h1 style=" + "margin:0;font-size:32px;font-weight:700;cursor:pointer;letter-spacing:-1px;line-height:48px;" + ">" + Resource.grant_access_title + "</h1></td></tr></table></td></tr><tr><td align=" +
                  "center" + " bgcolor=" + "#38547f" + "><table border=" + "0" + " cellpadding=" + "0" + " cellspacing=" + "0" + " width=" + "100%" + " style=" + "max-width:600px;" +
                  "><tr><td align=" + "left" + " bgcolor=" + "#ffffff" + " style=" + "padding:24px;font-size:16px;line-height:24px;" +
                  "><p style=" + "margin:0;" + ">" + comment + "</p></td></tr><tr><td align=" +
                  "left" + " bgcolor=" + "#ffffff" + "><table border=" + "0" + " cellpadding=" + "0" + " cellspacing=" + "0" + " width=" + "100%" +
                  "><tr><td align=" + "center" + " bgcolor=" + "#ffffff" + " style=" + "padding:12px;" + "><table border=" + "0" + " cellpadding=" + "0" + " cellspacing=" + "0" + "><tr><td align=" + "center" + " bgcolor=" + "#38547f" +
                  " style=" + "border-radius:6px;" + "><a" + " href=" + Link + " style=" + "font-size:16px;color:#ffffff;text-decoration:none;border-radius:6px;display:inline-block;padding:16px 36px;" +
                  ">" + Resource.verify_email + "</a></td></tr></table></td></tr></table></td></tr><tr><td align=" + "left" + " bgcolor=" + "#ffffff" + " style=" + "padding:24px;font-size:16px;line-height:24px;" +
                  "><p style=" + "margin:0;" + ">" + Resource.link_not_working + "</p><p style=" + "margin:0;" + "><a href=" + "#" + ">" + Link + "</a></p></td></tr><tr>" +
                  "<td align=" + "left" + " bgcolor=" + "#ffffff" + " style=" + "padding:24px;font-size:16px;line-height:24px;border-bottom:3px solid #d4dadf" +
                  "><p style=" + "margin:0;" + ">" + Resource.best_regards + "<br>" + Resource.broom_service + "</p></td></tr></table></td></tr><tr><td align=" + "center" + " bgcolor=" + "#38547f" + " style=" + "padding:24px;" + "><table border=" + "0" +
                  " cellpadding=" + "0" + " cellspacing=" + "0" + " width=" + "100%" + " style=" + "max-width:600px;" + "><tr><td align=" + "center" + " bgcolor=" + "#38547f" + " style=" + "font-size:14px;line-height:20px;color:#fff;padding:12px 24px;" +
                  "><p style=" + "margin:0;" + ">" + Resource.delete_email_if_not_belong + "</p></td></tr></table></td></tr></table></body></html>";


                AlternateView view = AlternateView.CreateAlternateViewFromString(mailmsg.Body, Encoding.UTF8, "text/html");
                mailmsg.AlternateViews.Add(view);

                string mailTo = email;
                MailAddress addressTo = new MailAddress(mailTo);
                if (mailTo.Length > 0)
                {
                    mailmsg.To.Add(addressTo.ToString());
                    var smtpHost = ConfigurationManager.AppSettings["SMTP_Host"].ToString();
                    var smtpPort = ConfigurationManager.AppSettings["SMTP_Port"].ToString();
                    SmtpClient SmtpServer = new SmtpClient(smtpHost);
                    SmtpServer.UseDefaultCredentials = false;
                    SmtpServer.EnableSsl = true;
                    SmtpServer.Port = !string.IsNullOrEmpty(smtpPort) ? Convert.ToInt32(smtpPort) : 0;
                    //Please enter here user password
                    SmtpServer.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["MAIL_FROM"].ToString(),
                        ConfigurationManager.AppSettings["MAIL_PWD"].ToString());
                    SmtpServer.Send(mailmsg);
                }
                return result;
            }
            catch (Exception ex)
            {
                return result = false;
            }
        }


        public static bool SendReportToPropertyOwner(string subusername, string prop, string cat, string firstname, string email, string _controllerPath)
        {
            bool result = true;
            try
            {
                string _path = BaseUrl;
                //string _controllerPath = "/Account/EmailVerification?userId=" + userID;

                var callbackUrl = "<a href=" + _path + _controllerPath + ">Order Service</a>";

                MailMessage mailmsg = new MailMessage();
                mailmsg.From = new MailAddress(ConfigurationManager.AppSettings["MAIL_FROM"].ToString());
                mailmsg.Subject = "Fix & Install Order Request";
                mailmsg.IsBodyHtml = true;
                mailmsg.Body = Resource.hello + " " + firstname + ",<br/><br/>";
                mailmsg.Body += "<b> Please Order following service for Fix & Install</b>" 
                        +"<br/>" +
                        "Renter Name: " + subusername +
                        "<br/>" +
                        "Porperty Name: " + prop +
                        "<br/>" +
                        "Category Name: " + cat +
                        "<br/><br/>" + callbackUrl;
                mailmsg.Body += "<br />";
                mailmsg.Body += Resource.thank_you;

                AlternateView view = AlternateView.CreateAlternateViewFromString(mailmsg.Body, Encoding.UTF8, "text/html");
                mailmsg.AlternateViews.Add(view);

                string mailTo = email;
                MailAddress addressTo = new MailAddress(mailTo);
                if (mailTo.Length > 0)
                {
                    mailmsg.To.Add(addressTo.ToString());
                    var smtpHost = ConfigurationManager.AppSettings["SMTP_Host"].ToString();
                    var smtpPort = ConfigurationManager.AppSettings["SMTP_Port"].ToString();
                    SmtpClient SmtpServer = new SmtpClient(smtpHost);
                    SmtpServer.UseDefaultCredentials = true;
                    SmtpServer.EnableSsl = true;
                    SmtpServer.Port = !string.IsNullOrEmpty(smtpPort) ? Convert.ToInt32(smtpPort) : 0;
                    //Please enter here user password
                    SmtpServer.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["MAIL_FROM"].ToString(),
                        ConfigurationManager.AppSettings["MAIL_PWD"].ToString());
                    SmtpServer.Send(mailmsg);
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region Save Image/Video

        public static bool SaveImage(string ImgStr, string ImgName)
        {
            String path = System.Web.HttpContext.Current.Server.MapPath("~/Images/JobRequest/"); //Path

            //Check if directory exist
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
            }

            //set the image path
            string imgPath = Path.Combine(path, ImgName);

            byte[] imageBytes = Convert.FromBase64String(ImgStr);

            File.WriteAllBytes(imgPath, imageBytes);

            return true;
        }
        public static bool SavePropertyImage(string ImgStr, string ImgName)
        {
            String path = System.Web.HttpContext.Current.Server.MapPath("~/Images/Property/"); //Path

            //Check if directory exist
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
            }
            //set the image path
            string imgPath = Path.Combine(path, ImgName);

            byte[] imageBytes = Convert.FromBase64String(ImgStr);

            File.WriteAllBytes(imgPath, imageBytes);

            return true;
        }

        public static string ConvertImageString(string url)
        {
            string file = string.Empty;
            try
            {
                String path = System.Web.HttpContext.Current.Server.MapPath("~/Images/Property/"); //Path

                string imgPath = Path.Combine(path, url);


                Byte[] bytes = !string.IsNullOrEmpty(imgPath) ? File.ReadAllBytes(imgPath) : null;

                file = bytes != null ? Convert.ToBase64String(bytes) : string.Empty;
            }
            catch (Exception)
            {

            }
            
            return file;
        }

        #endregion

        #region Notification

        public static string PushNotification(int? usertype, int? deviceid, string devicetoken, string message)
        {
            if (!string.IsNullOrEmpty(devicetoken))
            {
                string appTitle = usertype == (int)Enums.UserTypeEnum.Customer ? Resource.broom_service : "bs_crew";
                string serverKey = string.Empty;
                // serverKey = "AAAAotg6m_0:APA91bHaXoRugZ0YrNVjrSWlDk8F7KdgShWAKH-TCnA4OysL44qlk6Nc9xZjT6WCA_PIr46ioez_1H7ZpE7jrmOQkhCJ0ETdPHezHPmKFWyzZ26wX7dUmblvfImGHxnN9gwOXwWe_qyu";
               

                if(usertype == (int)Enums.UserTypeEnum.Customer)
                {
                    serverKey = "AAAAotg6m_0:APA91bHaXoRugZ0YrNVjrSWlDk8F7KdgShWAKH-TCnA4OysL44qlk6Nc9xZjT6WCA_PIr46ioez_1H7ZpE7jrmOQkhCJ0ETdPHezHPmKFWyzZ26wX7dUmblvfImGHxnN9gwOXwWe_qyu";
                }
                else
                {
                    serverKey = "AAAAVRNtLW8:APA91bGZmIvwtAMPTR_p25oEjTlQVFnsOg1U8wF9gzgBIGeSQjOk899EMMVMZqX5thV3rC9JWpaUBRZmH6XlBWFzS8ZNwTe3SAle1BnMUWPGyJUNgJDehn7fWznoJXgv4j4_9CWgg4SA";
                }

                //USE FOR Customer Key
                //serverKey = "AAAAotg6m_0:APA91bHaXoRugZ0YrNVjrSWlDk8F7KdgShWAKH-TCnA4OysL44qlk6Nc9xZjT6WCA_PIr46ioez_1H7ZpE7jrmOQkhCJ0ETdPHezHPmKFWyzZ26wX7dUmblvfImGHxnN9gwOXwWe_qyu";

                //Use for Worker key
                //serverKey = "AAAAVRNtLW8:APA91bGZmIvwtAMPTR_p25oEjTlQVFnsOg1U8wF9gzgBIGeSQjOk899EMMVMZqX5thV3rC9JWpaUBRZmH6XlBWFzS8ZNwTe3SAle1BnMUWPGyJUNgJDehn7fWznoJXgv4j4_9CWgg4SA";


                var result21 = "-1";
                var webAddr = "https://fcm.googleapis.com/fcm/send";
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(webAddr);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Headers.Add("Authorization:key=" + serverKey);
                httpWebRequest.Method = "POST";
                using (var streamWriter = new System.IO.StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = string.Empty;
                    if (deviceid == 1)
                    {
                        json = "{\"to\":\"" + devicetoken + "\",\"notification\":{\"title\":\"" + appTitle + "\",\"body\":\"" + message + "\"},\"priority\":\"high\"}";
                    }
                    else
                    {
                        json = "{\"to\": \"" + devicetoken + "\",\"notification\": {\"body\": \"" + message + "\",\"sound\": \"default\",\"content_available\": true}}";
                    }
                    streamWriter.Write(json);
                    streamWriter.Flush();
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                {
                    result21 = streamReader.ReadToEnd();
                }
                return result21;
            }
            return null;
        }
        
        #endregion

    }
}