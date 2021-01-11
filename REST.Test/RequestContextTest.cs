using NUnit.Framework;
using REST;

namespace REST.Test
{
   public class RequestContextTest
   {
      string _request, _method, _path, _body;
      RequestContext request;
      [SetUp]
      public void Setup()
      {
         _method = "POST";
         _path = "/messages";
         _body = "testbody";
         _request =
@$"{_method} {_path} HTTP/1.1
Content-Type: text/plain
User-Agent: PostmanRuntime/7.26.5
Accept: */*
Cache-Control: no-cache
Postman-Token: c276cb07-7cb5-484e-9166-c8ccca488f6a
Host: 127.0.0.1
Accept-Encoding: gzip, deflate, br
Connection: keep-alive
Content-Length: 4

{_body}";
         request = new RequestContext(_request);
      }

      [Test]
      public void ToStringTest()
      {
         Assert.AreEqual(request.ToString(), _request);
      }

      [Test]
      public void getBodyTest()
      {
         Assert.AreEqual(request.Body, _body);
      }

      [Test]
      public void GetMethodTest()
      {
         Assert.AreEqual(request.GetMethod().ToString(), _method);
      }

      [Test]
      public void getPathTest()
      {
         Assert.AreEqual(request.getPath(), _path);
      }
   }
}