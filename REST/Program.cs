using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REST
{
   
   public class Program
   {
      public class Messages
      {
         private List<string> _messages = new List<string>();

         // operator overloading for []
         public string this[int i]
         {
            get { return _messages[i]; }
            set { _messages[i] = value; }
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
            response.Body = (_messages.Count - 1).ToString();
            return response;
         }

         public ResponseContext Show(RequestContext reqC)
         {
            string path = reqC.getPath();
            Int32.TryParse(path.Substring(path.LastIndexOf('/') + 1), out int i);

            ResponseContext response = new ResponseContext();
            try
            {
               response.Body = this[i - 1];
               response.Status = StatusCode.OK;
            }
            catch (Exception)
            {
               response.Status = StatusCode.Not_Found;
            }
            return response;
         }

         public ResponseContext Update(RequestContext reqC)
         {
            string path = reqC.getPath();
            Int32.TryParse(path.Substring(path.LastIndexOf('/') + 1), out int i);

            ResponseContext response = new ResponseContext();
            try
            {
               this[i - 1] = reqC.getBody();
               response.Status = StatusCode.OK;
            }
            catch (Exception)
            {
               response.Status = StatusCode.Not_Found;
            }
            return response;
         }

         public ResponseContext Delete(RequestContext reqC)
         {
            string path = reqC.getPath();
            Int32.TryParse(path.Substring(path.LastIndexOf('/') + 1), out int i);

            ResponseContext response = new ResponseContext();
            try
            {
               _messages.RemoveAt(i - 1);
               response.Status = StatusCode.OK;
            }
            catch (Exception)
            {
               response.Status = StatusCode.Not_Found;
            }
            return response;
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
