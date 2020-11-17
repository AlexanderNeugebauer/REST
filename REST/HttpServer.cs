using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Sockets;

namespace REST
{
   [Flags]
   public enum Method
   {
      GET,
      POST,
      PUT,
      DELETE
   }

   public enum StatusCode
   {
      OK = 200,
      Bad_Request = 400,
      Not_Found = 404,
      Internal_Server_Error = 500
   }
   
   public class HttpServer
   {
      /// <summary>
      /// Container Class for all Endpoints. 
      /// Contains path, http verb and event
      /// </summary>
      public class Endpoints
      {
         // Endpoints.RegisterEndpoint("/messages/{msgNr}, Method.GET, 

         private Dictionary<string, Dictionary<Method, Func<RequestContext, ResponseContext>>> _endPoints;

         public Endpoints()
         {
            _endPoints = new Dictionary<string, Dictionary<Method, Func<RequestContext, ResponseContext>>>();
         }

         /// <summary>
         /// registers or alters an existing endpoint
         /// </summary>
         /// <param name="path">Starts with / ------ Can contain wildcard as * ------ e.g. /messages/* </param>
         /// <param name="verb">HTTP method</param>
         /// <param name="func">function handler</param>
         public void RegisterEndpoint(string path, Method verb, Func<RequestContext, ResponseContext> func)
         {
            if (_endPoints.ContainsKey(path))
            {
               if (_endPoints[path].ContainsKey(verb))
               {
                  _endPoints[path][verb] = func;
               }
               else
               {
                  _endPoints[path].Add(verb, func);
               }
            }
            else
            {
               _endPoints.Add(path, new Dictionary<Method, Func<RequestContext, ResponseContext>>());
               _endPoints[path].Add(verb, func);
            }
         }

         public void DeleteEndpoint(string path)
         {
            if (_endPoints.ContainsKey(path))
            {
               _endPoints.Remove(path);
            }
         }
         
         public void DeleteEndpoint(string path, Method verb)
         {
            if (_endPoints.ContainsKey(path) && _endPoints[path].ContainsKey(verb))
            {
               _endPoints[path].Remove(verb);
            }
         }

         public ResponseContext invokeEndpoint(RequestContext reqC)
         {
            var path = reqC.getPath();
            var verb = reqC.GetMethod();
            if (_endPoints.ContainsKey(path) && _endPoints[path].ContainsKey(verb))
            {
               return _endPoints[path][verb].Invoke(reqC);
            }
            var generalizedPath = path.Substring(0, path.LastIndexOf("/")) + "/*";
            if (_endPoints.ContainsKey(generalizedPath) && _endPoints[generalizedPath].ContainsKey(verb))
            {
               return _endPoints[generalizedPath][verb].Invoke(reqC);
            }          

            // no endpoint found
            throw new HttpException(StatusCode.Not_Found);
         }
      }

      private TcpListener _server;
      private Int32 _port;
      private IPAddress _localAddr;

      public Endpoints Endpoint { get; }

      public HttpServer()
      {
         _port = 80;
         _localAddr = IPAddress.Parse("127.0.0.1");
         _server = new TcpListener(_localAddr, _port);
         Endpoint = new Endpoints();
      }

      /// <summary>
      /// starts running the HTTP Server
      /// </summary>
      public void Run()
      {
         try
         {
            _server.Start();

            Byte[] bytes = new Byte[256];

            while (true)
            {
               Console.Write("Waiting for a connection... ");

               TcpClient client = _server.AcceptTcpClient();
               NetworkStream stream = client.GetStream();
               Console.WriteLine("Connected!");

               string data = "";

               // Loop to receive all the data sent by the client.
               do
               {
                  int i = stream.Read(bytes, 0, bytes.Length);
                  data += System.Text.Encoding.ASCII.GetString(bytes, 0, i); // Translate data bytes to a ASCII string.
               } while (stream.DataAvailable);

               // Process the data sent by the client.
               ResponseContext response;
               try
               {
                  RequestContext reqC = new RequestContext(data);
                  response = Endpoint.invokeEndpoint(reqC);
                  //Console.WriteLine(reqC.ToString());
               }
               catch (HttpException e)
               {
                  response = new ResponseContext(e.StatusCode);
               }
               catch (Exception)
               {
                  response = new ResponseContext(StatusCode.Internal_Server_Error);
               }

               // Send back a response.
               byte[] msg = System.Text.Encoding.ASCII.GetBytes(response.ToString());
               stream.Write(msg, 0, msg.Length);

               client.Close();
            }
         }
         catch (SocketException e)
         {
            Console.WriteLine("SocketException: {0}", e);
         }
         finally
         {
            // Stop listening for new clients.
            _server.Stop();
         }

         Console.WriteLine("\nHit enter to continue...");
         Console.Read();
      }

   }
}
