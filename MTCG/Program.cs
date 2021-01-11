using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using REST;

// only for testing
using Npgsql;

namespace MTCG
{
   public class Test
   {
      public void DbConnect()
      {
         var db = Database.db;

         var sql = "SELECT version()";

         var version = db.sendCmd(sql);
         Console.WriteLine($"PostgreSQL version: {version}");

      }
      public ResponseContext Abc(RequestContext reqC)
      {
         //foreach (var item in reqC.Headers)
         //{
         //   Console.WriteLine(item.Key);

         //}
         //Console.WriteLine(reqC.Headers["Username"]);
         Console.WriteLine(reqC.ToString());
         //var temp = reqC.getBodyJson();
         //foreach (var item in reqC.getBodyJson())
         //{
         //   Console.WriteLine(item.Key);

         //}
         return new ResponseContext();
      }
   }
   class Program
   {
      static void Main(string[] args)
      {
         


         HttpServer server = new HttpServer(10001);
         Game game = new Game();
         Test t = new Test();

         t.DbConnect();


         server.Endpoint.RegisterEndpoint("/users", Method.POST, game.Register);
         server.Endpoint.RegisterEndpoint("/sessions", Method.POST, game.Login);
         server.Endpoint.RegisterEndpoint("/packages", Method.POST, game.CreatePackage);
         server.Endpoint.RegisterEndpoint("/transactions/packages", Method.POST, game.AcquirePackage);
         server.Endpoint.RegisterEndpoint("/cards", Method.GET, game.GetCollection);
         server.Endpoint.RegisterEndpoint("/deck", Method.GET, game.GetDeck);
         server.Endpoint.RegisterEndpoint("/deck", Method.PUT, game.EditDeck);
         server.Endpoint.RegisterEndpoint("/battles", Method.POST, game.QueueFight);


         

         //server.Endpoint.RegisterEndpoint("/messages", Method.GET, msg.List);

         server.Run();
      }
   }
}
