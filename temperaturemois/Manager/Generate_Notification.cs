using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Results;
using temperaturemois.Models;

namespace temperaturemois.Manager
{
    public class Generate_Notification
    {
        public static List<ErrorViewModel> Notification_Log(int CustomerID, string notification, string macId,string dName)
        {

            ErrorViewModel data = new ErrorViewModel();
            List<ErrorViewModel> dataList = new List<ErrorViewModel>();
            try
            {
                using (SqlConnection cn = new SqlConnection("data source=SQL5;Database=ServerViewer;uid=semih;pwd=semih;"))
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO [ServerViewer].[dbo].[Notifications] ([CustomerID],[NotificationContent],[DeviceMacID],[DeviceName]) VALUES(@CustomerID,@NotificationContent,@DeviceMacID,@DeviceName)", cn))
                    {
                        cmd.CommandType = CommandType.Text;
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

            return dataList;
        }
    }
}
