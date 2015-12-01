using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace RFID
{
    class Database
        
    {
        private String connectionstring = null;
        private SqlConnection conn;
    

        public SqlConnection connection()
        {

            connectionstring = "Data Source=DESKTOP-0VIDBIA\\SQLEXPRESS;Initial Catalog=demo;Integrated Security=True";
            conn = new SqlConnection(connectionstring);
            return conn;

        }
    }
}
