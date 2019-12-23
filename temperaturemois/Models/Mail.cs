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
                    
                    var body = new StringBuilder();
                    

                    body.AppendLine("Dikkat! <br>"+item.DeviceName+":"+item.DeviceMacID+" Sensörünüzde Sıcaklık Yüksekliği Tespit Edildi! <br> Mevcut Sıcaklık: " + item.Temperature + " <br> Nem: " + item.Moisture+"%");

                    //till
                    Generate_Notification gnr = new Generate_Notification();
                    Generate_Notification.Notification_Log(Convert.ToInt32(item.CustomerID), body.ToString(), Convert.ToString(item.DeviceMacID), Convert.ToString(item.DeviceName));
                    BackServiceManager bc = new BackServiceManager();
                    bc.SendSms(item.DeviceMacID, item.Temperature, item.Moisture,item.DeviceName);

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
            catch (Exception ex)
            {
                throw;
            }

        }

    }
}
