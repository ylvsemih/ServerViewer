using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;
using ControlzEx.Standard;

namespace temperaturemois
{
    
    public class SendMail
    {
        public void Send()
        {
            //gmail Configurations

            SmtpClient sc = new SmtpClient();
            sc.Port = 587;
            sc.Host = "smtp.gmail.com";
            sc.EnableSsl = true;

            sc.Credentials = new NetworkCredential("ylvspy@hotmail.com", "s15298578");

            MailMessage mail = new MailMessage();

            mail.From = new MailAddress("ylvspy@hotmail.com", "test");

            mail.To.Add("semihyoltan@gmail.com");
            mail.To.Add("alici2@mail.com");

            mail.CC.Add("alici3@mail.com");
            mail.CC.Add("alici4@mail.com");

            mail.Subject = "E-Posta Konusu"; mail.IsBodyHtml = true; mail.Body = "E-Posta İçeriği";

            mail.Attachments.Add(new Attachment(@"C:\Rapor.xlsx"));
            mail.Attachments.Add(new Attachment(@"C:\Sonuc.pptx"));

            sc.Send(mail);
        }
    }
    
}
