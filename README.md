Code samples for *ServiceStack Succinctly* by Zoran Maksimovic.

5-16. REST HTTP Methods 
The ServiceStack framework virtually supports all of the available HTTP methods. The web 
service method to be executed will be determined at run time by combining the routes and 
requested HTTP method. ServiceStack will execute a method of the service that corresponds to 
the actual HTTP method name. The HTTP verb GET will be executed by Get(), POST will be 
executed by Post(), and so on. 
GET POST PUT DELETE PATCH

Content Negotiation 
If not specified otherwise, ServiceStack offers mainly three ways of negotiating
of the resource: 
 The HTTP Accept header. 
 The format query string parameter. 
 The file extension (e.g., http://<servername>/orders.json). 
There are different ways of defining the response content type: 
	a.Forcing the content type for every request to JSON by configuring the application host. 
	  here global in Global.asax.cs
	public class ServiceAppHost : AppHostBase 
	{ 
		public ServiceAppHost()
		        : base("Order Management", typeof(ServiceAppHost).Assembly) 
		{ 
			base.Config.DefaultContentType = ContentType.Json;  
		} 
	}
	b.Specifying the content type at the service method’s level, either by using an AddHeader 
	filter or by specifying the ContentType. in service implemtnt. 
	[AddHeader(ContentType = ContentType.Json)] 
	public object Get(GetOrdersRequest request) { /*..*/}
	Or, alternatively: 
	public object Get(GetOrdersRequest request) 
	{ 
		base.Response.ContentType = ContentType.Json; 
		return /*..*/ 
	}
	Returning a decorated response via HttpResult. 
	public object Get(GetOrdersRequest request) 
	{ 
		return new HttpResult(responseDTO, ContentType.Json); 
	}

Routing 
Routing is the process of selecting paths along which to send a request. To determine which 
action to perform for a request, ServiceStack has to keep the list of routes and this has to be 
instructed specifically when the application starts. There are several ways in which the route can 
be registered: 
 Using a default route. 
 Creating a custom route by using RouteAttribute or Fluent API. 
 Dynamic paths. 
 Autoregistered paths.
Default Routes 
By default, for every Request DTO, ServiceStack will create a default route in the following form: 
 /api?/[xml|json|html|jsv|csv]/[reply|oneway]/[servicename] 
Let’s suppose we want to support a custom route, http://<servername>/orders, and we expose a 
Request DTO called GetOrders. In this case, without specifying any route, ServiceStack will 
create http://<servername>/xml/reply/GetOrders automatically. 
Custom Routes 
A route can be declared as a class attribute directly at the Request DTO level or in the 
AppHostBase by using the Fluent API. The route has an option to define one or more HTTP 
verbs. 
Route Attribute 
By using the RouteAttribute, a route can be declared directly at the Request DTO object. 

[Route("/orders", "GET POST", Notes="…", Summary="…")] 
public class GetOrders { } 
Fluent API 
Instead of using the RouteAttribute, the same can be achieved by defining the route in the 
application host declaration. 

public class ServiceAppHost : AppHostBase 
{ 
    public ServiceAppHost() 
        : base("Order Management", typeof(ServiceAppHost).Assembly) 
    { 
        Routes 
            .Add<GetOrders>("/orders", "GET") 
            .Add<CreateOrder>("/orders", "POST") 
            .Add<GetOrder>("/orders/{Id}", "GET") 
            .Add<UpdateOrder>("/orders/{Id}", "PUT") 
            .Add<DeleteOrder>("/orders/{Id}", "DELETE") 
    } 
} 

Autoregistered Paths 
By using the Routes.AddFromAssembly(typeof(OrderService).Assembly) method, we can 
automatically map and define all routes. 

Route Wildcards 
There is a way to define a wildcard in the route; this is especially useful when the route 
becomes too complex. 
The following is an example with a route that uses a wildcard. 

Routes.Add("/orders/{CreationDate}//{Others*}", "GET"); 
In this case, the request would be /orders/2012-11-12/SomeOther/InfoGoes/Here. 
This will be translated and deserialized as follows. 
CreationDate = 2012-11-12; Others = “SomeOther/InfoGoes/Here” 
So, the Others keyword will be taken as it is, and it can then be further processed in the 
application code. 

Validator 
In the following code example, the validator would only be used in the case of GET or POST 
methods, and this is defined with the RuleSet method. RuleFor instead specifies the rule to be 
applied for a given DTO property. As you will see, a customized error message can be 
specified. 
public class GetOrderValidator : AbstractValidator<GetOrdersRequest> 
{ 
    public GetOrderValidator() 
    { 
        //Validation rules for GET request. 
        RuleSet(ApplyTo.Get | ApplyTo.Post, () => 
            { 
                RuleFor(x => x.Id) 
                   .GreaterThan(2)    
                   .WithMessage("OrderID has to be greater than 2"); 
            }); 
    } 
} 
 
public class ServiceAppHost : AppHostBase 
{ 
    public ServiceAppHost() 
         : base("Order Management", typeof(ServiceAppHost).Assembly) 
{ 
    //Enabling the validation. 
    Plugins.Add(new ValidationFeature()); 
    Container.RegisterValidator(typeof(GetOrderValidator));  
} 
In case of an invalid request, such as GET/orders/1, the framework will return the following 
error (in cases where the format specified is XML). 

for example.
http://localhost:16431/orders/1
Failed to load resource: the server responded with a status of 400 (GreaterThan)

Metadata Page (default page) 
By default, when accessing the web application, ServiceStack exposes a metadata page. The 
metadata page contains information about the operations, available content types, Web 
Services Description Language (WSDL), and other objects exposed in the current application 
host. It is extremely useful because it acts as documentation for the available operations and 
used types (see Figure 5). 

disable metadata page
Global.asax.cs 
class ServiceAppHost:AppHostBase => Configure 
using ServiceStack.Common;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

SetConfig(new EndpointHostConfig
{
	EnableFeatures = Feature.All.Remove(Feature.Metadata)
});
