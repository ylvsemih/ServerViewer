using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using temperaturemois.Manager;
using temperaturemois.Models;
using temperaturemois.Controllers;
using AutoMapper.Configuration;
using Microsoft.Extensions.Configuration;
using System.Web.Http;
using TempMoisFinal.Controllers;


namespace temperaturemois.BackgroundService
{
    public class MailSendService : BackgroundService
    {
        public MailSendService()
        {

        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Mail.MailSender();
                TimeSpan NextDate = new TimeSpan();
                var value = DateTime.Now.AddSeconds(900);
                NextDate = value.Subtract(DateTime.Now);
                await Task.Delay(NextDate, stoppingToken);
            }
        }
    }
}
