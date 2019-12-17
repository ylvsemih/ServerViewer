using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using temperaturemois.Models;
using temperaturemois.Controllers;
using TempMoisFinal.Controllers;

namespace temperaturemois.Manager
{
    public class MailManager 
    {

        public void Mailsend()
        {

            Mail.MailSender();
            
        }
    }
}
