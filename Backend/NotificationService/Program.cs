using BuildingBlocks.Messaging;
using NotificationService.Messaging;
using NotificationService.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddSingleton<IHtmlEmailTemplateRenderer, HtmlEmailTemplateRenderer>();
builder.Services.AddSingleton<IEmailSender, MailKitEmailSender>();
builder.Services.AddHostedService<EmailNotificationConsumer>();

var host = builder.Build();
host.Run();
