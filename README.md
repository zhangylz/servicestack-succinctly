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

Visual Studio Project Structure 
There are many ways to keep the assemblies separated, and this usually depends on the 
project’s needs. When working with services, I usually apply the following three rules: 
 The facade layer should be a thin layer without any business logic. 
 Keep the application implementation separated from the hosting application. This is very 
useful in case we want to reuse the application logic and expose it in a different way (e.g., 
as a desktop app, web service, or web application). 
 Keep the Request and Response DTOs in a separate assembly as this can be shared with 
the client (especially useful for .NET clients). 


ServiceStack.Succinctly.Host 
	ServiceStack.Succinctly.Host is the entry point of the application and the only component 
	that communicates with the client. It contains the exposed services and all of the necessary 
	plumbing, routing, instrumentation, etc. This is exactly where we are going to use the 
	ServiceStack framework functionalities. 
ServiceStack.Succinctly.ServiceInterface 
	ServiceStack.Succinctly.ServiceInterface contains Request and Response DTOs. This 
	library can be directly shared with the client application to ease the communication and 
	transformation of data. This library shouldn’t contain any application logic. 
OrderManagement.Core 
	OrderManagement.Core contains the domain model of the application and the required 
	business logic, application logic, etc. 
OrderManagement.DataAccessLayer 
	OrderManagement.DataAccessLayer contains the logic to access the database and the 
	related repositories. 
	
Chapter 4 Solution Configuration
Q1: ServiceStack EndpointHostConfig & WebHost does not exist? C# MVC
Advice: https://stackoverflow.com/questions/20431619/servicestack-endpointhostconfig-webhost-does-not-exist-c-sharp-mvc
as install servicestack， please execute command 
PM> Install-Package ServiceStack -Version 3.9.56     (packages.config)
if ServiceStack installed, please uninstall 
ex.  Uninstall-Package  ServiceStack.Text

Chapter 5 Service Implementation
For all three services, we will implement the following components: 
 Service Model (Request and Response DTO) object definitions. 
 Route specification. 
 Mapper(s) implementation. 
 Validator implementation. 
 Service implementation. 
 Configuration (application host) wiring everything together. 
   5.1  DTO objects
   ServiceStack.Succinctly.ServiceInterface
   defined class Status & Link(using System.Runtime.Serialization;)

	Removing Namespaces 
	To remove superfluous namespaces from XML returned objects, add the following code to the 
	Assembly.cs in the ServiceStack.Succinctly.ServiceInterface project. 

	[assembly: ContractNamespace("", ClrNamespace="ServiceStack.Succinctly.ServiceInte
	rface.OrderModel")] 
	[assembly: ContractNamespace("", ClrNamespace="ServiceStack.Succinctly.ServiceInte
	rface.OrderItemModel")] 
	[assembly: ContractNamespace("", ClrNamespace="ServiceStack.Succinctly.ServiceInte
	rface.ProductModel")]
	[assembly: ContractNamespace("", ClrNamespace="ServiceStack.Succinctly.ServiceInte
	rface")]
	
	Product Service
	namespace OrderManagement.Core.Domain class Product can be seen as a reference data class as it is a property of 
	the OrderItem class.
	
	Service Model
	As a particularity of  the service, I've chosen to create two separate DTOs for managing inserts and updates.
	ServiceStack.Succinctly.ServiceInterface.ProductModel
	CreateProduct DeleteProduct GetProduct UpdateProduct ProductResponse
	The GetProducts class intentionally does not have properties；it is mainly used for 
	namespace ServiceStack.Succinctly.ServiceInterface.ProductModel
	{
		public class GetProducts { }
	}
	
	
	Almost all of the Service methods will return the ProductResponse object. ProductResponse is 
	a mirror of the Product and, as we will see, it can contain more or less information. In our case, 
	it contains a list of Links and no information about the Product.Version, which is only used 
	for optimistic concurrency control. 
	ProductsResponse instead will hold a list of ProductResponse. This is helpful as we can reuse 
	this object and enrich it with further attributes that can be useful for paging, navigation, etc. 
	public class ProductResponse 
	{ 
		public int        Id     { get; set; } 
		public string     Name   { get; set; } 
		public Status     Status { get; set; } 
		public List<Link> Links  { get; set; } 
	} 
	 
	public class ProductsResponse 
	{ 
		public List<ProductResponse> Products { get; set; } 
	}
	
	5.2 Route Specification
	In the application host (Global.asax.cs), we need to register the various routes related to the 
	Product service. As we have seen in the previous chapters, this is done either in the application 
	host’s constructor or in the Configure method. I’ve chosen to use the constructor because I 
	want to use the Configure method only for the IoC-related items. 
	 
	public ServiceAppHost():                    base("Order Management", typeof (Servi
	ceAppHost).Assembly) 
	{ 
		Routes 
		  .Add<GetProducts>  ("/products",      "GET",    "Returns Products") 
		  .Add<GetProduct>   ("/products/{Id}", "GET",    "Returns a Product") 
		  .Add<CreateProduct>("/products",      "POST",   "Creates a Product") 
		  .Add<UpdateProduct>("/products/{Id}", "PUT",    "Updates a Product") 
		  .Add<DeleteProduct>("/products/{Id}", "DELETE", "Deletes a Product"); 
	}
	The ServiceStack Routes.Add() method currently doesn’t expose the method signature that 
	we have just seen. To achieve this, I’ve created an extension method. The Routes property 
	implements the IServiceRoute interface and, once we know this, it’s very easy to extend. 
	using ServiceStack.Succinctly.Host.Extensions; 
	public static class RoutesExtensions 
	{ 
		public static IServiceRoutes Add<T> (this IServiceRoutes routes,  
				   string restPath, string verbs, string summary) 
		{ 
			return routes.Add(typeof (T), restPath, verbs, summary, ""); 
		} 
	  
		public static IServiceRoutes Add<T>(this IServiceRoutes routes,  
				   string restPath, string verbs, string summary, string notes) 
		{ 
			return routes.Add(typeof(T), restPath, verbs, summary, notes); 
		} 
	} 
	
	5.3 Mapper(s) implementation. 
	Product Mapper Implementation 
	We need to map the data back and forth from the domain object model to the service object 
	model (DTOs). The best way to do so is to create a specific class that enables the application to 
	transform the Product domain object to ProductResponse and 
	CreateProduct/UpdateProduct to the Product. Because this can be quite a heavy workload 
	in the case of big classes, I advise you to use some specific libraries such as AutoMapper as 
	this would decrease the amount of necessary code. Explaining how AutoMapper works is 
	outside the scope of this book but, as we will see, it’s pretty intuitive and easy to use. 
	Adding the AutoMapper library to the project is as easy as running the following NuGet 
	command:  PM> Install-Package AutoMapper  
	AutoMapper中Mapper的CreateMap方法弃用解决方法
	https://blog.csdn.net/yzj_xiaoyue/article/details/62419152
	将以前使用的Mapper.CreateMap<Source,Dest>()方法改为Mapper.Initialize(x=>x.CreateMap<Source,Dest>()());即可。
	
	The ProductMapper implements the IProductMapper interface, which will make it easier to be 
	injected to the Service. Let’s create the following file in the ServiceStack.Succinctly.Host 
	project under the ServiceStack.Succinctly.Host.Mappers namespace. 
	The following code example shows the definition of the IProductMapper interface. 

	using System.Collections.Generic;
	using OrderManagement.Core.Domain;
	using ServiceStack.Succinctly.ServiceInterface.ProductModel;

	namespace ServiceStack.Succinctly.Host.Mappers
	{
		public interface IProductMapper
		{
			Product ToProduct(CreateProduct request);
			Product ToProduct(UpdateProduct request);
			ProductResponse ToProductResponse(Product product);
			List<ProductResponse> ToProductResponseList(List<Product> products);
		}
	}
	
	using System;
	using System.Collections.Generic;
	using AutoMapper;
	using ServiceStack.Text;
	using OrderManagement.Core.Domain;
	using SrvObjType = ServiceStack.Succinctly.ServiceInterface;
	using SrvObj = ServiceStack.Succinctly.ServiceInterface.ProductModel;

	namespace ServiceStack.Succinctly.Host.Mappers
	{
		public class ProductMapper : IProductMapper
		{
			static ProductMapper()
			{
				//AutoMapper 6.2.2 desperate CreateMap insteaded of Mapper.Initialize(x=>x.CreateMap<Source,Dest>());
				//Mapper.CreateMap<SrvObjType.Status, Status>();
				//Mapper.CreateMap<Status, SrvObjType.Status>();
				//Mapper.CreateMap<SrvObj.CreateProduct, Product>();
				//Mapper.CreateMap<SrvObj.UpdateProduct, Product>();
				//Mapper.CreateMap<Product, SrvObj.ProductResponse>();
				Mapper.Initialize(x => x.CreateMap<SrvObjType.Status, Status>());
				Mapper.Initialize(x => x.CreateMap<Status, SrvObjType.Status>());
				Mapper.Initialize(x => x.CreateMap<SrvObj.CreateProduct, Product>());
				Mapper.Initialize(x => x.CreateMap<SrvObj.UpdateProduct, Product>());
				Mapper.Initialize(x => x.CreateMap<Product, SrvObj.ProductResponse>());
			}

			public Product ToProduct(SrvObj.CreateProduct request)
			{
				return Mapper.Map<Product>(request);
			}

			public Product ToProduct(SrvObj.UpdateProduct request)
			{
				return Mapper.Map<Product>(request);
			}

			public SrvObj.ProductResponse ToProductResponse(Product product)
			{
				var productResponse = Mapper.Map<SrvObj.ProductResponse>(product);

				productResponse.Links = new List<SrvObjType.Link>
					{
						new SrvObjType.Link
							{
								Title = "self",
								Rel = "self",
								Href = "products/{0}".Fmt(product.Id),
							}
					};
				return productResponse;
			}

			//Transforms a list of products into a list of ProductResponse
			public List<SrvObj.ProductResponse> ToProductResponseList(List<Product> products)
			{
				var productResponseList = new List<SrvObj.ProductResponse>();
				products.ForEach(x => productResponseList.Add(ToProductResponse(x)));
				return productResponseList;
			}
		}
	}	
		   
	Note: All of our services will have a Mapper class by design because we want to separate 
	the Request and Response DTOs (service model) from the application domain model. 
	
	5.4 Validator implementation.
	As we saw in the previous chapter, we can create some custom validation logic. We will create 
	some in this case because we want to make our implementation a bit stronger. For our current 
	example, we will just make sure that when the Product is created or updated, we check that the 
	Name property is set and is not longer than 50 characters. We will create the following two 
	classes in the ServiceStack.Succinctly.Host.Validation namespace, and will register 
	those two validators in the application host. 
	
	namespace ServiceStack.Succinctly.Host.Validation
	{
		public class CreateProductValidator : AbstractValidator<CreateProduct> 
		{ 
			public CreateProductValidator() 
			{ 
				string nameNotSpecifiedMsg = "Name has not been specified."; 
				string maxLenghtMsg = "Name cannot be longer than 50 characters."; 
		 
				RuleFor(r => r.Name) 
					.NotEmpty().WithMessage(nameNotSpecifiedMsg) 
					.NotNull().WithMessage(nameNotSpecifiedMsg) 
					.Length(1, 50).WithMessage(maxLenghtMsg); 
			} 
		}
		  
		public class UpdateProductValidator : AbstractValidator<UpdateProduct> 
		{ 
			public UpdateProductValidator() 
			{ 
				string nameNotSpecifiedMsg = "Name has not been specified."; 
				string maxLenghtMsg = "Name cannot be longer than 50 characters."; 
		 
				RuleFor(r => r.Name) 
					.NotEmpty().WithMessage(nameNotSpecifiedMsg) 
					.NotNull().WithMessage(nameNotSpecifiedMsg) 
					.Length(1, 50).WithMessage(maxLenghtMsg); 
			} 
		}
	}
	Application Host Configuration    
	The following code shows the full implementation of the application host. 
	public class ServiceAppHost : AppHostBase 
	{ 
	  public ServiceAppHost() 
			: base("Order Management", typeof(ServiceAppHost).Assembly) 
	   { 
		Routes 
		.Add<GetProducts>  ("/products", "GET", "Returns a collection of Products") 
		.Add<GetProduct>   ("/products/{Id}", "GET", "Returns a single Product") 
		.Add<CreateProduct>("/products", "POST", "Create a product") 
		.Add<UpdateProduct>("/products/{Id}", "PUT", "Update a product") 
		.Add<DeleteProduct>("/products/{Id}", "DELETE", "Deletes a product") 
		.Add<DeleteProduct>("/products", "DELETE", "Deletes all products"); 
									
		 Plugins.Add(new ValidationFeature()); 
	   }  
	 
		public override void Configure(Container container) 
		{ 
			container.Register<IProductRepository>(new ProductRepository()); 
			container.Register<IProductMapper>(new ProductMapper()); 
					 
			container.RegisterValidator(typeof(CreateProductValidator)); 
			container.RegisterValidator(typeof(UpdateProductValidator)); 
		} 
	} 
	
	5.5 Service Implementation
	ProductService implements two Get methods, one Post, one Put, and one Delete. Every 
	ServiceStack service has to inherit from the ServiceStack.ServiceInterface.Service 
	class.
	As shown in the following code example, ProductService has two properties: ProductMapper 
	and ProductRepository, which will hold the instances that will be injected at run time by the 
	IoC container.
	
	namespace ServiceStack.Succinctly.Host.Services
	public class ProductService : ServiceStack.ServiceInterface.Service 
	{        
		public IProductMapper ProductMapper { get; set; } 
		public IProductRepository ProductRepository { get; set; } 
	 
		public ProductResponse Get(GetProduct request){…} 
		public List<ProductResponse> Get(GetProducts request){} 
		public ProductResponse Post(CreateProduct request){…} 
		public ProductResponse Put(UpdateProduct request) {…}
		public HttpResult Delete(DeleteProduct request){…} 
	}
	