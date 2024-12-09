using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using FactCheck1.Data;
using FactCheck1.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;
using Serilog.Events;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json")
    .Build();

builder.Host.UseSerilog((hostContext, services, conf) =>
{
    conf.MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .ReadFrom.Configuration(configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

Log.Logger.Information("App started...");
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();
builder.Services.AddDbContextFactory<FactcheckContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IPromptFetcher, PromptFetcher>();
builder.Services
    .AddBlazorise( options =>
    {
        options.Immediate = true;
    } )
    .AddBootstrapProviders()
    .AddFontAwesomeIcons();
builder.Services.AddScoped<DbService>();
builder.Services.AddSingleton<SharedArticles>();
builder.Services.AddScoped<IPromptFetcher, PromptFetcher>();

builder.Services.AddSession((Action<SessionOptions>) (options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30.0);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
}));
builder.Services.AddQuartz(q =>
{
//    q.UseMicrosoftDependencyInjectionJobFactory();
    string name1 = "rssJob";
    JobKey jobKey = new JobKey(name1);
    string key1 = "Quartz:" + name1;
    string cronSchedule = builder.Configuration.GetValue<string>(key1) ?? "0/5 * * * * ?";
    
    q.AddJob<RSSDownloadJob>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey) 
        .WithIdentity("rssJob-trigger") 
        .WithCronSchedule(cronSchedule)); 
    string name2 = "bardJob";
    JobKey bardJobKey = new JobKey(name2);
    string key2 = "Quartz:" + name2;
    string bardCronSchedule = builder.Configuration.GetValue<string>(key2) ?? "0 * * * * ?";
    
    q.AddJob<BardJob>(opts => opts.WithIdentity(bardJobKey));
    q.AddTrigger(opts => opts
        .ForJob(bardJobKey) 
        .WithIdentity("bardJobKey-trigger") 
        .WithCronSchedule(bardCronSchedule)); 
    
});
builder.Services.AddQuartzHostedService(opt =>
{
    opt.WaitForJobsToComplete = true;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();