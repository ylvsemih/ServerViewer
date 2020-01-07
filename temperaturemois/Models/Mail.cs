using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using TempMoisFinal.Controllers;
using temperaturemois.Manager;
using System.Text;

namespace temperaturemois.Models
{
    public class Mail
    {
        public static void MailSender()//string body
        {

            try
            {
                List<BackServiceManager.TEST> objList = BackServiceManager.MailService();
                //new code with dynamic value
                foreach (BackServiceManager.TEST item in objList)
                {
                    
                    //mail body
                    bool email = false;
                    bool sms = false;
                    //bool IVR = false;
                    //bool call = false;
                    bool delaycheck = false;
                    string type = "";
                    var body = new StringBuilder();
                    var smsbody = new StringBuilder();
                    DateTime dtm = DateTime.Now;
                    DateTime date = item.LastNotificationCreateTime;
                    double difference = (dtm-date).Minutes;
                    
                    if (item.Temperature > item.MaxTemp)
                    {
                        
                        if (difference > item.DelayTempUp)
                        {
                            delaycheck = true;
                        }

                        if (item.EmailNotification_TempUp == true)
                        {
                            type = "TempUp";
                            body.AppendLine("Dikkat! <br>" + item.DeviceName + ":" + item.DeviceMacID + " Sensörünüzde Sıcaklık Yüksekliği Tespit Edildi! <br> Mevcut Sıcaklık: " + item.Temperature + " <br> Nem: " + item.Moisture + "%");
                            delaycheck = true;
                            email = true;
                        }
                        if (item.SmsNotification_TempUp == true)
                        {
                            smsbody.AppendLine("Dikkat! \n" + item.DeviceName + ":" + item.DeviceMacID + " Sensörünüzde Sıcaklık Yüksekliği Tespit Edildi! \n Mevcut Sıcaklık: " + item.Temperature + " \n Nem: " + item.Moisture + "%");
                            delaycheck = true;
                            sms = true;
                        }

                    }
                    else if (item.Temperature < item.MinTemp)
                    {
                        if (difference > item.DelayTempDown)
                        {
                            delaycheck = true;
                        }
                        if (item.EmailNotification_TempDown == true)
                        {
                            type = "TempDown";
                            body.AppendLine("Dikkat! <br>" + item.DeviceName + ":" + item.DeviceMacID + " Sensörünüzde Sıcaklık Düşüklüğü Tespit Edildi! <br> Mevcut Sıcaklık: " + item.Temperature + " <br> Nem: " + item.Moisture + "%");
                            delaycheck = true;
                            email = true;
                        }
                        if (item.SmsNotification_TempDown == true)
                        {
                            smsbody.AppendLine("Dikkat! \n" + item.DeviceName + ":" + item.DeviceMacID + " Sensörünüzde Sıcaklık Düşüklüğü Tespit Edildi! \n Mevcut Sıcaklık: " + item.Temperature + " \n Nem: " + item.Moisture + "%");
                            delaycheck = true;
                            sms = true;
                        }

                    }
                    else if (item.Moisture > item.MaxMois)
                    {
                        if (difference > item.DelayMoisUp)
                        {
                            delaycheck = true;
                        }

                        if (item.EmailNotification_MoisUp == true)
                        {
                            type = "MoisUp";
                            body.AppendLine("Dikkat! <br>" + item.DeviceName + ":" + item.DeviceMacID + " Sensörünüzde Nem Yüksekliği Tespit Edildi! <br> Mevcut Sıcaklık: " + item.Temperature + " <br> Nem: " + item.Moisture + "%");
                            delaycheck = true;
                            email = true;
                        }
                        if (item.SmsNotification_MoisUp == true)
                        {
                            smsbody.AppendLine("Dikkat! \n" + item.DeviceName + ":" + item.DeviceMacID + " Sensörünüzde Nem Yüksekliği Tespit Edildi! \n Mevcut Sıcaklık: " + item.Temperature + " \n Nem: " + item.Moisture + "%");
                            sms = true;
                            delaycheck = true;
                        }

                    }
                    else if (item.Moisture < item.MinMois)
                    {
                        if (difference > item.DelayMoisDown)
                        {
                            delaycheck = true;
                        }
                        if (item.EmailNotification_MoisDown == true)
                        {
                            type = "MoisDown";
                            body.AppendLine("Dikkat! <br>" + item.DeviceName + ":" + item.DeviceMacID + " Sensörünüzde Nem Düşüklüğü Tespit Edildi! <br> Mevcut Sıcaklık: " + item.Temperature + " <br> Nem: " + item.Moisture + "%");
                            delaycheck = true;
                            email = true;
                        }
                        if (item.SmsNotification_MoisDown == true)
                        {
                            smsbody.AppendLine("Dikkat! \n" + item.DeviceName + ":" + item.DeviceMacID + " Sensörünüzde Nem Düşüklüğü Tespit Edildi! \n Mevcut Sıcaklık: " + item.Temperature + " \n Nem: " + item.Moisture + "%");
                            sms = true;
                            delaycheck = true;
                        }



                    }
                    
                    //send notification if device is offline last 15 min
                    //else if (difference >= 15)
                    //{
                    //    type = "Offline";
                    //    body.AppendLine("Dikkat! <br>" + item.DeviceName + ":" + item.DeviceMacID + " Sensörünüz " + item.LastActivityDiff + " Dakikadır Offline Durumda! <br> Son Alınan Sıcaklık: " + item.Temperature + " <br> Son Alınan Nem: " + item.Moisture + "%");
                    //    setrequest(body.ToString(), type);
                    //    smsbody.AppendLine("Dikkat! \n" + item.DeviceName + ":" + item.DeviceMacID + "  Sensörünüz " + item.LastActivityDiff + " Dakikadır Offline Durumda! \n Son Alınan Sıcaklık: " + item.Temperature + " \n Son Alınan Nem: " + item.Moisture + "%");
                    //    setrequestsms(smsbody.ToString());
                    //}
                    //till

                    if (delaycheck == true)
                    {

                        if (email == true)
                        {
                            setrequest(body.ToString(), type);
                        }
                        if (sms == true)
                        {
                            setrequestsms(smsbody.ToString());
                        }
                        delaycheck = false; // işlemi bir kere yap sonra kontrole geri dön 
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        public static void setrequest(string body, string type)
        {
            List<BackServiceManager.TEST> objList = BackServiceManager.MailService();
            //new code with dynamic value
            foreach (BackServiceManager.TEST item in objList)
            {
                Generate_Notification gnr = new Generate_Notification();
                Generate_Notification.Notification_Log(Convert.ToInt32(item.CustomerID), body.ToString(), Convert.ToString(item.DeviceMacID), Convert.ToString(item.DeviceName), type);

                var fromAddress = new MailAddress("noreply@vodatech.com.tr");
                List<BackServiceManager.MailBring> listview = BackServiceManager.GetMail(item.DeviceMacID.ToString());
                foreach (BackServiceManager.MailBring list in listview)
                {
                    var toAddress = new MailAddress(list.Mails); //bilgi.islem@vodatech.com.tr
                    const string subject = "Server Viewer Mail Bildirim";
                    using (var smtp = new SmtpClient
                    {
                        Host = "mail.vodatech.com.tr",
                        Port = 587,
                        EnableSsl = false,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(fromAddress.Address, "1qaz2wsxA")

                    })
                    {
                        using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body.ToString(), IsBodyHtml = true })
                        {
                            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                            smtp.Send(message);
                        }
                    }

                }
            }
        }
        public static void setrequestsms(string smsbody)
        {
            List<BackServiceManager.TEST> objList = BackServiceManager.MailService();
            //new code with dynamic value
            foreach (BackServiceManager.TEST item in objList)
            {
                BackServiceManager bc = new BackServiceManager();
                bc.SendSms(item.DeviceMacID, item.Temperature, item.Moisture, item.DeviceName, smsbody.ToString());
            }
        }
    }
}
