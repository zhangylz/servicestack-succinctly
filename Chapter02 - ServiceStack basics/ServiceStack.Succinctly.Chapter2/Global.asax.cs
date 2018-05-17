using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Funq;
using ServiceStack.Common;
using ServiceStack.Common.Web;
using ServiceStack.FluentValidation;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Validation;
using ServiceStack.WebHost.Endpoints;

namespace ServiceStack.Succinctly.Chapter2
{
    public class Global : HttpApplication
    {
        public class ServiceAppHost : AppHostBase
        {
            // minimum that has tobe configured.
            public ServiceAppHost()
                : base("Order Management", typeof (ServiceAppHost).Assembly)
            {
                //It’s possible to disable the file extension style by setting
                Config.AllowRouteContentTypeExtensions = false;
                //forcing the content type for every request to JSON by configuring the application host.
                Config.DefaultContentType = ContentType.Json;
                // Enabling the validation
                Plugins.Add(new ValidationFeature());
                Container.RegisterValidator(typeof(GetOrderValidator));

                //Fluent API
                // manual add routes.
                Routes
                    .Add<GetOrders>("/orders", "GET")
                    .Add<CreateOrders>("/orders", "POST")
                    //dynamic paths
                    .Add<GetOrder>("/orders/{Id}", "GET")
                    .Add<UpdateOrder>("/orders/{Id}", "PUT")
                    .Add<DeleteOrder>("/orders/{Id}", "DELETE");
                // Autoregistering the routes in the application host.
                //Routes.AddFromAssembly(typeof(OrderService).Assembly);
            }
            // minimum that has tobe configured.
            //All of the configuration can be done directly in the application host and declaring the objects is 
            //not very different than in any other framework.
            //All of the configuration can be done directly in the application host and declaring the objects is 
            //not very different than in any other framework.
            public override void Configure(Container container)
            {
                container.Register<IOrderRepository>(new OrderRepository());
                //container.Register<IProductRepository>(new ProductRepository());
                container.Register<IProductMapper>(x => new ProductMapper()).ReusedWithin(ReuseScope.Container);
                //  It is available at http://<servername>/metadata, and can be 
                //disabled by placing the following code in the application host’s Configure method.
                //SetConfig(new EndpointHostConfig
                //{
                //    EnableFeatures = Feature.All.Remove(Feature.Metadata)
                //});
            }
        }
        // minimum that has to be configured.
        protected void Application_Start(object sender, EventArgs e)
        {
            new ServiceAppHost().Init();
        }
    }

    // In the following code example, the validator would only be used in the case of GET or POST 
    //methods, and this is defined with the RuleSet method.RuleFor instead specifies the rule to be
    //applied for a given DTO property. As you will see, a customized error message can be
    //specified.
    public class GetOrderValidator : AbstractValidator<GetOrdersRequest>
    {
        public GetOrderValidator()
    {
        //Validation rules for GET request
        RuleSet(ApplyTo.Get |  ApplyTo.Post, () =>
            {
                RuleFor(x => x.Id)
                   .GreaterThan(2)   
                   .WithMessage("OrderID has to be greater than 2");
            });
    }
    }


}