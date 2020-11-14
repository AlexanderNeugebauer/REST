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
   class HttpServer
   {
      private TcpListener _server;
      private Int32 _port;
      private IPAddress _localAddr;

      public HttpServer()
      {
         _port = 80;
         _localAddr = IPAddress.Parse("127.0.0.1");
         _server = new TcpListener(_localAddr, _port);

      }

      /// <summary>
      /// starts running the HTTP Server
      /// </summary>
      public void Run()
      {
         try
         {
            // Start listening for client requests.
            _server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[256];
            String data;

            // Enter the listening loop.
            while (true)
            {
               Console.Write("Waiting for a connection... ");

               // Perform a blocking call to accept requests.
               // You could also use server.AcceptSocket() here.
               TcpClient client = _server.AcceptTcpClient();
               Console.WriteLine("Connected!");

               data = "";

               // Get a stream object for reading and writing
               NetworkStream stream = client.GetStream();

               int i;

               // Loop to receive all the data sent by the client.

               do
               {
                  i = stream.Read(bytes, 0, bytes.Length);
                  // Translate data bytes to a ASCII string.
                  data += System.Text.Encoding.ASCII.GetString(bytes, 0, i);
               } while (stream.DataAvailable);
               // Process the data sent by the client.
               try
               {
                  RequestContext rc = new RequestContext(data);
                  Console.WriteLine(rc.ToString());

                  byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                  stream.Write(msg, 0, msg.Length);
               }
               catch (Exception e)
               {
                  throw;
               }
               
               // Send back a response.
               // Shutdown and end connection
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
