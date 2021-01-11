using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Npgsql;
using REST;

namespace MTCG
{
   class Game
   {
      private User Authorize(RequestContext reqC)
      {
         string token = "";
         if (!reqC.Headers.TryGetValue("Authorization", out token))
         {
            throw new HttpException(StatusCode.Unauthorized);
         }
         
         token = token.Replace("Basic ", "");
         token = token.Replace("-mtcgToken", "");

         return new User(token);
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
      public ResponseContext Register(RequestContext reqC)
      {
         var body = reqC.getBodyJson();
         string username, password;
         if (!body.TryGetValue("Username", out username))
         {
            throw new HttpException(StatusCode.Bad_Request);
         }
         if (!body.TryGetValue("Password", out password))
         {
            throw new HttpException(StatusCode.Bad_Request);
         }
         //User u = User.Register(username, password);

         try
         {
            Database.Con.Open();
            string count = new NpgsqlCommand($"select count(*) from usr where name = '{username}'", Database.Con).ExecuteScalar().ToString();
            if (count != "0") { throw new Exception("Username already taken!"); }
            var hash = ComputeSha256Hash(password);

            new NpgsqlCommand($"insert into usr (name, password, coins) values ('{username}', '{hash}', 20);", Database.Con).ExecuteNonQuery();

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

         return new ResponseContext(StatusCode.OK);
      }

      public ResponseContext Login(RequestContext reqC)
      {
         var body = reqC.getBodyJson();
         string username, password;
         if (!body.TryGetValue("Username", out username))
         {
            throw new HttpException(StatusCode.Bad_Request);
         }
         if (!body.TryGetValue("Password", out password))
         {
            throw new HttpException(StatusCode.Bad_Request);
         }

         var response = new ResponseContext(StatusCode.OK);

         try
         {
            Database.Con.Open();
            var cmd = new NpgsqlCommand($"select * from usr where name = '{username}'", Database.Con);
            var qr = cmd.ExecuteReader();
            if (!qr.HasRows) { throw new HttpException(StatusCode.Unauthorized); }
            qr.Read();
            //var q_id = qr.GetInt32(0);
            var q_pw = qr.GetString(2);
            Database.Con.Close();
            if (q_pw != ComputeSha256Hash(password)) { throw new HttpException(StatusCode.Unauthorized); }
         }
         catch (Exception e)
         {
            if (Database.Con.State == System.Data.ConnectionState.Open)
            {
               Database.Con.Close();
            }
            throw e;
         }
         

         

         return response;
         
      }

      public ResponseContext CreatePackage(RequestContext reqC)
      {
         try
         {
            var body = JObject.Parse($"{{\"cards\": {reqC.Body}}}");

            Database.Con.Open();
            string cardIDs = "(";

            foreach (var card in body.First.First)
            {
               Console.WriteLine("----");
               //Console.WriteLine(card.ToString());
               //Console.WriteLine(card.Value.ToString());
               Console.WriteLine(card["Id"]);

               new NpgsqlCommand($"insert into cards (id, name, damage) values ('{card["Id"]}', '{card["Name"]}', '{card["Damage"]}');", Database.Con).ExecuteNonQuery();
               cardIDs += $"'{card["Id"]}', ";
            }
            cardIDs = cardIDs.Substring(0, cardIDs.Length - 2);
            Console.WriteLine($"insert into package values {cardIDs});");
            new NpgsqlCommand($"insert into package values {cardIDs});", Database.Con).ExecuteNonQuery();
            Database.Con.Close();

            return new ResponseContext(StatusCode.OK);
         }
         catch (Exception)
         {
            Database.Con.Close();
            throw;
         }
      }

      public ResponseContext AcquirePackage(RequestContext reqC)
      {
         User user = Authorize(reqC);

         user.BuyPackage();
         
         var temp = new ResponseContext(StatusCode.OK);
         temp.Body = user.ID.ToString();
         return temp;
      }
   }
}

