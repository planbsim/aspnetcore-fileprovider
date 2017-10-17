using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Reflection;

namespace FileProvider
{
    public class Startup
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        private IFileProvider _physicalFileProvider;

        public Startup(IHostingEnvironment env)
        {
            _hostingEnvironment = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // FIRST: use the physical file provider from the hosting environment 
            //services.AddSingleton<IFileProvider>(_hostingEnvironment.ContentRootFileProvider);

            // SECOND: create a new physical file provider from the hosting environment´s root path
            //services.AddSingleton<IFileProvider>(new PhysicalFileProvider(_hostingEnvironment.ContentRootPath));

            // THIRD: combine to only one directory, for example wwwroot
            string path = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot");
            _physicalFileProvider = new PhysicalFileProvider(path);

            // FOURTH: use files embedded in assemblies, for example the entry assembly itself  
            var embeddedFileProvider = new EmbeddedFileProvider(Assembly.GetEntryAssembly());

            //services.AddSingleton<IFileProvider>(_physicalFileProvider);
            services.AddSingleton<IFileProvider>(new CompositeFileProvider(_physicalFileProvider, embeddedFileProvider));

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //_hostingEnvironment = env;


            app.UseStaticFiles(new StaticFileOptions
            {
                RequestPath = "",
                //RequestPath = "/content",
                FileProvider = _physicalFileProvider,
            });

            app.UseMvcWithDefaultRoute();
        }
    }
}
