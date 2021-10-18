using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace HttpConnectionDisposalIssue
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _ = services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _ = app.UseStaticFiles();
            _ = app.UseWebSockets();
            _ = app.UseRouting();
            
            _ = app.Use(async (context, next) =>
            {

                Console.WriteLine(context?.User?.Identity?.Name);
                Debug.Assert(context?.User?.Identity is WindowsIdentity,"This must be run in a context where context.user.identity is a windowsIdentity for the failure to occur");
                await next.Invoke();
                // Do logging or other work that doesn't write to the Response.
                //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-5.0#create-a-middleware-pipeline-with-iapplicationbuilder-1
                if (context.GetEndpoint()?.DisplayName == "TestHub_route")
                {
                    try
                    {

                        string name = context?.User?.Identity?.Name;
                        Console.WriteLine(name);
                    }
                    catch (ObjectDisposedException ex)
                    {
                        Debug.Assert(false, "this should not be reached");
                    }
                }
            });

            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapHub<TestHub>("/testhub").WithDisplayName("TestHub_route");
            });
        }
    }
}
