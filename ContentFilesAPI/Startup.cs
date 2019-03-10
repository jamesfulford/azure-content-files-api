using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ContentFilesAPI {

    /// <summary>
    /// Defines the configuration of the Web API application
    /// </summary>
    public class Startup {
        private string Title = "ContentFiles";
        private string Version = "v1";

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="hostingEnvironment">The hosting environment.</param>
        public Startup (IConfiguration configuration, IHostingEnvironment hostingEnvironment) {
            HostingEnvironment = hostingEnvironment;
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the hosting environment.
        /// </summary>
        /// <value>
        /// The hosting environment.
        /// </value>
        public IHostingEnvironment HostingEnvironment { get; }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices (IServiceCollection services) {
            services.AddMvc ().SetCompatibilityVersion (CompatibilityVersion.Version_2_1);

            services.Configure<ApiBehaviorOptions> (options => {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddSwaggerGen (ConfigureSwaggerUI);

            services.AddOptions ();

            services.AddSingleton<IConfiguration> (Configuration);
        }

        /// <summary>
        /// Configures the swagger UI in middleware.
        /// </summary>
        /// <param name="swaggerGenOptions">The swagger gen options.</param>
        private void ConfigureSwaggerUI (SwaggerGenOptions swaggerGenOptions) {
            swaggerGenOptions.SwaggerDoc (Version, new Info { Title = Title, Version = Version });

            var filePath = Path.Combine (HostingEnvironment.ContentRootPath, $"{Title}.config");
            swaggerGenOptions.IncludeXmlComments (filePath);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        public void Configure (IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            } else {
                app.UseHsts ();
            }

            app.UseHttpsRedirection ();
            app.UseMvc ();

            // SWAGGER: Insert middleware to expose the generated Swagger as JSON endpoints
            app.UseSwagger (c => {
                c.PreSerializeFilters.Add ((swaggerDoc, httpReq) => {
                    // Necessary for API management so it has the proper values for the Backend service URL otherwise
                    // you will see an error in trace similar to "Backend service URL is not defined"
                    swaggerDoc.Host = httpReq.Host.Value;
                });
            });
            app.UseSwaggerUI (c => {
                c.SwaggerEndpoint ($"/swagger/{Version}/swagger.json", Title);
            });

        }
    }
}
