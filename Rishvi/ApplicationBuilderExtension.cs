using Hangfire;

namespace Rishvi
{
    public static class ApplicationBuilderExtension
    {
        public static void Configure(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            //app.UseAuthorization();
            app.UseStaticFiles();

            if (!env.IsDevelopment())
            {
                //app.UseHangfireServer();
                //app.UseHangfireDashboard("/hangfire", new DashboardOptions
                //{
                //    //IsReadOnlyFunc = (DashboardContext context) => true,
                //    Authorization = new[] { new CustomHangfireAuthorizeFilter() },
                //});
            }

            app.UseSwagger();
            if (env.IsDevelopment())
                app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Rishvi API Services"));
            else
                app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Rishvi API Services"));


            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "all",
            //        template: "{*query}",
            //        defaults: new { controller = "Home", action = "Index" });
            //});
        }
    }
}
