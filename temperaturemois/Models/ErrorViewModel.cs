using System;

namespace temperaturemois.Models
{
    public class ErrorViewModel
    {
        public int ID { get; set; }


        public int DeviceID { get; set; }



        public float Temp { get; set; }


        public float Mois { get; set; }

        public float SNI { get; set; }

        public string CreateTime { get; set; }
        public bool Result { get; set; }
        public string Msg { get; set; }
        public string surname { get; set; }

        
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public int MaxTemp { get; set; }
        public int MinTemp { get; set; }
        public int MaxMois { get; set; }
        public int MinMois { get; set; }
        
        public int CustomerId { get; set; }
        public string PhoneNumber { get; set; }
        public string Company { get; set; }
        public bool EmailCheck { get; set; }
        public string DeviceCode { get; set; }
        public string DeviceName { get; set; }
        public string[] AllPhones { get; set; }

        public string CihazAD { get; set; }

        public string MacID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string NotificationMessage { get; set; }

        

        //email bildirimleri
        public bool Email_TempUp { get; set; }
        public bool Email_TempDown { get; set; }
        public bool Email_MoisUp { get; set; }
        public bool Email_MoisDown { get; set; }
        //msj bildirimleri
        public bool Msg_TempUp { get; set; }
        public bool Msg_TempDown { get; set; }
        public bool Msg_MoisUp { get; set; }
        public bool Msg_MoisDown { get; set; }
        //IVR Bildirimleri
        public bool IVR_TempUp { get; set; }
        public bool IVR_TempDown { get; set;}
        public bool IVR_MoisUp { get; set; }
        public bool IVR_MoisDown { get; set; }
        //Call Center
        public bool Call_TempUp { get; set; }
        public bool Call_TempDown { get; set; }
        public bool Call_MoisUp { get; set; }
        public bool Call_MoisDown { get; set; }

    }
}