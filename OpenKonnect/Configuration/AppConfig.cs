using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace OpenKonnect.Configuration
{
    public class AppConfig
    {
        private readonly string connectionString = Convert.ToString(ConfigurationManager.AppSettings["ConnectionString"]);
        private readonly bool fakeMode = Convert.ToBoolean(ConfigurationManager.AppSettings["FakeMode"]);
        private readonly bool safeMode = Convert.ToBoolean(ConfigurationManager.AppSettings["SafeMode"]);
        private readonly int garbageCollectorInterval_sec = Convert.ToInt32(ConfigurationManager.AppSettings["GarbageCollectorInterval_sec"]);
        private readonly int fetchDefaultInterval_sec = Convert.ToInt32(ConfigurationManager.AppSettings["FetchDefaultInterval_sec"]);
        private readonly bool updateClocks_Active = Convert.ToBoolean(ConfigurationManager.AppSettings["UpdateClocks_Active"]);
        private readonly DateTime updateClocks_TimeOfDay = DateTime.ParseExact(ConfigurationManager.AppSettings["UpdateClocks_TimeOfDay"], "HHmmss", null);
        private readonly int updateClocks_Interval_sec = Convert.ToInt32(ConfigurationManager.AppSettings["UpdateClocks_Interval_sec"]);
        private readonly int updateClocks_WithinTime_msec = Convert.ToInt32(ConfigurationManager.AppSettings["UpdateClocks_WithinTime_msec"]);

        public string ConnectionString
        {
            get { return connectionString; }
        }

        public bool FakeMode
        {
            get { return fakeMode; }
        }

        public bool SafeMode
        {
            get { return safeMode; }
        }

        public int GarbageCollectorInterval_sec
        {
            get { return garbageCollectorInterval_sec; }
        }

        public int FetchDefaultInterval_sec
        {
            get { return fetchDefaultInterval_sec; }
        }

        public bool UpdateClocks_Active
        {
            get { return updateClocks_Active; }
        }

        public DateTime UpdateClocks_TimeOfDay
        {
            get { return updateClocks_TimeOfDay; }
        }

        public int UpdateClocks_Interval_sec
        {
            get { return updateClocks_Interval_sec; }
        }

        public int UpdateClocks_WithinTime_msec
        {
            get { return updateClocks_WithinTime_msec; }
        }
    }
}
