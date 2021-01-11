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
         User u = User.Register(username, password);
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
         User u = User.Login(username, password);
         return new ResponseContext(StatusCode.OK);
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

         
         //server.Endpoint.RegisterEndpoint("/messages", Method.GET, msg.List);

         server.Run();
      }
   }
}
