using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REST
{

   public class ResponseContext
   {
      private string _version;   // HTTP protocol version
      private Dictionary<string, string> _headers = new Dictionary<string, string>(); // HTTP headers
      public string Body { get; set; } // HTTP body
      public StatusCode Status{ get; set; }

      public ResponseContext()
      {
         _version = "HTTP/1.1";
      }

      public ResponseContext(StatusCode code)
      {
         _version = "HTTP/1.1";
         Status = code;
      }


      public override string ToString()
      {
         string ret = "";
         ret += $"{_version} {StatusToFriendlyString(Status)}\r\n";

         foreach (var header in _headers)
         {
            ret += $"{header.Key}: {header.Value}\r\n";
         }
         ret += "\r\n";
         ret += Body;
         return ret;
      }

      public static string StatusToFriendlyString(StatusCode code)
      {
         switch (code)
         {
            case StatusCode.OK:
               return "200 OK";
            case StatusCode.Bad_Request:
               return "400 Bad Request";
            case StatusCode.Unauthorized:
               return "401 Unauthorized";
            case StatusCode.Not_Found:
               return "404 Not Found";
            default:
               return "500 Internal Server Error";
         }
      }
   }
}
