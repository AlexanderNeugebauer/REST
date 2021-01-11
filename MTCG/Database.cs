using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace MTCG
{
   class Database
   {
      public static NpgsqlConnection Con = new NpgsqlConnection("Host=localhost;Username=postgres;Password=12345;Database=MTCG");
      public static Database db = new Database();
      private NpgsqlConnection _con;

      private Database()
      {
         var cs = "Host=localhost;Username=postgres;Password=12345;Database=MTCG";

         _con = new NpgsqlConnection(cs);
      }

      public string sendCmd(string sql)
      {
         _con.Open();
         var cmd = new NpgsqlCommand(sql, _con);
         var qr = cmd.ExecuteReader();
         string result = "";
         while (qr.Read())
         {
            Console.WriteLine($"abc: {qr[0]}");
            result += qr[0].ToString();
         }
         //   result
         //if (result == null) { result == ""};
         //string result = cmd.ExecuteScalar().ToString();
         _con.Close();

         return result;
      }
   }
}
