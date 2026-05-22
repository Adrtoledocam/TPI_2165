using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPI_ArcaludoApp.Services
{
    public static class Config
    {
        public static string BaseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:8080/api"
            : "http://localhost:8080/api";
    }
}
