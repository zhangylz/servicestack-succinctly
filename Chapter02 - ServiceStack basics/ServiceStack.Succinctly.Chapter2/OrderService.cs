using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace ServiceStack.Succinctly.Chapter2
{
    //Historically, there are serveral ways in which the web service can be created. Interfaces used in earlier versions of ServiceStack used IRestService and IService<T>.
    //However, currently the recommended way is to use ServiceStack.ServiceInterface.Service,which is the approach used in this book.
    public class OrderService : ServiceStack.ServiceInterface.Service
    {
        public IOrderRepository OrderRepository { get; set; }
        private IProductRepository _productRepository { get; set; }

        public OrderService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        //specifying the content type at the service method's level,either by using an AddHeader filter or
        //by specifying the Contenttype.
        [AddHeader(ContentType = ContentType.Json)]
        public object Get(GetOrdersRequest request)
        {
            //alternatively
            //base.Response.ContentType = ContentType.Json;

            //value = json
            var pageQueryStringValue = this.Request.QueryString["page"];

            //value = application/json
            var acceptHeaderValue = this.Request.Headers["Accept"];

            //value = GET
            var httpMethod = this.Request.HttpMethod;

            //Setting up the response.
            this.Response.ContentType = ContentType.Json;
            //Response represents the object that the client will eventually receive.
            //Among others，one of the most useful methods exposed by the Response object is the AddHeader method，
            //which manipulates the headers returned to the clint.
            this.Response.AddHeader("Location", "http://<servername>/orders");
            return new HttpResult { StatusCode = HttpStatusCode.OK };
            //Returning a decorated response via HttpResult.
            //return new HttpResult(resonseDTO, ContentType.Json);
        }
        // web service method return types
        //DTO data transfer object,between Design Patterns 
        /**************************************************************************************
         *A web service method can return one of the following: 
       *Response DTO object serialized to the response type (JSON, XML, PNG). 
       *Any basic .NET value. 
       *HttpResult: Used whenever full control of what client recieves is needed. 
       *HttpError: Used to return the error message to the client. 
       *CompressedResult (IHttpResult) for a customized HTTP response.
         */
        //The following two methods both produce the same result.
        //public List<GetOrderResponse> Get(GetOrdersRequest request)
        //{
        //    return new List<GetOrderResponse>();
        //}

        //public HttpResult Get(GetOrdersRequest request)
        //{
        //    return new HttpResult(response: new List<GetOrderResponse>(),
        //                          contentType: "application/json",
        //                          statusCode: HttpStatusCode.OK);
        //}
        public object Post(CreateOrders request)
        {
            return null;
        }
    }

    [Route("/orders", "GET")]
    [Route("/orders/{Id}", "GET")]
    // Request DTO (notic there is no route defined as the Attribute!)
    public class GetOrdersRequest
    {
        public int Id { get; set; }
    }

    public class GetOrderResponse
    {
    }

    public interface IOrderRepository
    {

    }
    public class OrderRepository : IOrderRepository
    {

    }

    public interface IProductRepository
    {

    }

    public interface IProductMapper
    {
    }

    public class ProductMapper : IProductMapper
    {
    }

    public class ProductRepository : IProductRepository
    {

    }

    //By using the RouteAttribute, a route can be declared directly at the Request DTO object.
    [Route("/orders", "GET POST", Notes = "...", Summary = "...")]
    public class GetOrders { }

    public class CreateOrders{}
    public class GetOrder {
        public string Id { get; set; }
    }
    public class UpdateOrder { }
    public class DeleteOrder { }
    public class CreateOrder { }
    public interface IContainerAdapter
    {
        T Resolve<T>();
        T TryResolve<T>();
    }
}