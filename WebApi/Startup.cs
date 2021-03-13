using AutoMapper;
using Game.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using WebApi.Models;

namespace WebApi
{
    public class MyProfile : Profile
    {
        public MyProfile()
        {
            CreateMap<UserEntity, UserDto>()
                .ForMember(q => q.FullName, w => w.MapFrom(q => $"{q.LastName} {q.FirstName}"));

            CreateMap<UserForCreateDto, UserEntity>()
                // .ForMember(q => q.FirstName, w => w.MapFrom(q => q.FirstName))
                // .ForMember(q => q.LastName, w => w.MapFrom(q => q.LastName))
                // .ForMember(q => q.Id, w => w.MapFrom(q => q.Login))
                // .ForMember(q => q.Login, w => w.Ignore())
                // .ForMember(q => q.CurrentGameId, w => w.Ignore())
                // .ForMember(q => q.GamesPlayed, w => w.Ignore())
                ;

            CreateMap<UserForEdit, UserEntity>()
                ;
        }
    }

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
            services.AddAutoMapper(cfg => { cfg.AddProfile(new MyProfile()); }, new System.Reflection.Assembly[0]);
            services.AddControllers(options =>
                {
                    options.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                    options.ReturnHttpNotAcceptable = true;
                    options.RespectBrowserAcceptHeader = true;
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Populate;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                })
                .ConfigureApiBehaviorOptions(
                    options => { }
                );

            services.AddSingleton<Game.Domain.IUserRepository, Game.Domain.InMemoryUserRepository>();
            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                    options.SuppressMapClientErrors = true;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}