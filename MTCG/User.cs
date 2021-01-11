using Npgsql;
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



      private User(int id, string username)
      {
         
      }

      private static string ComputeSha256Hash(string rawData)
      {
         // Create a SHA256   
         using (SHA256 sha256Hash = SHA256.Create())
         {
            // ComputeHash - returns byte array  
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // Convert byte array to a string   
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
               builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
         }
      }

      public static User Register(string username, string password)
      {
         Database.Con.Open();
         string count = new NpgsqlCommand($"select count(*) from usr where name = '{username}'", Database.Con).ExecuteScalar().ToString();
         if (count != "0") { throw new Exception("Username already taken!"); }
         var hash = ComputeSha256Hash(password);

         new NpgsqlCommand($"insert into usr (name, password) values ('{username}', '{hash}');", Database.Con).ExecuteNonQuery();
         
         Database.Con.Close();
         return Login(username, password);
      }

      public static User Login(string username, string password)
      {
         //string rawUserData = Database.db.sendCmd($"select * from usr where name = '{username}';");

         //Console.WriteLine("------");
         //Console.WriteLine(rawUserData);

         //var pw = "";
         //if (pw != ComputeSha256Hash(password)) { throw new Exception("Login failed!"); }


         // fetch user from database
         Database.Con.Open();
         var cmd = new NpgsqlCommand($"select * from usr where name = '{username}'", Database.Con);
         var qr = cmd.ExecuteReader();
         if (!qr.HasRows) { throw new Exception("Username not found!"); }
         qr.Read();
         Console.WriteLine("a");
         var q_id = qr.GetInt32(0);
         var q_pw = qr.GetString(2);
         Database.Con.Close();

         if (q_pw != ComputeSha256Hash(password)) { throw new Exception("Wrong Password!"); }
         

         return new User(q_id, username);
      }
   }
}
