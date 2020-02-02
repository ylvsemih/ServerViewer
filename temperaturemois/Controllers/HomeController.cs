using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using temperaturemois.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using System.Globalization;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using System.Text;
using temperaturemois.Manager;
using System.Net.Mail;
using System.Net;
using TempMoisFinal.Controllers;
using Microsoft.AspNetCore.Http;

namespace TempMoisFinal.Controllers
{

    public class Phone
    {
        public string Phones { get; set; }
    }


    public class HomeController : Controller
    {

        public string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }


        public IConfiguration Configuration { get; }


        public HomeController(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        [ClaimRequirement]
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet, Route("Api/Charts")]
        public JsonResult Charts(string Macid)
        {
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();

            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                //string sql = "SELECT TOP 1 [ID], [DeviceID], [Temp], [Mois], [CreateTime],[MacID] FROM[ServerViewer].[dbo].[ServerStatus] WHERE MacID='" + Macid + "'ORDER BY CreateTime DESC";
              
                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 [ID], [DeviceID], [Value], [Key], [CreateTime],[MacID] FROM[ServerViewer].[dbo].[ServerStatusTemp] WHERE MacID=@DeviceMacID AND Value=@Value+'/t' ORDER BY CreateTime DESC", connection))
                {
                    
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@DeviceMacID", Macid);
                    cmd.Parameters.AddWithValue("@Value", Macid);
                    SqlDataReader dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        ErrorViewModel data = new ErrorViewModel();
                        data.Temp = float.Parse(dataReader["Key"].ToString());
                        data.CreateTime = Convert.ToDateTime(dataReader["CreateTime"]).ToShortTimeString();
                        data.ID = Convert.ToInt32(dataReader["ID"]);
                        data.DeviceID = Convert.ToInt32(dataReader["DeviceID"]);
                        dataList.Add(data);

                    }
                    connection.Close();
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
                
                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 [ID], [DeviceID], [Value], [Key], [CreateTime],[MacID] FROM[ServerViewer].[dbo].[ServerStatusTemp] WHERE MacID=@DeviceMacID AND Value=@Value+'/h' ORDER BY CreateTime DESC", connection))
                {
                    
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@DeviceMacID", Macid);
                    cmd.Parameters.AddWithValue("@Value", Macid);
                    SqlDataReader dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        ErrorViewModel data = new ErrorViewModel();
                        data.Mois = float.Parse(dataReader["Key"].ToString());
                        data.CreateTime = Convert.ToDateTime(dataReader["CreateTime"]).ToShortTimeString();
                        data.ID = Convert.ToInt32(dataReader["ID"]);
                        data.DeviceID = Convert.ToInt32(dataReader["DeviceID"]);
                        dataList.Add(data);

                    }
                    connection.Close();
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
                
                connection.Close();
            }
            return Json(dataList);
        }


        public class PostModelTest
        {
            public string startDate { get; set; }
            public string endDate { get; set; }
        }

        public class Marker
        {
            public decimal position { get; set; }
            public int markerPosition { get; set; }
        }


        [HttpPost, Route("Api/Charts_Line")]
        public JsonResult Charts_Line(string startDate, string endDate, string Macid,string cases)
        {
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();
            string sql = null;

            try
            {
                string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    DateTime d1 = DateTime.ParseExact(startDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                    DateTime d2 = DateTime.ParseExact(endDate, "yyyyMMdd", CultureInfo.InvariantCulture);
                    if(cases == "temp")
                    {
                        if ((d1 - d2).TotalDays == 0)
                        {
                            d2 = d2.AddDays(1);
                            sql = "SELECT FORMAT ([CreateTime], 'yyyy-MM-dd HH:mm:ss') [CreateTime], ROUND(AVG([Key]),2)[Key] FROM [ServerViewer].[dbo].[ServerStatusTemp]  WHERE CreateTime between '" + d1.ToString("yyyy-MM-dd hh:mm:ss") + "' and '" + d2.ToString("yyyy-MM-dd hh:mm:ss") + "' AND MacID='" + Macid + "' AND Value='" + Macid + "/t' GROUP BY FORMAT ([CreateTime], 'yyyy-MM-dd HH:mm:ss') ORDER by min(CreateTime) asc";
                        }
                        if ((d1 - d2).TotalDays == -1)
                        {
                            sql = "SELECT FORMAT ([CreateTime], 'yyyy-MM-dd HH:mm:ss') [CreateTime],ROUND(AVG([Key]),2) [Key] FROM[ServerViewer].[dbo].[ServerStatusTemp]   WHERE CreateTime between '" + d1.ToString("yyyy-MM-dd hh:mm:ss") + "' and '" + d2.ToString("yyyy-MM-dd hh:mm:ss") + "' AND MacID='" + Macid + "' AND Value='" + Macid + "/t' GROUP BY FORMAT ([CreateTime], 'yyyy-MM-dd HH:mm:ss') ORDER by min(CreateTime) asc";
                        }
                        else
                        {
                            sql = "SELECT FORMAT ([CreateTime], 'yyyy-MM-dd') [CreateTime],ROUND(AVG([Key]),2) [Key] FROM[ServerViewer].[dbo].[ServerStatusTemp]  WHERE CreateTime between '" + d1.ToString("yyyy-MM-dd") + "' and '" + d2.ToString("yyyy-MM-dd") + "' AND MacID='" + Macid + "' AND Value='" + Macid + "/t' GROUP BY FORMAT([CreateTime], 'yyyy-MM-dd') ORDER BY min(CreateTime) asc";
                        }
                    }
                    if(cases == "mois")
                    {
                        if ((d1 - d2).TotalDays == 0)
                        {
                            d2 = d2.AddDays(1);
                            sql = "SELECT FORMAT ([CreateTime], 'yyyy-MM-dd HH:mm:ss') [CreateTime], ROUND(AVG([Key]),2)[Key] FROM [ServerViewer].[dbo].[ServerStatusTemp]  WHERE CreateTime between '" + d1.ToString("yyyy-MM-dd hh:mm:ss") + "' and '" + d2.ToString("yyyy-MM-dd hh:mm:ss") + "' AND MacID='" + Macid + "' AND Value='" + Macid + "/h' GROUP BY FORMAT ([CreateTime], 'yyyy-MM-dd HH:mm:ss') ORDER by min(CreateTime) asc";
                        }
                        else if ((d1 - d2).TotalDays == -1)
                        {
                            sql = "SELECT FORMAT ([CreateTime], 'yyyy-MM-dd HH:mm:ss') [CreateTime],ROUND(AVG([Key]),2) [Key] FROM[ServerViewer].[dbo].[ServerStatusTemp]   WHERE CreateTime between '" + d1.ToString("yyyy-MM-dd hh:mm:ss") + "' and '" + d2.ToString("yyyy-MM-dd hh:mm:ss") + "' AND MacID='" + Macid + "' AND Value='" + Macid + "/h' GROUP BY FORMAT ([CreateTime], 'yyyy-MM-dd HH:mm:ss') ORDER by min(CreateTime) asc";
                        }
                        else
                        {
                            sql = "SELECT FORMAT ([CreateTime], 'yyyy-MM-dd') [CreateTime],ROUND(AVG([Key]),2) [Key] FROM[ServerViewer].[dbo].[ServerStatusTemp]  WHERE CreateTime between '" + d1.ToString("yyyy-MM-dd") + "' and '" + d2.ToString("yyyy-MM-dd") + "' AND MacID='" + Macid + "' AND Value='" + Macid + "/h' GROUP BY FORMAT([CreateTime], 'yyyy-MM-dd') ORDER BY min(CreateTime) asc";
                        }

                    }

                    SqlCommand command = new SqlCommand(sql, connection);
                    
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            command.CommandType = CommandType.Text;
                            ErrorViewModel data = new ErrorViewModel();
                            if(cases == "temp")
                            {
                                data.Temp = float.Parse(dataReader["Key"].ToString());
                                data.CreateTime = Convert.ToString(dataReader["CreateTime"]);
                            }
                            else if (cases =="mois" )
                            {
                                data.Mois = float.Parse(dataReader["Key"].ToString());
                                data.CreateTime = Convert.ToString(dataReader["CreateTime"]);
                            }
                            data.Msg = sql;
                            dataList.Add(data);
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorViewModel data = new ErrorViewModel();
                data.Temp = 0;
                data.Mois = 0;
                data.CreateTime = DateTime.Now.ToString();
                data.Msg = sql;
                dataList.Add(data);
            }
            return Json(dataList);
        }

        [HttpPost, Route("Api/Arc_Value")]
        public JsonResult Arc_Value(int Position, string Type, string Macid)
        {
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();
            string Fsql;
            var MacAdress = HttpContext.Session.GetString("MacAdress");
            var email = HttpContext.Session.GetInt32("KullaniciEmail");
            switch (Type)
            {
                case "mintemp":

                    string sql_arc_tempMin = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET MinTemp=" + Position + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = sql_arc_tempMin;
                    break;
                case "maxtemp":
                    string sql_arc_tempMax = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET MaxTemp=" + Position + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";

                    Fsql = sql_arc_tempMax;
                    break;
                case "minmois":
                    string sql_arc_moisMin = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET MinMois=" + Position + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";

                    Fsql = sql_arc_moisMin;

                    break;
                case "maxmois":
                    string sql_arc_moisMax = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET MaxMois=" + Position + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";

                    Fsql = sql_arc_moisMax;
                    break;
                default:
                    string sql_arc_default = "SELECT [CustomerID],[MaxTemp],[MinTemp],[MaxMois],[MinMois] FROM [ServerViewer].[dbo].[DeviceInfo] WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";

                    Fsql = sql_arc_default;
                    break;

            }
            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand command = new SqlCommand(Fsql, connection);

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        command.CommandType = CommandType.Text;
                        ErrorViewModel data = new ErrorViewModel();
                        data.MaxTemp = Convert.ToInt32(dataReader["MaxTemp"]);
                        data.MinTemp = Convert.ToInt32(dataReader["MinTemp"]);
                        data.MinMois = Convert.ToInt32(dataReader["MinMois"]);
                        data.MaxMois = Convert.ToInt32(dataReader["MaxMois"]);
                        data.CustomerId = Convert.ToInt32(dataReader["CustomerID"]);
                        dataList.Add(data);

                    }
                }


                connection.Close();
            }
            return Json(dataList);
        }

        //Checkbox Notifies
        [HttpPost, Route("Api/Mails")]
        public JsonResult CheckNotifies(bool EmailNotification, string CheckType, string Macid, string CihazAd)
        {
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();
            string Fsql;
            var email = HttpContext.Session.GetInt32("KullaniciEmail");
            switch (CheckType)
            {
                case "email1":
                    string set_mail = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET EmailNotification_TempUp=" + Convert.ToInt32(EmailNotification) + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = set_mail;
                    break;
                case "mesaj1":
                    string set_message = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET SmsNotification_TempUp=" + Convert.ToInt32(EmailNotification) + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = set_message;
                    break;
                case "IVR1":
                    string set_IVR = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET IvrNotification_TempUp=" + Convert.ToInt32(EmailNotification) + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = set_IVR;
                    break;
                case "CAGRI1":
                    string set_CAGRI = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET CallNotification_TempUp=" + Convert.ToInt32(EmailNotification) + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = set_CAGRI;
                    break;
                //2nd vawe

                case "email2":
                    string set_mail2 = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET EmailNotification_TempDown=" + Convert.ToInt32(EmailNotification) + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = set_mail2;
                    break;
                case "mesaj2":
                    string set_message2 = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET SmsNotification_TempDown=" + Convert.ToInt32(EmailNotification) + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = set_message2;
                    break;
                case "IVR2":
                    string set_IVR2 = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET IvrNotification_TempDown=" + Convert.ToInt32(EmailNotification) + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = set_IVR2;
                    break;
                case "CAGRI2":
                    string set_CAGRI2 = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET CallNotification_TempDown=" + Convert.ToInt32(EmailNotification) + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = set_CAGRI2;
                    break;

                //3rd vawe
                case "email3":
                    string set_mail3 = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET EmailNotification_MoisUp=" + Convert.ToInt32(EmailNotification) + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = set_mail3;
                    break;
                case "mesaj3":
                    string set_message3 = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET SmsNotification_MoisUp=" + Convert.ToInt32(EmailNotification) + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = set_message3;
                    break;
                case "IVR3":
                    string set_IVR3 = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET IvrNotification_MoisUp=" + Convert.ToInt32(EmailNotification) + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = set_IVR3;
                    break;
                case "CAGRI3":
                    string set_CAGRI3 = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET CallNotification_MoisUp=" + Convert.ToInt32(EmailNotification) + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = set_CAGRI3;
                    break;

                //4th vawe
                case "email4":
                    string set_mail4 = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET EmailNotification_MoisDown=" + Convert.ToInt32(EmailNotification) + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = set_mail4;
                    break;
                case "mesaj4":
                    string set_message4 = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET SmsNotification_MoisDown=" + Convert.ToInt32(EmailNotification) + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = set_message4;
                    break;
                case "IVR4":
                    string set_IVR4 = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET IvrNotification_MoisDown=" + Convert.ToInt32(EmailNotification) + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = set_IVR4;
                    break;
                case "CAGRI4":
                    string set_CAGRI4 = "UPDATE [ServerViewer].[dbo].[DeviceInfo] SET CallNotification_MoisDown=" + Convert.ToInt32(EmailNotification) + " WHERE CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = set_CAGRI4;
                    break;

                default:
                    string set_DEFAULT = "SELECT [DeviceMacID],[CustomerID],[EmailNotification_TempUp],[SmsNotification_TempUp],[IvrNotification_TempUp],[CallNotification_TempUp],[EmailNotification_TempDown],[SmsNotification_TempDown],[IvrNotification_TempDown],[CallNotification_TempDown],[EmailNotification_MoisUp],[SmsNotification_MoisUp],[IvrNotification_MoisUp],[CallNotification_MoisUp],[EmailNotification_MoisDown],[SmsNotification_MoisDown],[IvrNotification_MoisDown],[CallNotification_MoisDown] FROM [ServerViewer].[dbo].[DeviceInfo] Where CustomerID='" + email + "' AND DeviceMacID='" + Macid + "'";
                    Fsql = set_DEFAULT;
                    break;

            }
            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(Fsql, connection);
                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        command.CommandType = CommandType.Text;
                        ErrorViewModel data = new ErrorViewModel();
                        data.MacID = Convert.ToString(dataReader["DeviceMacID"]);
                        data.CustomerId = Convert.ToInt32(dataReader["CustomerID"]);
                        //Mail
                        data.Email_TempUp = Convert.ToBoolean(dataReader["EmailNotification_TempUp"]);
                        data.Email_TempDown = Convert.ToBoolean(dataReader["EmailNotification_TempDown"]);
                        data.Email_MoisUp = Convert.ToBoolean(dataReader["EmailNotification_MoisUp"]);
                        data.Email_MoisDown = Convert.ToBoolean(dataReader["EmailNotification_MoisDown"]);
                        //mesaj
                        data.Msg_TempUp = Convert.ToBoolean(dataReader["SmsNotification_TempUp"]);
                        data.Msg_TempDown = Convert.ToBoolean(dataReader["SmsNotification_TempDown"]);
                        data.Msg_MoisUp = Convert.ToBoolean(dataReader["SmsNotification_MoisUp"]);
                        data.Msg_MoisDown = Convert.ToBoolean(dataReader["SmsNotification_MoisDown"]);
                        //IVR
                        data.IVR_TempUp = Convert.ToBoolean(dataReader["IvrNotification_TempUp"]);
                        data.IVR_TempDown = Convert.ToBoolean(dataReader["IvrNotification_TempDown"]);
                        data.IVR_MoisUp = Convert.ToBoolean(dataReader["IvrNotification_MoisUp"]);
                        data.IVR_MoisDown = Convert.ToBoolean(dataReader["IvrNotification_MoisDown"]);
                        //Call
                        data.Call_TempUp = Convert.ToBoolean(dataReader["CallNotification_TempUp"]);
                        data.Call_TempDown = Convert.ToBoolean(dataReader["CallNotification_TempDown"]);
                        data.Call_MoisUp = Convert.ToBoolean(dataReader["CallNotification_MoisUp"]);
                        data.Call_MoisDown = Convert.ToBoolean(dataReader["CallNotification_MoisDown"]);



                        dataList.Add(data);
                    }
                }

                connection.Close();
            }
            return Json(dataList);
        }
        [ClaimRequirement]
        public IActionResult Create()
        {

            return View();
        }
        [ClaimRequirement]
        public IActionResult OptionsDashboad()
        {

            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];
            var email = HttpContext.Session.GetInt32("KullaniciEmail");
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();
            string sql2 = "SELECT [CustomerID],[CreateTime],[MaxTemp],[MinTemp],[MaxMois],[MinMois] FROM [ServerViewer].[dbo].[DeviceInfo] WHERE CustomerID='" + email + "'";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command2 = new SqlCommand(sql2, connection);

                using (SqlDataReader dataReader = command2.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        command2.CommandType = CommandType.Text;
                        ErrorViewModel data = new ErrorViewModel();
                        data.MaxTemp = Convert.ToInt32(dataReader["MaxTemp"].ToString());
                        data.MinTemp = Convert.ToInt32(dataReader["MinTemp"].ToString());
                        data.MaxMois = Convert.ToInt32(dataReader["MaxMois"].ToString());
                        data.MinMois = Convert.ToInt32(dataReader["MinMois"].ToString());
                        ViewData["MaxTempArc"] = data.MaxTemp;
                        ViewData["MinTempArc"] = data.MinTemp;
                        ViewData["MaxMoisArc"] = data.MaxMois;
                        ViewData["MinMoisArc"] = data.MinMois;
                        dataList.Add(data);

                    }
                }
                connection.Close();
            }
            return View();
        }
        [ClaimRequirement]
        public IActionResult OptionsDashboard_Notification()
        {

            return View();
        }
        [ClaimRequirement]
        public IActionResult OptionsDashboard_Notification2()
        {

            return View();
        }
        [ClaimRequirement]
        public IActionResult Device_List()
        {

            return View();
        }

        public IActionResult Forget_pass()
        {

            return View();
        }
        [ClaimRequirement]
        public IActionResult Device_Logs()
        {

            return View();
        }
        [ClaimRequirement]
        public IActionResult New_Device()
        {
            return View();
        }
        [ClaimRequirement]
        public IActionResult Profile()
        {

            return View();
        }
        public IActionResult MacID_Register_ByADMIN()
        {
            return View();
        }


        [HttpGet, Route("Api/LogServerStatus")]
        public ErrorViewModel Create([FromUri]string temp, [FromUri]string mois, [FromUri]string MacId)
        {

            ErrorViewModel data = new ErrorViewModel();

            try
            {

                data.Temp = float.Parse(temp);
                data.Mois = float.Parse(mois);
                using (SqlConnection cn = new SqlConnection("data source=SQL5;Database=ServerViewer;uid=semih;pwd=semih;"))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.ServerStatus(DeviceID,Temp,Mois,MacID,CreateTime) VALUES(@DeviceID,@Temp,@Mois,@MacID,GETDATE())", cn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@DeviceID", 1);
                        cmd.Parameters.AddWithValue("@Temp", Math.Round(data.Temp, 2));
                        cmd.Parameters.AddWithValue("@Mois", Math.Round(data.Mois, 2));
                        cmd.Parameters.AddWithValue("@MacID", MacId);
                        cn.Open();
                        cmd.ExecuteNonQuery();
                        cn.Close();

                    }
                }
                data.Msg = "Sunucu log kaydı başarıyla kaydedilmiştir.";
                data.Result = true;
            }
            catch (Exception ex)
            {
                data.Msg = ex.Message;
                data.Result = false;
            }

            return data;
        }

        [HttpPost, Route("Api/Phones")]
        public string TakeNumbers(string[] phonelist, string Macid)
        {
            var email = HttpContext.Session.GetInt32("KullaniciEmail");
            string connectionString = "data source=SQL5;Database=ServerViewer;User ID=semih; Password=semih;";

            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("Delete From PhoneList WHERE CustomerID=@CustomerID AND DeviceMacID=@DeviceMacID", cn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@CustomerID", email);
                    cmd.Parameters.AddWithValue("@DeviceMacID", Macid);
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    cn.Close();

                }
            }
            foreach (string item in phonelist)
            {
                using (SqlConnection cn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO PhoneList(CustomerID,Phones,DeviceMacID) Values(@CustomerID,@Phone,@DeviceMacID)", cn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@CustomerID", email);
                        cmd.Parameters.AddWithValue("@Phone", item);
                        cmd.Parameters.AddWithValue("@DeviceMacID", Macid);
                        if (item != null)
                        {
                            cn.Open();
                            cmd.ExecuteNonQuery();
                        }
                        if (item == null)
                        {
                            cn.Close();
                        }



                        cn.Close();

                    }
                }
            }

            return null;

        }

        [HttpPost, Route("Api/SetText")]
        public JsonResult Set_Text(string Macid, string CihazAd)
        {
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();

            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];
            var email = HttpContext.Session.GetInt32("KullaniciEmail");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT[CustomerID],[Phones],[DeviceMacID] FROM[ServerViewer].[dbo].[PhoneList] WHERE CustomerID=@CustomerID AND DeviceMacID=@DeviceMacID",connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@CustomerID", email);
                    cmd.Parameters.AddWithValue("@DeviceMacID", Macid);
                    SqlDataReader dataReader = cmd.ExecuteReader();

                    while (dataReader.Read())
                    {
                        ErrorViewModel data = new ErrorViewModel();
                        data.MacID = Convert.ToString(dataReader["DeviceMacID"]);
                        data.PhoneNumber = Convert.ToString(dataReader["Phones"]);
                        dataList.Add(data);

                    }
                    connection.Close();
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    
                }

                connection.Close();
            }
            return Json(dataList);
        }

        [HttpPost, Route("Api/DeviceConfig")]
        public JsonResult Device_Config(string Macid, string CihazAd)
        {
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();

            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var email = HttpContext.Session.GetInt32("KullaniciEmail");
                connection.Open();
                string sql = "SELECT [ID],[Code],[DeviceMacID],[CustomerID],[DeviceName], FORMAT (getdate(), 'yyyyMMdd ') [CreateTime] FROM [ServerViewer].[dbo].[DeviceInfo] WHERE CustomerID='" + email + "'";
                SqlCommand command = new SqlCommand(sql, connection);

                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        command.CommandType = CommandType.Text;
                        ErrorViewModel data = new ErrorViewModel();
                        data.Name = Convert.ToString(dataReader["DeviceName"]);
                        data.CustomerId = Convert.ToInt32(dataReader["CustomerID"]);
                        data.ID = Convert.ToInt32(dataReader["ID"]);
                        data.DeviceCode = Convert.ToString(dataReader["Code"]);
                        data.MacID = Convert.ToString(dataReader["DeviceMacID"]);
                        data.CreateTime = Convert.ToString(dataReader["CreateTime"]);
                        dataList.Add(data);

                    }
                }

                connection.Close();
            }
            return Json(dataList);
        }

        [HttpPost, Route("Api/Devices")]
        public JsonResult Devices(string Macid, string CihazAd)
        {

            List<ErrorViewModel> dataList = new List<ErrorViewModel>();
            var email = HttpContext.Session.GetInt32("KullaniciEmail");
            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("UPDATE [ServerViewer].[dbo].[DeviceInfo] SET DeviceName=@DeviceName WHERE CustomerID=@CustomerID AND DeviceMacID=@DeviceMacID",connection))
                {
                    
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@DeviceName", CihazAd);
                    cmd.Parameters.AddWithValue("@CustomerID", email);
                    cmd.Parameters.AddWithValue("@DeviceMacID", Macid);
                    SqlDataReader dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        ErrorViewModel data = new ErrorViewModel();
                        data.Name = Convert.ToString(dataReader["Name"]);
                        data.CustomerId = Convert.ToInt32(dataReader["CustomerID"]);
                        data.ID = Convert.ToInt32(dataReader["ID"]);
                        data.DeviceCode = Convert.ToString(dataReader["Code"]);
                        data.MacID = Convert.ToString(dataReader["MacID"]);
                        dataList.Add(data);

                    }
                    connection.Close();
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }


                connection.Close();
            }
            return Json(dataList);
        }
        //Create Account
        [HttpPost, Route("Api/UserData_Create")]
        public JsonResult New_User(string username, string password, string phoneNumber, string email, string Name, string macId, string devicename)
        {

            ErrorViewModel data = new ErrorViewModel();
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();

            try
            {
                using (SqlConnection cn = new SqlConnection("data source=SQL5;Database=ServerViewer;uid=semih;pwd=semih;"))
                {
                    using (SqlCommand cmd = new SqlCommand("spCustomerRegister", cn))
                    {
                        //(@CustomerID,@Username,@Phone,@EMail,@Password,@DeviceName,@DeviceMacID)
                        cmd.CommandType = CommandType.StoredProcedure;
                        //cmd.Parameters.AddWithValue("@CustomerID", email);

                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@Phone", phoneNumber);
                        cmd.Parameters.AddWithValue("@EMail", email);
                        cmd.Parameters.AddWithValue("@Company", Name);
                        cmd.Parameters.AddWithValue("@DeviceMacID", macId);
                        cmd.Parameters.AddWithValue("@DeviceName", devicename);
                        dataList.Add(data);
                        cn.Open();
                        cmd.ExecuteNonQuery();
                        cn.Close();
                    }
                }
                data.Msg = "Sunucu log kaydı başarıyla kaydedilmiştir.";
                data.Result = true;
            }
            catch (Exception ex)
            {
                data.Msg = ex.Message;
                data.Result = false;
            }


            return Json(dataList);
        }

        //Login
        [HttpPost, Route("Api/UserData_Manage")]
        public JsonResult Manage_User(string username, string password)
        {

            int Customer = 0;
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();
            string UseSQL = "";
            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];
            //security error (when you enter ' character it brokes the sql)
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                if (username != null && password != null)
                {
                    UseSQL = "SELECT TOP 1 [CustomerID],[Username],[Password],[Name],[Surname],[Company] FROM [ServerViewer].[dbo].[Customers] Where Username='" + username + "' And Password='" + password + "'";
                    ErrorViewModel data = new ErrorViewModel();
                    SqlCommand command = new SqlCommand(UseSQL, connection);
                    using (SqlConnection cn = new SqlConnection("data source=SQL5;Database=ServerViewer;uid=semih;pwd=semih;"))
                    {
                        using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 [CustomerID],[Username],[Password],[Name],[Surname],[Company] FROM [ServerViewer].[dbo].[Customers] Where Username=@username And Password=@password", cn))
                        {
                            cn.Open();

                            cmd.CommandType = CommandType.Text;
                            command.CommandType = CommandType.Text;

                            cmd.Parameters.AddWithValue("@username", username);
                            cmd.Parameters.AddWithValue("@password", password);
                            SqlDataReader ssr = cmd.ExecuteReader();
                            while (ssr.Read())
                            {
                                Customer = Convert.ToInt32(ssr["CustomerID"]);
                                data.Name = Convert.ToString(ssr["Name"]);
                                data.CustomerId = Convert.ToInt32(ssr["CustomerID"]);
                                data.Username = Convert.ToString(ssr["Username"]);
                                data.Password = Convert.ToString(ssr["Password"]);
                            }
                            cn.Close();
                            HttpContext.Session.SetInt32("KullaniciEmail", Customer);
                            dataList.Add(data);
                            cn.Open();
                            cmd.ExecuteNonQuery();
                            cn.Close();

                        }
                    }
                }
                else
                {
                    HttpContext.Session.Clear();
                }
                connection.Close();
            }
            return Json(dataList);
        }
        public IActionResult Register()
        {
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Redirect("/Home/Register");
            return View();
        }
        
        //forgetmypassword
        [HttpPost, Route("Api/Reset_Password")]
        public JsonResult Reset_Password(string username, string email, string mac)
        {
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();
            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

            //mail send
            string newpass = CreatePassword(8);
            ErrorViewModel data = new ErrorViewModel();
 
            //till
            using (SqlConnection cn = new SqlConnection("data source=SQL5;Database=ServerViewer;uid=semih;pwd=semih;"))
            {
                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 [CustomerID],[Username],[Password],[EMail],[Name],[Surname],[Company] FROM [ServerViewer].[dbo].[Customers] Where Username=@username And EMail=@email", cn))
                {
                    cn.Open();

                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@email", email);
                    SqlDataReader ssr = cmd.ExecuteReader();
                    while (ssr.Read())
                    {
                        data.Username = Convert.ToString(ssr["Username"]);
                        data.Email = Convert.ToString(ssr["EMail"]);
                    }
                    cn.Close();
                    dataList.Add(data);
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    cn.Close();
                    if (data.Username == username && data.Email == email)
                    {
                        try
                        {
                            var body = new StringBuilder();
                            body.AppendLine("Şifreniz Sıfırlanmıştır: ");
                            body.AppendLine("Yeni Şifreniz : " + newpass);
                            var fromAddress = new MailAddress("noreply@vodatech.com.tr");
                            var toAddress = new MailAddress(email); //bilgi.islem@vodatech.com.tr
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
                                using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body.ToString() })
                                {
                                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                                    smtp.Send(message);
                                }
                            }

                        }
                        catch (Exception ex)
                        {

                            throw;
                        }

                        using (SqlCommand comm = new SqlCommand("UPDATE [ServerViewer].[dbo].[Customers] SET Password='" + newpass + "' WHERE EMail=@email AND Username=@username", cn))
                        {
                            comm.CommandType = CommandType.Text;
                            comm.Parameters.AddWithValue("@username", username);
                            comm.Parameters.AddWithValue("@email", email);
                            cn.Open();
                            comm.ExecuteNonQuery();
                            cn.Close();
                        }
                    }
                    else
                    {
                        data.Msg = "No such account";
                    }
                }
            }


            return Json(dataList);
        }
        //bring data with macID
        [HttpPost, Route("Api/DeviceList_Eject")]
        public JsonResult Device_List_Eject(string Macid, string CihazAd)
        {
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();

            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];
            var email = HttpContext.Session.GetInt32("KullaniciEmail");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT [Code],[DeviceMacID],[CustomerID],[DeviceName] FROM [ServerViewer].[dbo].[DeviceInfo] WHERE CustomerID=@CustomerID AND DeviceMacID=@DeviceMacID",connection))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@CustomerID", email);
                    cmd.Parameters.AddWithValue("@DeviceMacID", Macid);
                    SqlDataReader dataReader = cmd.ExecuteReader();

                    while (dataReader.Read())
                    {
                        ErrorViewModel data = new ErrorViewModel();
                        data.Name = Convert.ToString(dataReader["DeviceName"]);
                        data.CustomerId = Convert.ToInt32(dataReader["CustomerID"]);
                        data.DeviceCode = Convert.ToString(dataReader["Code"]);
                        data.MacID = Convert.ToString(dataReader["DeviceMacID"]);
                        dataList.Add(data);
                    }
                    connection.Close();
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }

                connection.Close();
            }
            return Json(dataList);
        }

        //Notification INSERT TO DB(to deleted)
        [HttpPost, Route("Api/Notification_Log")]
        public JsonResult Notification_Log(int CustomerID, string dName, string macId, string notification)
        {

            ErrorViewModel data = new ErrorViewModel();
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();

            try
            {
                using (SqlConnection cn = new SqlConnection("data source=SQL5;Database=ServerViewer;uid=semih;pwd=semih;"))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO [ServerViewer].[dbo].[Notifications] ([CustomerID],[NotificationContent],[DeviceMacID],[DeviceName]) VALUES(@CustomerID,@NotificationContent,@DeviceMacID,@DeviceName)", cn))
                    {
                        //(@CustomerID,@Username,@Phone,@EMail,@Password,@DeviceName,@DeviceMacID)
                        cmd.CommandType = CommandType.Text;
                        //cmd.Parameters.AddWithValue("@CustomerID", email);

                        cmd.Parameters.AddWithValue("@CustomerID", CustomerID);
                        cmd.Parameters.AddWithValue("@NotificationContent", notification);
                        cmd.Parameters.AddWithValue("@DeviceMacID", macId);
                        cmd.Parameters.AddWithValue("@DeviceName", dName);
                        dataList.Add(data);
                        cn.Open();
                        cmd.ExecuteNonQuery();
                        cn.Close();

                    }
                }
                data.Msg = "Sunucu log kaydı başarıyla kaydedilmiştir.";
                data.Result = true;
            }
            catch (Exception ex)
            {
                data.Msg = ex.Message;
                data.Result = false;
            }

            return Json(dataList);
        }

        //Select anormal temperatures from the server. 
        [HttpPost, Route("Api/Select_Notify")]
        public JsonResult Select_Notify(string Macid)
        {
            var email = HttpContext.Session.GetInt32("KullaniciEmail");

            List<ErrorViewModel> dataList = new List<ErrorViewModel>();
            string UseSQL = "SELECT TOP 3 [CustomerID],[NotificationContent],[DeviceMacID],[DeviceName],[CreateTime] FROM [ServerViewer].[dbo].[Notifications] WHERE CustomerID='" + email + "' AND Type='general' ORDER BY CreateTime DESC";
            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(UseSQL, connection);
                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        command.CommandType = CommandType.Text;
                        ErrorViewModel data = new ErrorViewModel();
                        data.CustomerId = Convert.ToInt32(dataReader["CustomerID"]);
                        data.NotificationMessage = Convert.ToString(dataReader["NotificationContent"]);
                        data.MacID = Convert.ToString(dataReader["DeviceMacID"]);
                        data.Name = Convert.ToString(dataReader["DeviceName"]);
                        data.CreateTime = Convert.ToString(dataReader["CreateTime"]);
                        dataList.Add(data);

                    }
                }

                connection.Close();
            }
            return Json(dataList);
        }

        //selects which notification message to give
        [HttpPost, Route("Api/Select_Message")]
        public JsonResult Select_Message(string Macid)
        {
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();
            var email = HttpContext.Session.GetInt32("KullaniciEmail");
            try
            {
                string connectionString = Configuration["ConnectionStrings:DefaultConnection"];
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT TOP 4 [CustomerID],[NotificationContent],[DeviceMacID],[DeviceName],[CreateTime] FROM [ServerViewer].[dbo].[Notifications] WHERE CustomerID=@CustomerID AND DeviceMacID=@DeviceMacID AND Type='general' ORDER BY CreateTime DESC",connection))
                    {
                        
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@CustomerID", email);
                        cmd.Parameters.AddWithValue("@DeviceMacID", Macid);
                        SqlDataReader dataReader = cmd.ExecuteReader();
                        while (dataReader.Read())
                        {
                            
                            ErrorViewModel data = new ErrorViewModel();
                            data.CustomerId = Convert.ToInt32(dataReader["CustomerID"]);
                            data.NotificationMessage = Convert.ToString(dataReader["NotificationContent"]);
                            data.MacID = Convert.ToString(dataReader["DeviceMacID"]);
                            data.Name = Convert.ToString(dataReader["DeviceName"]);
                            data.CreateTime = Convert.ToString(dataReader["CreateTime"]);
                            dataList.Add(data);

                        }
                        connection.Close();
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {

            }

            return Json(dataList);
        }

        [HttpPost, Route("Api/NewDevice_Create")]
        public JsonResult New_Device(string macId, string DeviceName)
        {
            ErrorViewModel data = new ErrorViewModel();
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();

            var email = HttpContext.Session.GetInt32("KullaniciEmail");
            try
            {
                using (SqlConnection cn = new SqlConnection("data source=SQL5;Database=ServerViewer;uid=semih;pwd=semih;"))
                {
                    using (SqlCommand cmd = new SqlCommand("spDeviceCreate", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@DeviceMacID", macId);
                        cmd.Parameters.AddWithValue("@CustomerID", email);
                        cmd.Parameters.AddWithValue("@DeviceName", DeviceName);
                        dataList.Add(data);
                        cn.Open();
                        cmd.ExecuteNonQuery();
                        cn.Close();
                    }
                }
                data.Msg = "Sunucu log kaydı başarıyla kaydedilmiştir.";
                data.Result = true;
            }
            catch (Exception ex)
            {
                data.Msg = ex.Message;
                data.Result = false;
            }
            return Json(dataList);
        }
        //morris
        [HttpPost, Route("Api/Morris_Chart")]
        public JsonResult Morris_Chart(string Macid)
        {
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();
            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];
            var email = HttpContext.Session.GetInt32("KullaniciEmail");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string UseSQL = "Select t1.[Key] Temp,t1.[Value] tempkey,t2.[Key] Mois,t2.[Value] moiskey,[CustomerID],[DeviceMacID] FROM [ServerViewer].[dbo].[DeviceInfo] AS tbl LEFT JOIN (SELECT DISTINCT t.[DeviceID],t.[MacID], t.[CreateTime], t.[Key], t.[Value]  FROM ServerStatusTemp t INNER JOIN (SELECT MacID, MAX(CreateTime) CreateTime FROM ServerStatusTemp  WHERE [Value]=MacID+'/t' GROUP BY MacID) tm ON t.MacID = tm.MacID and t.CreateTime = tm.CreateTime ) AS t1 ON tbl.DeviceMacID= t1.MacID LEFT JOIN (SELECT DISTINCT t.[DeviceID],t.[MacID], t.[CreateTime], t.[Key], t.[Value] FROM ServerStatusTemp t INNER JOIN (SELECT MacID, MAX(CreateTime) CreateTime FROM ServerStatusTemp WHERE [Value]=MacID+'/h' GROUP BY MacID) tm ON t.MacID = tm.MacID and t.CreateTime = tm.CreateTime ) AS t2 ON tbl.DeviceMacID= t2.MacID  WHERE tbl.CustomerID='" + email + "'";
                SqlCommand command = new SqlCommand(UseSQL, connection);
                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        command.CommandType = CommandType.Text;
                        ErrorViewModel data = new ErrorViewModel();
                        data.CustomerId = Convert.ToInt32(dataReader["CustomerID"]);
                        data.Temp = float.Parse(dataReader["Temp"].ToString());
                        data.Mois = float.Parse(dataReader["Mois"].ToString());
                        data.MacID = Convert.ToString(dataReader["DeviceMacID"]);
                        dataList.Add(data);

                    }
                }

                connection.Close();
            }
            return Json(dataList);
        }
        //profile (user) info
        [HttpPost, Route("Api/Profile_UserData")]
        public JsonResult Profile_UserData(string Macid)
        {
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();
            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];
            var email = HttpContext.Session.GetInt32("KullaniciEmail");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string UseSQL = "SELECT [CustomerID],[Name],[Surname],[DeviceMacID],[Company],[Phone],[EMail] FROM [ServerViewer].[dbo].[Customers] WHERE CustomerID='" + email + "'";
                SqlCommand command = new SqlCommand(UseSQL, connection);
                using (SqlDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        command.CommandType = CommandType.Text;
                        ErrorViewModel data = new ErrorViewModel();
                        data.CustomerId = Convert.ToInt32(dataReader["CustomerID"]);
                        data.Name = Convert.ToString(dataReader["Name"]);
                        data.surname = Convert.ToString(dataReader["Surname"]);
                        data.MacID = Convert.ToString(dataReader["DeviceMacID"]);
                        data.Company = Convert.ToString(dataReader["Company"]);
                        data.PhoneNumber = Convert.ToString(dataReader["Phone"]);
                        data.Email = Convert.ToString(dataReader["EMail"]);
                        dataList.Add(data);

                    }
                }

                connection.Close();
            }
            return Json(dataList);
        }

        //profile(user) info edit
        [HttpPost, Route("Api/Profile_Userdata_Update")]
        public JsonResult Profile_Edit(string name, string surname, string emailaddr, string phone, string company)
        {

            ErrorViewModel data = new ErrorViewModel();
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();

            var email = HttpContext.Session.GetInt32("KullaniciEmail");
            try
            {
                using (SqlConnection cn = new SqlConnection("data source=SQL5;Database=ServerViewer;uid=semih;pwd=semih;"))
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE [ServerViewer].[dbo].[Customers] SET Name=@Name,Surname=@Surname,EMail=@EMail,Phone=@Phone,Company=@Company WHERE CustomerID=@CustomerID", cn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@CustomerID", email);
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Surname", surname);
                        cmd.Parameters.AddWithValue("@EMail", emailaddr);
                        cmd.Parameters.AddWithValue("@Phone", phone);
                        cmd.Parameters.AddWithValue("@Company", company);

                        dataList.Add(data);
                        cn.Open();
                        cmd.ExecuteNonQuery();
                        cn.Close();
                    }
                }
                data.Msg = "Sunucu log kaydı başarıyla kaydedilmiştir.";
                data.Result = true;
            }
            catch (Exception ex)
            {
                data.Msg = ex.Message;
                data.Result = false;
            }


            return Json(dataList);
        }
        //SELECT EMAIL LIST
        [HttpPost, Route("Api/EmailList")]
        public JsonResult Get_Emails(string Macid)
        {
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();
            
            string connectionString = Configuration["ConnectionStrings:DefaultConnection"];
            var email = HttpContext.Session.GetInt32("KullaniciEmail");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT [CustomerID],[DeviceMacID],[EMail] FROM [ServerViewer].[dbo].[EmailList] WHERE CustomerID=@CustomerID AND DeviceMacID=@DeviceMacID",connection))
                {
                    cmd.CommandType = CommandType.Text;
                    
                    
                    cmd.Parameters.AddWithValue("@CustomerID", email);
                    cmd.Parameters.AddWithValue("@DeviceMacID", Macid);
                    SqlDataReader dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                    ErrorViewModel data = new ErrorViewModel();
                        data.MacID = Convert.ToString(dataReader["DeviceMacID"]);
                        data.Email = Convert.ToString(dataReader["EMail"]);
                        dataList.Add(data);
                    }
                    connection.Close();
                    
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    connection.Close();
                   
                }

            }
            return Json(dataList);
        }

        //INSERT EMAIL LIST
        [HttpPost, Route("Api/SetEmailList")]
        public JsonResult Set_Email_List(string Macid, string[] sEmail)
        {

            ErrorViewModel data = new ErrorViewModel();
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();
            string connectionString = "data source=SQL5;Database=ServerViewer;User ID=semih; Password=semih;";
            var email = HttpContext.Session.GetInt32("KullaniciEmail");
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("Delete From EmailList WHERE CustomerID=@CustomerID AND DeviceMacID=@DeviceMacID", cn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@CustomerID", email);
                    cmd.Parameters.AddWithValue("@DeviceMacID", Macid);
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    cn.Close();

                }
            }
            foreach (string item in sEmail)
            {
                using (SqlConnection cn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.EmailList(CustomerID,DeviceMacID,EMail) VALUES(@CustomerID,@DeviceMacID,@EMail) ", cn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@CustomerID", email);
                        cmd.Parameters.AddWithValue("@DeviceMacID", Macid);
                        cmd.Parameters.AddWithValue("@EMail", item);
                        if (item != null)
                        {
                            cn.Open();
                            cmd.ExecuteNonQuery();
                        }
                        if (item == null)
                        {
                            cn.Close();
                        }



                        cn.Close();

                    }
                }
            }


            return Json(dataList);
        }

        //SET DELAY
        [HttpPost, Route("Api/SetDelay")]
        public JsonResult Set_Notification_Delay(string Macid, int delayValue, string type)
        {
            ErrorViewModel data = new ErrorViewModel();
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();
            string connectionString = "data source=SQL5;Database=ServerViewer;User ID=semih; Password=semih;";
            var email = HttpContext.Session.GetInt32("KullaniciEmail");

            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE [ServerViewer].[dbo].[DeviceInfo] SET " + type + "=" + delayValue + " WHERE CustomerID=@CustomerID AND DeviceMacID=@DeviceMacID", cn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@CustomerID", email);
                    cmd.Parameters.AddWithValue("@DeviceMacID", Macid);
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    cn.Close();

                }
            }
            return Json(dataList);
        }

        [HttpPost, Route("Api/GetDelay")]
        public JsonResult Get_Notification_Delay(string Macid)
        {
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();
            ErrorViewModel data = new ErrorViewModel();
            var email = HttpContext.Session.GetInt32("KullaniciEmail");
            using (SqlConnection connection = new SqlConnection("data source=SQL5;Database=ServerViewer;uid=semih;pwd=semih;"))
            {
                
                using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 [DelayTempUp],[DelayTempDown],[DelayMoisUp],[DelayMoisDown],[DeviceName] FROM [ServerViewer].[dbo].[DeviceInfo] WHERE CustomerID=@CustomerID AND DeviceMacID=@DeviceMacID",connection))
                {
                    connection.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@DeviceMacID", Macid);
                    cmd.Parameters.AddWithValue("@CustomerID", email);
                    SqlDataReader dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {

                        
                        data.DelayTempUp = Convert.ToInt32(dataReader["DelayTempUp"]);
                        data.DelayTempDown = Convert.ToInt32(dataReader["DelayTempDown"]);
                        data.DelayMoisUp = Convert.ToInt32(dataReader["DelayMoisUp"]);
                        data.DelayMoisDown = Convert.ToInt32(dataReader["DelayMoisDown"]);
                        data.DeviceName = Convert.ToString(dataReader["DeviceName"]);
                        
                    }
                    connection.Close();
                    dataList.Add(data);
                    connection.Open();
                    cmd.ExecuteNonQuery();

                }

                connection.Close();
            }
            return Json(dataList);
        }
        
        [HttpPost, Route("Api/NewDevice_Create_ByAdmin")]
        public JsonResult New_Device_ByAdmin(string macId, string code)
        {
            ErrorViewModel data = new ErrorViewModel();
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();
            try
            {
                using (SqlConnection cn = new SqlConnection("data source=SQL5;Database=ServerViewer;uid=semih;pwd=semih;"))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO [ServerViewer].[dbo].[DeviceInfo](DeviceMacID,Code) VALUES(@DeviceMacID,@Code)", cn))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@DeviceMacID", macId);
                        cmd.Parameters.AddWithValue("@Code", code);
                        dataList.Add(data);
                        cn.Open();
                        cmd.ExecuteNonQuery();
                        cn.Close();
                    }
                }
                data.Msg = "Sunucu log kaydı başarıyla kaydedilmiştir.";
                data.Result = true;
            }
            catch (Exception ex)
            {
                data.Msg = ex.Message;
                data.Result = false;
            }
            return Json(dataList);
        }
    }
}