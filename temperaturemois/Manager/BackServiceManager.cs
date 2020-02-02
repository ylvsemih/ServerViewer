using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using temperaturemois.Models;
using temperaturemois.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols;
using System.Configuration;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using IdentityModel.Client;
using OpenQA.Selenium.Remote;
using System.IO;

namespace temperaturemois.Manager
{

    public class BackServiceManager
    {
        public class TEST
        {
            public int MaxTemp { get; set; }
            public int MinTemp { get; set; }
            public int MaxMois { get; set; }
            public int MinMois { get; set; }
            public string email { get; set; }
            public int CustomerID { get; set; }
            public string Phone { get; set; }
            public bool EmailNotification_TempUp { get; set; }
            public bool EmailNotification_TempDown { get; set; }
            public bool EmailNotification_MoisUp { get; set; }
            public bool EmailNotification_MoisDown { get; set; }
            public bool SmsNotification_TempUp { get; set; }
            public bool SmsNotification_TempDown { get; set; }
            public bool SmsNotification_MoisUp { get; set; }
            public bool SmsNotification_MoisDown { get; set; }
            public bool IvrNotification_TempUp { get; set; }
            public bool IvrNotification_TempDown { get;set; }
            public bool IvrNotification_MoisUp { get; set; }
            public bool IvrNotification_MoisDown { get; set; }
            public bool CallNotification_TempUp { get; set; }
            public bool CallNotification_TempDown { get; set; }
            public bool CallNotification_MoisUp { get; set; }
            public bool CallNotification_MoisDown { get; set; }
            public  DateTime LastNotificationCreateTime { get; set; }
            public string DeviceMacID { get; set; }
            public string DeviceName { get; set; }
            public string NotificationContent { get; set; }
            public float Temperature { get; set; }
            public float Moisture { get; set; }
            public string LastActivity { get; set; }
            public int LastActivityDiff { get; set; }
            public int DelayTempUp { get; set; }
            public int DelayTempDown { get; set; }
            public int DelayMoisUp { get; set; }
            public int DelayMoisDown { get; set; }

        }
        public class PhoneBring
        {
            public string phoneNumbers { get; set; }
        }

        public class MailBring
        {
            public string Mails { get; set; }
        }
        public static List<TEST> MailService()
        {
            List<TEST> obj = new List<TEST>();
            string connectionString = "data source=SQL5;Database=ServerViewer;User ID=semih; Password=semih;";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM ViolationDevices ORDER BY DeviceMacID";


                    SqlCommand command = new SqlCommand(sql, connection);

                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            command.CommandType = CommandType.Text;
                            TEST _obj = new TEST();
                            _obj.MaxTemp = int.Parse(dataReader["MaxTemp"].ToString());
                            _obj.MinTemp = int.Parse(dataReader["MinTemp"].ToString());
                            _obj.MaxMois = int.Parse(dataReader["MaxMois"].ToString());
                            _obj.MinMois = int.Parse(dataReader["MinMois"].ToString());
                            _obj.CustomerID = Convert.ToInt32(dataReader["CustomerId"]);
                            _obj.email = Convert.ToString(dataReader["EMail"]);
                            _obj.Phone = Convert.ToString(dataReader["Phone"]);
                            _obj.EmailNotification_TempUp = Convert.ToBoolean(dataReader["EmailNotification_TempUp"]);
                            _obj.EmailNotification_TempDown = Convert.ToBoolean(dataReader["EmailNotification_TempDown"]);
                            _obj.EmailNotification_MoisUp = Convert.ToBoolean(dataReader["EmailNotification_MoisUp"]);
                            _obj.EmailNotification_MoisDown = Convert.ToBoolean(dataReader["EmailNotification_MoisDown"]);
                            _obj.SmsNotification_TempUp = Convert.ToBoolean(dataReader["SmsNotification_TempUp"]);
                            _obj.SmsNotification_TempDown = Convert.ToBoolean(dataReader["SmsNotification_TempDown"]);
                            _obj.SmsNotification_MoisUp = Convert.ToBoolean(dataReader["SmsNotification_MoisUp"]);
                            _obj.SmsNotification_MoisDown = Convert.ToBoolean(dataReader["SmsNotification_MoisDown"]);
                            _obj.IvrNotification_TempUp = Convert.ToBoolean(dataReader["IvrNotification_TempUp"]);
                            _obj.IvrNotification_TempDown = Convert.ToBoolean(dataReader["IvrNotification_TempDown"]);
                            _obj.IvrNotification_MoisUp = Convert.ToBoolean(dataReader["IvrNotification_MoisUp"]);
                            _obj.IvrNotification_MoisDown = Convert.ToBoolean(dataReader["IvrNotification_MoisDown"]);
                            _obj.CallNotification_TempUp = Convert.ToBoolean(dataReader["CallNotification_TempUp"]);
                            _obj.CallNotification_TempDown = Convert.ToBoolean(dataReader["CallNotification_TempDown"]);
                            _obj.CallNotification_MoisUp = Convert.ToBoolean(dataReader["CallNotification_MoisUp"]);
                            _obj.CallNotification_MoisDown = Convert.ToBoolean(dataReader["CallNotification_MoisDown"]);
                            if (!string.IsNullOrEmpty(dataReader["LastNotificationCreateTime"].ToString())) {
                                _obj.LastNotificationCreateTime = Convert.ToDateTime(dataReader["LastNotificationCreateTime"]);
                            }
                            _obj.DeviceMacID = Convert.ToString(dataReader["DeviceMacID"]);
                            _obj.DeviceName = Convert.ToString(dataReader["DeviceName"]);
                            _obj.Temperature = float.Parse((dataReader["Temp"]).ToString());
                            _obj.Moisture = float.Parse((dataReader["Mois"]).ToString());
                            _obj.LastActivity = Convert.ToString(dataReader["LastActivityTime"]);
                            _obj.LastActivityDiff = Convert.ToInt32(dataReader["LastActivityDifference"]);
                            _obj.DelayTempUp = Convert.ToInt32(dataReader["DelayTempUp"]);
                            _obj.DelayTempDown = Convert.ToInt32(dataReader["DelayTempDown"]);
                            _obj.DelayMoisUp = Convert.ToInt32(dataReader["DelayMoisUp"]);
                            _obj.DelayMoisDown = Convert.ToInt32(dataReader["DelayMoisDown"]);
                            obj.Add(_obj);
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {

            }
            return obj;
        }

        public static List<PhoneBring> GetNumber(string macid)
        {
            List<PhoneBring> each = new List<PhoneBring>();
            string connectionString = "data source=SQL5;Database=ServerViewer;User ID=semih; Password=semih;";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT [CustomerID],[Phones] FROM [ServerViewer].[dbo].[PhoneList] WHERE DeviceMacID=@DeviceMacID";
                    SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@DeviceMacID", macid);
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            command.CommandType = CommandType.Text;
                            PhoneBring _obj = new PhoneBring();
                            _obj.phoneNumbers = Convert.ToString(dataReader["Phones"]);
                            each.Add(_obj);
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {

            }

            return each;
        }

        public static List<MailBring> GetMail(string macid)
        {
            List<MailBring> each = new List<MailBring>();
            string connectionString = "data source=SQL5;Database=ServerViewer;User ID=semih; Password=semih;";
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT [CustomerID],[EMail] FROM [ServerViewer].[dbo].[EmailList] WHERE DeviceMacID=@DeviceMacID";
                    SqlCommand command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@DeviceMacID", macid);
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            command.CommandType = CommandType.Text;
                            MailBring _elm = new MailBring();
                            _elm.Mails = Convert.ToString(dataReader["EMail"]);
                            each.Add(_elm);
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {

            }

            return each;
        }

        //sms service

        class Mesaj
        {
            public string msg { get; set; }
            public string dest { get; set; }

            public Mesaj() { }

            public Mesaj(string msg, string dest)
            {
                this.msg = msg;
                this.dest = dest;
            }
        }
        class SmsIstegi
        {
            public string username { get; set; }
            public string password { get; set; }
            public string source_addr { get; set; }
            public Mesaj[] messages { get; set; }
        }

        private void IstegiGonder(SmsIstegi istek)
        {
            string payload = JsonConvert.SerializeObject(istek);

            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            wc.Headers["Content-Type"] = "application/json";

            try
            {
                string campaign_id = wc.UploadString("http://sms.verimor.com.tr/v2/send.json", payload);
                //Response.Write("Mesaj gönderildi, kampanya id: " + campaign_id);
            }
            catch (WebException ex) // 400 hatalarında response body'de hatanın ne olduğunu yakalıyoruz
            {
                if (ex.Status == WebExceptionStatus.ProtocolError) // 400 hataları
                {
                    var responseBody = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    //Response.Write("Mesaj gönderilemedi, dönen hata: " + responseBody);
                }
                else // diğer hatalar
                {
                    //Response.Write("Mesaj gönderilemedi, dönen hata: " + ex.Status);
                    throw;
                }
            }
        }

        public void SendSms(string macid, float temp, float mois,string devicename, string body )
        {
            List<BackServiceManager.PhoneBring> listview = BackServiceManager.GetNumber(macid);
            foreach (BackServiceManager.PhoneBring item in listview)
            {
                var smsIstegi = new SmsIstegi();
                smsIstegi.username = "908502551103";
                smsIstegi.password = "xQBoYRJu";
                smsIstegi.source_addr = "Vodatech";
                smsIstegi.messages = new Mesaj[] { new Mesaj(body.ToString(), item.phoneNumbers.ToString()) };
                IstegiGonder(smsIstegi);
            }

        }



    }
}
