using Microsoft.AspNetCore.Http;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;


namespace temperaturemois.Manager
{
    public class KullaniciBilgileri
    {
        public string Email { get; set; }

        public int KullaniciId { get; set; }
    }
    public class SessionHelper
    {
        private readonly IHttpContextAccessor HttpContextAccessor;

        public SessionHelper(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        public void SetEmail(string emailValue)
        {
            //Sessiona Kullanıcın Mail adresini atalım
            HttpContextAccessor.HttpContext.Session.SetString("KullaniciEmail", emailValue);
        }

        public  string GetEmail()
        {
            //Sessiondan Kullanıcın Mail adresini okuyalım
            return HttpContextAccessor.HttpContext.Session.GetString("KullaniciEmail");
        }

        

    }
}
