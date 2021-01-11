using Npgsql;
using REST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
   class User
   {
      public int ID { get; private set; }
      public string Username { get; private set; }
      //private int[] _cards;
      private int _coins;


      public User(string username)
      {
         try
         {
            Database.Con.Open();
            var cmd = new NpgsqlCommand($"select * from usr where name = '{username}'", Database.Con);
            var qr = cmd.ExecuteReader();
            if (!qr.HasRows) { throw new HttpException(StatusCode.Unauthorized); }
            qr.Read();
            ID = qr.GetInt32(0);
            Username = username;
            _coins = qr.GetInt32(3);
            Database.Con.Close();
         }
         catch (Exception e)
         {
            if (Database.Con.State == System.Data.ConnectionState.Open)
            {
               Database.Con.Close();
            }
            throw e;
         }
      }
   }
}
