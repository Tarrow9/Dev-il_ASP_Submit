using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using bangsoo.Data;
using System.Text;
// For AddIdentity
using Microsoft.Extensions.DependencyInjection;
using bangsoo.Models;
using Microsoft.AspNetCore.Identity;
// For JWT
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
// .NET 5 이전 Startup.cs 파일의 ConfigureServices 메서드의 코드
// services.AddXXX() 형태는 builder.Services.AddXXX() 형태로 변경해서 작성

// UseLazyLoadingProxiex() : 외래키 Lazy Loading 사용하기 위해
builder.Services.AddDbContext<bangsooContext>(options =>
    options.UseLazyLoadingProxies().UseSqlServer(builder.Configuration.GetConnectionString("bangsooContext") ?? throw new InvalidOperationException("Connection string 'bangsooContext' not found.")));


builder.Services.AddIdentity<Users, Roles>()
        .AddEntityFrameworkStores<bangsooContext>()
        .AddDefaultTokenProviders();

// JWT인증 구성
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer("Bearer", options => {
            options.TokenValidationParameters = new TokenValidationParameters {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "dev-il.kr",
                ValidAudience = "dev-il.kr",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("****Censored****"))
            };
            // options.Events = new JwtBearerEvents {
            //     OnMessageReceived = context => {
            //         context.Token = context.Request.Cookies["Access-Token"];
            //         return Task.CompletedTask;
            //     }
            // };
        });

var policy = new AuthorizationPolicyBuilder("Identity.Application", "Bearer")
.RequireAuthenticatedUser().Build();

builder.Services.AddAuthorization(m => m.DefaultPolicy = policy);



// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddSwaggerGen(c => {
    var securitySchema = new OpenApiSecurityScheme {
        Description = "다음과 같은 형식으로 JWT Authorization header에 토큰을 보내도록 합니다.<br /> \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        Reference = new OpenApiReference {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    var securityRequirement = new OpenApiSecurityRequirement {
        { securitySchema, new[] { "Bearer" } }
    };
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "myapi", Version = "v1" });
    c.AddSecurityDefinition("Bearer", securitySchema);
    c.AddSecurityRequirement(securityRequirement);
});








var app = builder.Build();

// .NET 5 이전 Startup.cs 파일의 Configure 메서드의 코드
// app.UseXXX() 형태의 코드를 그대로 사용 가능

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// JWT 추가
app.UseAuthentication();
app.UseAuthorization();
//세션과 쿠키 사용시
//app.UseCookeiPolicy();
//app.UseSession();


// Swagger 추가
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "json menual");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
