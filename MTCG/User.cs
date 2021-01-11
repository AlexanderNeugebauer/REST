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
      public int GamesPlayed { get; }
      public int GamesWon { get; }


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
            GamesPlayed = qr.GetInt32(3);
            GamesWon = qr.GetInt32(3);
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

      public bool BuyPackage()
      {
         if(_coins < 5) { return false; }
         _coins -= 5;
         //Dictionary<int, object> cards= new Dictionary<int, object>();
         List<string> ccards = new List<string>();
         try
         {
            Database.Con.Open();
            var cmd = new NpgsqlCommand($"select * from package limit 1;", Database.Con);
            var qr = cmd.ExecuteReader();
            if (!qr.HasRows) { return false; }
            qr.Read();
            int package_ID = qr.GetInt32(5);
            string cards = $"update cards set belongs_to = {ID} where id in ('";
            for (int i = 0; i < 5; i++)
            {
               cards += $"{qr.GetString(i)}', '";
            }
            cards = cards.Substring(0, cards.Length - 3);
            qr.Close();
            new NpgsqlCommand($"update usr set coins = {_coins} where uid = {ID};", Database.Con).ExecuteNonQuery();
            new NpgsqlCommand(cards + ");", Database.Con).ExecuteNonQuery();
            new NpgsqlCommand($"delete from package where id = {package_ID};", Database.Con).ExecuteNonQuery();
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


         return true;
      }

      public string getCollection()
      {
         try
         {
            Database.Con.Open();
            var cmd = new NpgsqlCommand($"select * from cards where belongs_to = {ID};", Database.Con);
            var qr = cmd.ExecuteReader();
            if (!qr.HasRows) { return ""; }
            List<Card> collection = new List<Card>();
            object[] buf = new object[16];
            while (qr.Read())
            {
               qr.GetValues(buf);
               collection.Add(new Card(buf));
            }
            Database.Con.Close();
            return Card.CollectionToJson(collection);
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

      public string getDeckJson()
      {
         try
         {
            Database.Con.Open();
            var cmd = new NpgsqlCommand($"select * from cards where belongs_to = {ID} and in_deck = true;", Database.Con);
            var qr = cmd.ExecuteReader();
            if (!qr.HasRows) { return ""; }
            List<Card> collection = new List<Card>();
            object[] buf = new object[16];
            while (qr.Read())
            {
               qr.GetValues(buf);
               collection.Add(new Card(buf));
            }
            Database.Con.Close();
            return Card.CollectionToJson(collection);
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

      public List<Card> getDeckList()
      {
         try
         {
            Database.Con.Open();
            var cmd = new NpgsqlCommand($"select * from cards where belongs_to = {ID} and in_deck = true;", Database.Con);
            var qr = cmd.ExecuteReader();
            List<Card> collection = new List<Card>();
            if (!qr.HasRows) { return collection; }
            object[] buf = new object[16];
            while (qr.Read())
            {
               qr.GetValues(buf);
               if(buf[1].ToString().Contains("Spell"))
               {
                  collection.Add(new SpellCard(buf));
               }
               else
               {
                  collection.Add(new MonsterCard(buf));
               }
               //collection.Add(new Card(buf));
            }
            Database.Con.Close();
            return collection;
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
