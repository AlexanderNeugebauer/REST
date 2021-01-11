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
      private User _defender;
      private User _challenger;
      private Random random = new Random();
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
         var temp = new ResponseContext(StatusCode.OK);
         if (user.BuyPackage()) { temp.Body = "'purchase': 'success'"; }
         else { temp.Body = "'purchase': 'failed'"; }
         return temp;
      }

      public ResponseContext GetCollection(RequestContext reqC)
      {
         User user = Authorize(reqC);

         var response = new ResponseContext(StatusCode.OK);
         response.Body = user.getCollection();

         return response;
      }

      public ResponseContext GetDeck(RequestContext reqC)
      {
         User user = Authorize(reqC);

         var response = new ResponseContext(StatusCode.OK);
         response.Body = user.getDeckJson();

         return response;
      }

      public ResponseContext EditDeck(RequestContext reqC)
      {
         User user = Authorize(reqC);

         try
         {
            var body = JObject.Parse($"{{\"cards\": {reqC.Body}}}");

            Database.Con.Open();
            //Database.Con.BeginTransaction();
            string cardIDs = "(";
            int i = 0;
            foreach (var card in body.First.First)
            {
               Console.WriteLine(card.ToString());
               i++;
               //new NpgsqlCommand($"insert into cards (id, name, damage) values ('{card["Id"]}', '{card["Name"]}', '{card["Damage"]}');", Database.Con).ExecuteNonQuery();
               cardIDs += $"'{card.ToString()}', ";
            }
            if (i != 4 ) { throw new HttpException(StatusCode.Bad_Request); }
            cardIDs = cardIDs.Substring(0, cardIDs.Length - 2);
            new NpgsqlCommand($"update cards set in_deck = false where belongs_to = {user.ID};", Database.Con).ExecuteNonQuery();
            Console.WriteLine($"update cards set in_deck = true where belongs_to = {user.ID} and id in {cardIDs});");
            new NpgsqlCommand($"update cards set in_deck = true where belongs_to = {user.ID} and id in {cardIDs});", Database.Con).ExecuteNonQuery();
            //Console.WriteLine($"insert into package values {cardIDs});");
            //new NpgsqlCommand($"insert into package values {cardIDs});", Database.Con).ExecuteNonQuery();
            Database.Con.Close();

            return new ResponseContext(StatusCode.OK);
         }
         catch (Exception e)
         {
            Console.WriteLine(e.ToString());
            
            Database.Con.Close();
            throw;
         }
      }

      public ResponseContext QueueFight(RequestContext reqC)
      {
         User user = Authorize(reqC);

         if (_defender == null)
         {
            _defender = user;
         }
         else if (_challenger == null)
         {
            _challenger = user;
            try
            {
               Fight(_defender, _challenger);   

            }
            catch (Exception e)
            {
               Console.WriteLine(e.StackTrace);
               throw;
            }


            _defender = null;
            _challenger = null;
         }


         var response = new ResponseContext(StatusCode.OK);
         response.Body = user.getDeckJson();
         return response;
      }

      public void Fight(User defender, User challenger)
      {
         string fightLog = $"--------------------------\n{defender.Username} vs {challenger.Username}\n--------------------------\n";
         int d, c;
         double d_dmg, c_dmg; 
         var d_deck = defender.getDeckList();
         var c_deck = challenger.getDeckList();

         for (int i = 0; i < 100; i++)
         {
            fightLog += $"Round: #{i}\n";
            d = random.Next(d_deck.Count);
            c = random.Next(c_deck.Count);

            Console.WriteLine(d);
            Console.WriteLine(c);

            c_dmg = c_deck[c].CalcDamage(d_deck[d]);
            d_dmg = d_deck[d].CalcDamage(c_deck[c]);
            fightLog += $"{d_deck[d].Name}: {d_dmg} | {c_deck[c].Name}: {c_dmg}\n";
            if (d_dmg > c_dmg)
            {
               fightLog += $"{defender.Username} wins this round!\n\n";
               c_deck.Add(d_deck[d]);
               d_deck.RemoveAt(d);
            } else if (d_dmg < c_dmg)
            {
               fightLog += $"{challenger.Username} wins this round!\n\n";
               d_deck.Add(c_deck[c]);
               c_deck.RemoveAt(c);
            }

            if (c_deck.Count == 0)
            {
               fightLog += $"{defender.Username} wins the battle!\n\n";
               break;
            }
            if (d_deck.Count == 0)
            {
               fightLog += $"{challenger.Username} wins the battle!\n\n";
               break;
            }
         }
         Console.WriteLine(fightLog);
      }
   }
}

