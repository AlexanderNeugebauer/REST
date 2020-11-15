using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REST
{
   public class RequestContext
   {
      private Method _method;    // HTTP method token
      private string _uri;       // HTTP Request-URI
      private string _version;   // HTTP protocol version
      private Dictionary<string, string> _headers = new Dictionary<string, string>(); // HTTP headers
      private string _body;       // HTTP body
      public RequestContext(string data)
      {
         ParseFromString(data);
      }

      override public string ToString()
      {
         string ret = ($"{_method.ToString()} {_uri} {_version}\r\n");
         foreach (var header in _headers)
         {
            ret += ($"{header.Key}: {header.Value}\r\n");
         }
         ret += "\r\n";
         ret += _body;

         return ret;
      }

      /// <summary>
      /// Initializes all values from a string that contains the full request
      /// </summary>
      /// <param name="data"></param>
      private void ParseFromString(String data)
      {
         int i;
         // split request line
         i = data.IndexOf("\r\n");
         string requestLine = data.Substring(0, i);
         // split headers
         data = data.Substring(i + 2); // +2 to skip the \r\n
         i = data.IndexOf("\r\n\r\n");
         string headersFull = data.Substring(0, i);
         // split body
         data = data.Substring(i + 4); // +4 to skip the \n\r\n

         // parse request line
         var requestParams = requestLine.Split(' ');
         SetMethod(requestParams[0]);
         SetPath(requestParams[1]);
         SetVersion(requestParams[2]);


         // parse headers
         var headerList = headersFull.Split(new string[] { "\r\n" }, StringSplitOptions.None);
         foreach (var header in headerList)
         {
            var pair = header.Split(new string[] { ": " }, StringSplitOptions.None);
            this._headers.Add(pair[0], pair[1]);
         }

         // body
         this._body = data;
      }

      private void SetMethod(string verb)
      {
         switch (verb)
         {
            case "GET":
               _method = Method.GET;
               break;
            case "POST":
               _method = Method.POST;
               break;
            case "PUT":
               _method = Method.PUT;
               break;
            case "DELETE":
               _method = Method.DELETE;
               break;
            default:
               throw new HttpException(StatusCode.Bad_Request);
         }
      }
      private void SetPath(string path)
      {
         this._uri = path;
      }
      private void SetVersion(string v)
      {
         //if (v.Equals("HTTP/1.1\r")) Console.WriteLine("aaaaa");
         switch (v)
         {
            case "HTTP/0.9":
            case "HTTP/1.0":
            case "HTTP/1.1":
            case "HTTP/2.0":
               this._version = v;
               break;
            default:
               Console.WriteLine(v);
               throw new HttpException(StatusCode.Bad_Request);
         }
      }

      public string getPath() { return _uri; }
      public Method GetMethod() { return _method; }

      public string getBody() { return _body; }
   }
}
