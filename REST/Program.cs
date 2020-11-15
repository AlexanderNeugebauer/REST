using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REST
{
   
   class Program
   {
      public class Messages
      {
         List<string> _messages = new List<string>();

         // operator overloading for []
         public string this[int i]
         {
            get { return _messages[i]; }
            // set { _messages[i] = value; }
         }

         public ResponseContext List(RequestContext reqC)
         {
            ResponseContext response = new ResponseContext();
            string temp = "";
            for (int i = 0; i < _messages.Count; i++)
            {
               temp += $"{i}: {_messages[i]}\n";
            }
            response.Body = temp;
            response.Status = StatusCode.OK;

            return response;
         }

         public ResponseContext Add(RequestContext reqC)
         {
            _messages.Add(reqC.getBody());
            ResponseContext response = new ResponseContext(StatusCode.OK);
            response.Status = StatusCode.OK;
            response.Body = (_messages.Count - 1).ToString();
            return response;
         }

         public ResponseContext Show(RequestContext reqC)
         {
            return new ResponseContext();
         }

         public ResponseContext Update(RequestContext reqC)
         {
            return new ResponseContext();
         }

         public ResponseContext Delete(RequestContext reqC)
         {
            return new ResponseContext();
         }
      }
      static void Main(string[] args)
      {
         HttpServer server = new HttpServer();
         Messages msg = new Messages();
         server.Endpoint.RegisterEndpoint("/messages",   Method.GET,    msg.List);
         server.Endpoint.RegisterEndpoint("/messages",   Method.POST,   msg.Add);
         server.Endpoint.RegisterEndpoint("/messages/*", Method.GET,    msg.Show);
         server.Endpoint.RegisterEndpoint("/messages/*", Method.PUT,    msg.Update);
         server.Endpoint.RegisterEndpoint("/messages/*", Method.DELETE, msg.Delete);

         server.Run();
      }
   }
}
