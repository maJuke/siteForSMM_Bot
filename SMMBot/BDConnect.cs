using MySql.Data.MySqlClient;

namespace SMMBot
{
    internal class BDConnect
    {
        private BDConnect() 
        { 
        }

        public string Server {  get; set; }
        public string BDName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public MySqlConnection Connection { get; set; }

        private static BDConnect _instance = null;
        public static BDConnect Instance()
        {
            if (_instance == null)
                _instance = new BDConnect();
            return _instance;
        }

        public bool IsConnect()
        {
            if (Connection == null)
            {
                if (String.IsNullOrEmpty(BDName))
                    return false;
                string connstring = string.Format("Server={0}; database={1}; UID={2}; password={3}", Server, BDName, UserName, Password);
                Connection = new MySqlConnection(connstring);
                Connection.Open();
            }

            return true;
        }

        public void Close()
        {
            Connection.Close();
        }
    }
}
